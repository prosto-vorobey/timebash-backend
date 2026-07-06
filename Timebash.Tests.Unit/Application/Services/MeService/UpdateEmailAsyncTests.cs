using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class UpdateEmailAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task UpdateEmail_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var user = new User(id, name, Faker.Internet.Email());
        var currentCreatedTime = user.CreatedAt;
        var request = new UserEmailUpdateRequest($"{user.Email} changed");

        UserRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync(user);

        var result = await Service.UpdateEmailAsync(request, id);

        result.Should().BeTrue();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(request.Email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        UserRepositoryMock.Verify(repository => repository.ExistsByEmailAsync(request.Email), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateEmail_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var email = Faker.Internet.Email();
        var user = new User(id, name, email);
        var currentCreatedTime = user.CreatedAt;
        var request = new UserEmailUpdateRequest(user.Email);

        UserRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync(user);

        var result = await Service.UpdateEmailAsync(request, id);

        result.Should().BeFalse();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        UserRepositoryMock.Verify(repository => repository.ExistsByEmailAsync(request.Email), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEmail_EmailAlreadyExists_ShouldThrowResourceConflictException()
    {
        var request = new UserEmailUpdateRequest(Faker.Internet.Email());
        UserRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

        var exception = await FluentActions
            .Awaiting(() => Service.UpdateEmailAsync(request, Guid.NewGuid()))
            .Should()
            .ThrowAsync<ResourceConflictException>();
        exception.Which.Field.Should().Be("Email");
    }

    [Fact]
    public async Task UpdateEmail_EmptyId_ShouldThrowBadRequest()
    {
        var request = new UserEmailUpdateRequest(Faker.Internet.Email());
        UserRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.UpdateEmailAsync(request, Guid.Empty))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");
    }

    [Fact]
    public async Task UpdateEmail_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var request = new UserEmailUpdateRequest(Faker.Internet.Email());

        UserRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => Service.UpdateEmailAsync(request, id))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
