using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class GetAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task Get_ValidAccess_ShouldReturnResponse()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var expected = user.ToResponse();

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await Service.GetAsync(user.Id);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Get_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetAsync(Guid.Empty))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task Get_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => Service.GetAsync(id))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
