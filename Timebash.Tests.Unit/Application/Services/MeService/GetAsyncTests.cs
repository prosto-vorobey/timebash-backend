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

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await Service.GetAsync(user.Id, CancellationToken.None);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Get_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetAsync(Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task Get_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => Service.GetAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
