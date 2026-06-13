using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Application.Helpers;
using Timebash.Core.Contracts;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.DTOs.Shared;
using Timebash.Core.Exceptions;
using Timebash.Core.Extensions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;

namespace Timebash.Application.Services;

public class JournalService(
    IUnitOfWork unitOfWork,
    IJournalRepository journalRepository,
    IActivityRepository activityRepository,
    IUserSettingsRepository userSettingsRepository) : IJournalService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IJournalRepository _journalRepository = journalRepository;
    private readonly IActivityRepository _activityRepository = activityRepository;
    private readonly IUserSettingsRepository _userSettingsRepository = userSettingsRepository;

    public async Task<JournalResponse> GetByIdAsync(Guid id, Guid userId)
        => (await EntityAccessGuard.EnsureJournalAccessAsync(_journalRepository, id, userId)).ToResponse();

    public async Task<ActivitiesListResponse> GetActivitiesByJournalIdAsync(
        Guid id,
        Guid userId,
        DateTime? date = null, 
        int? offsetMinutes = null)
    {
        await EntityAccessGuard.EnsureJournalAccessAsync(_journalRepository, id, userId);
        var activities = date is null
            ? await _activityRepository.GetByJournalIdAsync(id)
            : await GetByJournalIdAsync(id, date.Value, offsetMinutes);

        return new ActivitiesListResponse(
            [.. activities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]
        );
    }

    public async Task<JournalResponse> CreateAsync(JournalRequest journalRequest, Guid userId)
    {
        var journal = journalRequest.ToJournal(Guid.NewGuid(), userId);

        _journalRepository.Add(journal);
        await _unitOfWork.SaveChangesAsync();

        return journal.ToResponse();
    }

    public async Task<bool> UpdateAsync(Guid id, JournalRequest journalRequest, Guid userId)
    {
        var journal = await EntityAccessGuard.EnsureJournalAccessAsync(_journalRepository, id, userId);
        if (!journal.ApplyUpdate(journalRequest)) return false;
        
        journal.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var journal = await EntityAccessGuard.EnsureJournalAccessAsync(_journalRepository, id, userId);
        var userSettings = await _userSettingsRepository.GetByIdAsync(journal.UserId) ?? throw new NotFoundException();
        if (userSettings.DefaultJournalId == id) throw new ConflictException("Unpossible to delete the default journal");

        _journalRepository.Delete(journal);
        await _unitOfWork.SaveChangesAsync();

        return;
    }

    public async Task<ConflictCorrectionsListResponse> GetTimeCorrectionConflictsAsync(Guid id, DateTime startTime, DateTime endTime, Guid userId)
    {
        await EntityAccessGuard.EnsureJournalAccessAsync(_journalRepository, id, userId);

        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();

        var conflictCorrections = new List<ConflictCorrectionInfo>();
        var isPreviousAdjusted = false;
        var isNextAdjusted = false;

        foreach (var overlap in await _activityRepository.GetOverlappingActivitiesAsync(id, truncatedStart, truncatedEnd))
        {
            CorrectionOptionBase? currentActivityCorrection = null;
            CorrectionOptionBase? addedActivityCorrection = null;

            if (truncatedStart < overlap.StartTime && overlap.EndTime < truncatedEnd)
            {
                currentActivityCorrection = new DeleteCorrection();
                addedActivityCorrection = new SplitCorrection([new(truncatedStart, overlap.StartTime), new(overlap.EndTime, truncatedEnd)]);
            }
            else if (truncatedStart == overlap.StartTime && overlap.EndTime <= truncatedEnd)
            {
                currentActivityCorrection = new ShiftCorrection(overlap.StartTime, overlap.StartTime);
                addedActivityCorrection = new ShiftCorrection(overlap.EndTime, truncatedEnd);
            }
            else if (truncatedStart < overlap.StartTime && overlap.EndTime == truncatedEnd)
            {
                currentActivityCorrection = new ShiftCorrection(overlap.EndTime, overlap.EndTime);
                addedActivityCorrection = new ShiftCorrection(truncatedStart, overlap.StartTime);
            }
            else if (overlap.StartTime < truncatedStart && truncatedEnd < overlap.EndTime)
            {
                currentActivityCorrection = new SplitCorrection([new(overlap.StartTime, truncatedStart), new(truncatedEnd, overlap.EndTime)]);
                addedActivityCorrection = new DeleteCorrection();

                isPreviousAdjusted = true;
                isNextAdjusted = true;
            }
            else if (overlap.StartTime < truncatedStart && truncatedStart < overlap.EndTime)
            {
                currentActivityCorrection = new ShiftCorrection(overlap.StartTime, truncatedStart);
                addedActivityCorrection = new ShiftCorrection(overlap.EndTime, truncatedEnd);

                isPreviousAdjusted = true;
            }
            else if (overlap.StartTime < truncatedEnd && truncatedEnd < overlap.EndTime)
            {
                currentActivityCorrection = new ShiftCorrection(truncatedEnd, overlap.EndTime);
                addedActivityCorrection = new ShiftCorrection(truncatedStart, overlap.StartTime);

                isNextAdjusted = true;
            }

            if (currentActivityCorrection is not null && addedActivityCorrection is not null)
            {
                conflictCorrections.Add(new
                (
                    overlap.Id,
                    overlap.Name,
                    overlap.StartTime,
                    overlap.EndTime,
                    currentActivityCorrection,
                    addedActivityCorrection
                ));
            }
        }

        if (!isPreviousAdjusted)
        {
            var previous = await _activityRepository.GetPreviousActivityAsync(id, truncatedStart);
            if (previous is not null && previous.EndTime != truncatedStart)
            {
                conflictCorrections.Insert(0, new
                (
                    previous.Id,
                    previous.Name,
                    previous.StartTime,
                    previous.EndTime,
                    new ShiftCorrection(previous.StartTime, truncatedStart),
                    new ShiftCorrection(previous.EndTime, truncatedEnd)
                ));
            }
        }

        if (!isNextAdjusted)
        {
            var next = await _activityRepository.GetNextActivityAsync(id, truncatedEnd);
            if (next is not null && next.StartTime != truncatedEnd)
            {
                conflictCorrections.Add(new
                (
                    next.Id,
                    next.Name,
                    next.StartTime,
                    next.EndTime,
                    new ShiftCorrection(truncatedEnd, next.EndTime),
                    new ShiftCorrection(truncatedStart, next.StartTime)
                ));
            }
        }

        return new (conflictCorrections);
    }

    private static (DateTime startUtc, DateTime endUtc) GetDayRange(DateTime date, int? offsetMinutes)
    {
        var startUtc = DateTime.SpecifyKind(
            offsetMinutes is null ? date : date.AddMinutes(offsetMinutes.Value),
            DateTimeKind.Utc);
        return (startUtc, startUtc.AddDays(1));
    }

    private async Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid id, DateTime date, int? offsetMinutes)
    {
        var (startUtc, endUnc) = GetDayRange(date, offsetMinutes);
        return await _activityRepository.GetByJournalIdAsync(id, startUtc, endUnc);
    }
}
