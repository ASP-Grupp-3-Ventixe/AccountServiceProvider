using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Presentation.Services;
using Grpc.Core;
using Grpc.Core.Testing;
using System.Threading;
using System.Threading.Tasks;
using Presentation;

public class AccountServiceTests
{
    [Fact]
    public async Task CreateAccount_ShouldReturnSuccess_WhenUserIsCreated()
    {
        // Arrange
        var mockUserManager = MockUserManager();
        mockUserManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success);

        var service = new AccountService(mockUserManager.Object);
        var request = new CreateAccountRequest { Email = "test@example.com", Password = "Password123!" };

        var context = CreateTestContext();

        // Act
        var result = await service.CreateAccount(request, context);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Account created successfully.", result.Message);
        Assert.False(string.IsNullOrWhiteSpace(result.UserId));
    }

    [Fact]
    public async Task ValidateCredentials_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var mockUserManager = MockUserManager();
        mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((IdentityUser)null);

        var service = new AccountService(mockUserManager.Object);
        var request = new ValidateCredentialsRequest { Email = "notfound@example.com", Password = "invalid" };

        var context = CreateTestContext();

        // Act
        var result = await service.ValidateCredentials(request, context);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Invalid credentials.", result.Message);
    }

    // Helpers
    private static Mock<UserManager<IdentityUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private static ServerCallContext CreateTestContext()
    {
        return TestServerCallContext.Create(
            method: "test/method",
            host: "localhost",
            deadline: DateTime.UtcNow.AddMinutes(1),
            requestHeaders: new Metadata(),
            cancellationToken: CancellationToken.None,
            peer: "test-peer",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => new WriteOptions(),
            writeOptionsSetter: _ => { }
        );
    }
}
