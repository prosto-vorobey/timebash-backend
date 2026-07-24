using FluentAssertions;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class GetAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task Get_ValidAccess_ShouldReturnResponse()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var expected = user.ToResponse();

        UserAccessServiceMock.SetupEnsureAccess(user);

        var result = await Service.GetAsync(user.Id, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        UserAccessServiceMock.VerifyEnsureAccessCalled(user.Id);
    }

    [Fact]
    public async Task Get_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        UserAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.GetAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }

    [Fact]
    public async Task Get_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserAccessServiceMock.SetupEnsureAccessThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.GetAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
        
        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }
}
