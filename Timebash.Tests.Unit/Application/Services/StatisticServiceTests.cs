using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Application.Services;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;

namespace Timebash.Tests.Unit.Application.Services
{
    public class StatisticServiceTests
    {
        private static readonly Faker _faker = new();
        private readonly StatisticService _service;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJournalRepository> _journalRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IActivityQueryService> _activityQueryServiceMock;

        public StatisticServiceTests()
        {
            _userRepositoryMock = new();
            _journalRepositoryMock = new();
            _categoryRepositoryMock = new();
            _activityQueryServiceMock = new();

            _service = new(
                _userRepositoryMock.Object,
                _journalRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _activityQueryServiceMock.Object);
        }

        [Fact]
        public async Task GetUserAggregateStatisticAsync_EmptyId_ShouldThrowBadRequest()
            => await FluentActions
                .Awaiting(() => _service.GetUserAggregateStatisticAsync(Guid.Empty, null, null))
                .Should()
                .ThrowAsync<BadRequestException>();

        [Fact]
        public async Task GetUserAggregateStatisticAsync_UserNotFound_ShouldThrowNotFound()
        {
            var id = Guid.NewGuid();
            _userRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

            await FluentActions
                .Awaiting(() => _service.GetUserAggregateStatisticAsync(id, null, null))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetJournalStatisticAsync_EmptyId_ShouldThrowBadRequest()
            => await FluentActions
                .Awaiting(() => _service.GetJournalStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
                .Should()
                .ThrowAsync<BadRequestException>();

        [Fact]
        public async Task GetJournalStatisticAsync_JournalNotFound_ShouldThrowNotFound()
        {
            var id = Guid.NewGuid();
            _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Journal?)null);

            await FluentActions
                .Awaiting(() => _service.GetJournalStatisticAsync(id, null, null, Guid.NewGuid()))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetCategoryStatisticAsync_EmptyId_ShouldThrowBadRequest()
            => await FluentActions
                .Awaiting(() => _service.GetCategoryStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
                .Should()
                .ThrowAsync<BadRequestException>();

        [Fact]
        public async Task GetCategoryStatisticAsync_CategoryNotFound_ShouldThrowNotFound()
        {
            var id = Guid.NewGuid();
            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Category?)null);

            await FluentActions
                .Awaiting(() => _service.GetCategoryStatisticAsync(id, null, null, Guid.NewGuid()))
                .Should()
                .ThrowAsync<NotFoundException>();
        }
    }
}