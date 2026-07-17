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

        SetupUserEnsureAccess(user);

        var result = await Service.GetAsync(user.Id, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyEnsureAccessCalled(user.Id);
    }

    [Fact]
    public async Task Get_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        SetupUserEnsureAccessThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.GetAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyEnsureAccessCalled(id);
    }

    [Fact]
    public async Task Get_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupUserEnsureAccessThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.GetAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
        
        VerifyEnsureAccessCalled(id);
    }
}
