using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Application.Services.Access;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;

namespace Timebash.Tests.Unit.Application.Services.Access;

public class CategoryAccessServiceTests
{
    private static readonly Faker _faker = new();
    private readonly CategoryAccessService _service;
    private readonly Mock<ICategoryRepository> _repositoryMock;

    public CategoryAccessServiceTests()
    {
        _repositoryMock = new();
        _service = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task EnsureCategoryAccess_ValidAccess_ShouldReturnCategory()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        _repositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await _service.EnsureAccessAsync(category.Id, category.UserId, CancellationToken.None);
        result.Should().Be(category);
    }

    [Fact]
    public async Task EnsureCategoryAccess_EmptyCategoryId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureCategoryAccess_NonexistentCategoryId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureCategoryAccess_WrongUserId_ShouldThrowNotFound()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        _repositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(category.Id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateCategoryAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _service.ValidateAccessAsync(id, userId, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateCategoryAccess_EmptyCategoryId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateCategoryAccess_NotUserLinked_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
