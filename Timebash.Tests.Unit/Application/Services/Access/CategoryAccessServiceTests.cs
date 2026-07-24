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
        _repositoryMock = new(MockBehavior.Strict);
        _service = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task EnsureCategoryAccess_ValidAccess_ShouldReturnCategory()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        SetupGetById(category.Id, category);

        var result = await _service.EnsureAccessAsync(category.Id, category.UserId, CancellationToken.None);
        result.Should().Be(category);

        VerifyGetByIdCalled(category.Id);
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
        SetupGetById(id, null);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyGetByIdCalled(id);
    }

    [Fact]
    public async Task EnsureCategoryAccess_WrongUserId_ShouldThrowNotFound()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        SetupGetById(category.Id, category);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(category.Id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyGetByIdCalled(category.Id);
    }

    [Fact]
    public async Task ValidateCategoryAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupIsUserLinked(id, userId, true);

        await _service.ValidateAccessAsync(id, userId, CancellationToken.None);

        VerifyIsUserLinkedCalled(id, userId);
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
        SetupIsUserLinked(id, userId, false);

        await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyIsUserLinkedCalled(id, userId);
    }

    private void SetupGetById(Guid id, Category? category)
        => _repositoryMock
            .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

    private void SetupIsUserLinked(Guid id, Guid userId, bool isLinked)
        => _repositoryMock
            .Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isLinked);

    private void VerifyGetByIdCalled(Guid id)
        => _repositoryMock.Verify(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyIsUserLinkedCalled(Guid id, Guid userId)
        => _repositoryMock.Verify(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);
}
