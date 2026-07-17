using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public class GetByIdAsyncTests : CategoryServiceTestsBase
{
    [Fact]
    public async Task GetById_ValidAccess_ShouldReturnResponse()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000")
        {
            Keywords = [.. Faker.Lorem.Words()]
        };
        var expected = category.ToResponse();

        SetupEnsureAccess(category);

        var result = await Service.GetByIdAsync(category.Id, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyEnsureAccessCalled(category.Id, category.UserId);
    }

    [Fact]
    public async Task GetById_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task GetById_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyEnsureAccessCalled(id, userId);
    }
}
