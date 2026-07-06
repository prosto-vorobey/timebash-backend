using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class UpdateNameAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task UpdateName_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var email = Faker.Internet.Email();
        var user = new User(id, Faker.Internet.UserName(), email);
        var currentCreatedTime = user.CreatedAt;
        var request = new UserNameUpdateRequest($"{user.Name} changed");

        UserRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name)).ReturnsAsync(false);
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync(user);

        var result = await Service.UpdateNameAsync(request, id);

        result.Should().BeTrue();
        user.Name.Should().Be(request.Name);
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        UserRepositoryMock.Verify(repository => repository.ExistsByNameAsync(request.Name), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateName_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var email = Faker.Internet.Email();
        var user = new User(id, name, email);
        var currentCreatedTime = user.CreatedAt;
        var request = new UserNameUpdateRequest(name);

        UserRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name)).ReturnsAsync(false);
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync(user);

        var result = await Service.UpdateNameAsync(request, id);

        result.Should().BeFalse();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        UserRepositoryMock.Verify(repository => repository.ExistsByNameAsync(request.Name), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateName_NameAlreadyExists_ShouldThrowResourceConflictException()
    {
        var request = new UserNameUpdateRequest(Faker.Internet.UserName());
        UserRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name)).ReturnsAsync(true);

        var exception = await FluentActions
            .Awaiting(() => Service.UpdateNameAsync(request, Guid.NewGuid()))
            .Should()
            .ThrowAsync<ResourceConflictException>();
        exception.Which.Field.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateName_EmptyId_ShouldThrowBadRequest()
    {
        var request = new UserNameUpdateRequest(Faker.Internet.UserName());
        UserRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.UpdateNameAsync(request, Guid.Empty))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");
    }

    [Fact]
    public async Task UpdateName_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var request = new UserNameUpdateRequest(Faker.Internet.UserName());

        UserRepositoryMock.Setup(repository => repository.ExistsByNameAsync(request.Name)).ReturnsAsync(false);
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => Service.UpdateNameAsync(request, id))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
