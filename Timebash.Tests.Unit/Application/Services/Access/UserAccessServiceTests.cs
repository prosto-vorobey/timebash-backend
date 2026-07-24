using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Application.Services.Access;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;

namespace Timebash.Tests.Unit.Application.Services.Access;

public class UserAccessServiceTests
{
    private static readonly Faker _faker = new();
    private readonly UserAccessService _service;
    private readonly Mock<IUserRepository> _repositoryMock;

    public UserAccessServiceTests()
    {
        _repositoryMock = new(MockBehavior.Strict);
        _service = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task EnsureAccess_ValidAccess_ShouldReturnUser()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        SetupGetById(user.Id, user);

        var result = await _service.EnsureAccessAsync(user.Id, CancellationToken.None);
        result.Should().Be(user);

        VerifyGetByIdCalled(user.Id);
    }

    [Fact]
    public async Task EnsureAccess_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureAccess_NonexistentId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupGetById(id, null);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyGetByIdCalled(id);
    }

    [Fact]
    public async Task ValidateExists_WhenUserExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        SetupIsUserLinked(id, true);

        await _service.ValidateExistsAsync(id, CancellationToken.None);

        VerifyIsUserLinkedCalled(id);
    }

    [Fact]
    public async Task ValidateExists_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.ValidateExistsAsync(Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateExists_NonexistentId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupIsUserLinked(id, false);

        await FluentActions
            .Awaiting(() => _service.ValidateExistsAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyIsUserLinkedCalled(id);
    }

    private void SetupGetById(Guid id, User? user)
        => _repositoryMock
            .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    private void SetupIsUserLinked(Guid id, bool isLinked)
        => _repositoryMock
            .Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isLinked);

    private void VerifyGetByIdCalled(Guid id)
        => _repositoryMock.Verify(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyIsUserLinkedCalled(Guid id)
        => _repositoryMock.Verify(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
}
