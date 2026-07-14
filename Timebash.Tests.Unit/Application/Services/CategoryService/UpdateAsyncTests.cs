using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

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

        RepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await Service.UpdateAsync(category.Id, request, category.UserId, CancellationToken.None);

        result.Should().BeTrue();
        category.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        category.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(category => category.UpdatedAt)
                .Excluding(category => category.CreatedAt));

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        RepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await Service.UpdateAsync(category.Id, request, category.UserId, CancellationToken.None);

        result.Should().BeFalse();
        category.UpdatedAt.Should().Be(currentUpdateTime);
        category.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(category => category.UpdatedAt)
                .Excluding(category => category.CreatedAt));

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        RepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await Service.UpdateAsync(category.Id, request, category.UserId, CancellationToken.None);

        result.Should().BeFalse();
        category.UpdatedAt.Should().Be(currentUpdateTime);
        category.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(category => category.UpdatedAt)
                .Excluding(category => category.CreatedAt));

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                Guid.Empty, 
                new CategoryRequest(Faker.Lorem.Word(), "#000000", []), Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task Update_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        RepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                id, 
                new CategoryRequest(Faker.Lorem.Word(), "#000000", []), Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
