using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Application.Services;
using Timebash.Core.Contracts;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;

namespace Timebash.Tests.Unit.Application.Services;

public class AuthServiceTests
{
    private static readonly Faker _faker = new();
    private readonly AuthService _service;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJournalRepository> _journalRepositoryMock;
    private readonly Mock<IUserSettingsRepository> _settingsRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IJwtProvider> _providerMock;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new();
        _userRepositoryMock = new();
        _journalRepositoryMock = new();
        _settingsRepositoryMock = new();
        _passwordServiceMock = new();
        _providerMock = new();

        _service = new AuthService(
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object,
            _journalRepositoryMock.Object,
            _settingsRepositoryMock.Object,
            _passwordServiceMock.Object,
            _providerMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnResponse()
    {
        var request = new RegisterRequest(_faker.Internet.UserName(), _faker.Internet.Email(), _faker.Internet.Password());
        var expectedPasswordHash = _faker.Random.Hash();
        User? capturedUser = null;

        _userRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordServiceMock
            .Setup(service => service.HashPassword(
                It.Is<User>(user => user.Name == request.Name && user.Email == request.Email),
                request.Password))
            .Returns(expectedPasswordHash);
        _userRepositoryMock.Setup(repository => repository.Add(It.IsAny<User>())).Callback<User>(user => capturedUser = user);

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser.Id.Should().NotBeEmpty();

        var expectedUser = request.ToUser(capturedUser.Id);
        expectedUser.PasswordHash = expectedPasswordHash;
        capturedUser.Should().BeEquivalentTo(
            expectedUser,
            options => options.Excluding(user => user.CreatedAt));

        var expected = capturedUser.ToResponse();
        result.Should().BeEquivalentTo(expected);

        _userRepositoryMock.Verify(repository => repository.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repository => repository.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _journalRepositoryMock.Verify(repository => repository.Add(
            It.Is<Journal>(journal => journal.Name == "Стандартный журнал" && journal.UserId == capturedUser.Id)),
            Times.Once);
        _settingsRepositoryMock.Verify(repository => repository.Add(
            It.Is<UserSettings>(settings => settings.UserId == capturedUser.Id && settings.DefaultJournalId != Guid.Empty)),
            Times.Once);
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_NameAlreadyExists_ShouldThrowResourceConflictException()
    {
        var request = new RegisterRequest(_faker.Internet.UserName(), _faker.Internet.Email(), _faker.Internet.Password());
        _userRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var exception = await FluentActions
            .Awaiting(() => _service.RegisterAsync(request, CancellationToken.None))
            .Should()
            .ThrowAsync<ResourceConflictException>();
        exception.Which.Field.Should().Be("Name");
    }

    [Fact]
    public async Task Register_EmailAlreadyExists_ShouldThrowResourceConflictException()
    {
        var request = new RegisterRequest(_faker.Internet.UserName(), _faker.Internet.Email(), _faker.Internet.Password());
        _userRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var exception = await FluentActions
            .Awaiting(() => _service.RegisterAsync(request, CancellationToken.None))
            .Should()
            .ThrowAsync<ResourceConflictException>();
        exception.Which.Field.Should().Be("Email");
    }

    [Fact]
    public async Task Login_WithName_ValidRequest_ShouldReturnToken()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        var request = new LoginRequest(user.Name, _faker.Internet.Password());
        var expectedToken = _faker.Random.Hash();

        _userRepositoryMock.Setup(repository => repository.GetByNameAsync(user.Name, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(service => service.VerifyPassword(user, request.Password)).Returns(true);
        _providerMock.Setup(provider => provider.GenerateToken(user)).Returns(expectedToken);

        var result = await _service.LoginAsync(request, CancellationToken.None);

        result.Token.Should().Be(expectedToken);
        _passwordServiceMock.Verify(service => service.VerifyPassword(user, request.Password), Times.Once);
        _userRepositoryMock.Verify(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithEmail_ValidRequest_ShouldReturnToken()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        var request = new LoginRequest(user.Email, _faker.Internet.Password());
        var expectedToken = _faker.Random.Hash();

        _userRepositoryMock.Setup(repository => repository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(service => service.VerifyPassword(user, request.Password)).Returns(true);
        _providerMock.Setup(provider => provider.GenerateToken(user)).Returns(expectedToken);

        var result = await _service.LoginAsync(request, CancellationToken.None);

        result.Token.Should().Be(expectedToken);
        _passwordServiceMock.Verify(service => service.VerifyPassword(user, request.Password), Times.Once);
        _userRepositoryMock.Verify(repository => repository.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithName_UserNotFound_ShouldThrowUnauthorized()
    {
        var request = new LoginRequest(_faker.Internet.UserName(), _faker.Internet.Password());
        _userRepositoryMock.Setup(repository => repository.GetByNameAsync(request.Login, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => _service.LoginAsync(request, CancellationToken.None))
            .Should()
            .ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_WithEmail_UserNotFound_ShouldThrowUnauthorized()
    {
        var request = new LoginRequest(_faker.Internet.Email(), _faker.Internet.Password());
        _userRepositoryMock.Setup(repository => repository.GetByEmailAsync(request.Login, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => _service.LoginAsync(request, CancellationToken.None))
            .Should()
            .ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_InvalidPassword_ShouldThrowUnauthorized()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        var request = new LoginRequest(user.Email, _faker.Internet.Password());

        _userRepositoryMock.Setup(repository => repository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(service => service.VerifyPassword(user, request.Password)).Returns(false);

        await FluentActions
            .Awaiting(() => _service.LoginAsync(request, CancellationToken.None))
            .Should()
            .ThrowAsync<UnauthorizedException>();
    }
}
