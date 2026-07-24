using FluentAssertions;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public class UpdateAsyncTests : CategoryServiceTestsBase
{
    [Fact]
    public async Task Update_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000")
        {
            Keywords = [.. Faker.Lorem.Words()]
        };
        var request = new CategoryRequest(Faker.Lorem.Word(), "#111111", [.. Faker.Lorem.Words()]);

        var currentUpdatedTime = category.UpdatedAt;
        var expected = new Category(category.Id, category.UserId, category.Name, category.Color)
        {
            Keywords = category.Keywords
        };
        expected.ApplyUpdate(request);

        AccessServiceMock.SetupEnsureAccess(category);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateAsync(category.Id, request, category.UserId, CancellationToken.None);

        result.Should().BeTrue();
        category.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        category.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(category => category.UpdatedAt)
                .Excluding(category => category.CreatedAt));

        AccessServiceMock.VerifyEnsureAccessCalled(category.Id, category.UserId);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Update_NoChanges_ShouldReturnFalse()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000")
        {
            Keywords = [.. Faker.Lorem.Words()]
        };
        var request = new CategoryRequest(category.Name, category.Color, category.Keywords);

        var currentUpdateTime = category.UpdatedAt;
        var expected = new Category(category.Id, category.UserId, category.Name, category.Color)
        {
            Keywords = category.Keywords
        };

        AccessServiceMock.SetupEnsureAccess(category);

        var result = await Service.UpdateAsync(category.Id, request, category.UserId, CancellationToken.None);

        result.Should().BeFalse();
        category.UpdatedAt.Should().Be(currentUpdateTime);
        category.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(category => category.UpdatedAt)
                .Excluding(category => category.CreatedAt));

        AccessServiceMock.VerifyEnsureAccessCalled(category.Id, category.UserId);
    }

    [Fact]
    public async Task Update_NoChanges_WithOtherKeywordsOrder_ShouldReturnFalse()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000")
        {
            Keywords = [.. Faker.Lorem.Words()]
        };
        var request = new CategoryRequest(category.Name, category.Color, category.Keywords);

        var currentUpdateTime = category.UpdatedAt;
        var expected = new Category(category.Id, category.UserId, category.Name, category.Color)
        {
            Keywords = [.. category.Keywords.Shuffle()]
        };

        AccessServiceMock.SetupEnsureAccess(category);

        var result = await Service.UpdateAsync(category.Id, request, category.UserId, CancellationToken.None);

        result.Should().BeFalse();
        category.UpdatedAt.Should().Be(currentUpdateTime);
        category.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(category => category.UpdatedAt)
                .Excluding(category => category.CreatedAt));

        AccessServiceMock.VerifyEnsureAccessCalled(category.Id, category.UserId);
    }

    [Fact]
    public async Task Update_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(id, new CategoryRequest(Faker.Lorem.Word(), "#000000", []), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Update_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(id, new CategoryRequest(Faker.Lorem.Word(), "#000000", []), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }
}
