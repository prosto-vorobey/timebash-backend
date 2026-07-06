using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class DeleteAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task Delete_ValidRequest_ShouldDeleteUser()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);

        await Service.DeleteAsync(user.Id);

        SettingsRepositoryMock.Verify(repository => repository.DeleteAsync(user.Id), Times.Once);
        UserRepositoryMock.Verify(repository => repository.Delete(user), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.DeleteAsync(Guid.Empty))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task Delete_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
