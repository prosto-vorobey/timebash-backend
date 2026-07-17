using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public class CreateAsyncTests : CategoryServiceTestsBase
{
    [Fact]
    public async Task Create_ShouldReturnResponse()
    {
        var request = new CategoryRequest(Faker.Lorem.Word(), "#000000", [.. Faker.Lorem.Words()]);
        var userId = Guid.NewGuid();

        var capturedCategories = new List<Category>();
        RepositoryMock.Setup(repository => repository.Add(It.IsAny<Category>())).Callback<Category>(capturedCategories.Add);

        var result = await Service.CreateAsync(request, userId, CancellationToken.None);

        capturedCategories.Should().HaveCount(1);

        var capturedCategory = capturedCategories.First();
        capturedCategory.Id.Should().NotBeEmpty();
        capturedCategory.Should().BeEquivalentTo(
            request.ToCategory(capturedCategory.Id, userId),
            options => options 
                .Excluding(category => category.CreatedAt)
                .Excluding(category => category.UpdatedAt));

        result.Should().BeEquivalentTo(capturedCategory.ToResponse());
        VerifySaveChangesCalled();
    }
}
