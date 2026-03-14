using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using SharedKernel.Middleware;

namespace RiskManagement.Api.Tests;

public class InternalAuthMiddlewareTests
{
    private const string ValidApiKey = "test-api-key-12345";

    private static IHostEnvironment CreateEnvironment(string environmentName = "Development")
    {
        var mock = new Mock<IHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns(environmentName);
        return mock.Object;
    }

    private static InternalAuthMiddleware CreateMiddleware(
        RequestDelegate next,
        string environmentName = "Development")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SERVICE_API_KEY"] = ValidApiKey
            })
            .Build();

        return new InternalAuthMiddleware(next, config, CreateEnvironment(environmentName));
    }

    private static DefaultHttpContext CreateContext(
        string path,
        Dictionary<string, string>? headers = null,
        ClaimsPrincipal? user = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                context.Request.Headers[key] = value;
            }
        }
        if (user != null)
        {
            context.User = user;
        }
        return context;
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Health_Endpoint_Bypasses_Auth()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateContext("/health");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Non_Api_Path_Bypasses_Auth()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateContext("/some-page");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Api_Request_Without_ApiKey_Returns_401()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateContext("/api/applications");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Api_Request_With_Invalid_ApiKey_Returns_401()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateContext("/api/applications", new Dictionary<string, string>
        {
            ["X-Api-Key"] = "wrong-key"
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Api_Internal_With_Valid_ApiKey_Passes_Without_User()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateContext("/api/internal/score", new Dictionary<string, string>
        {
            ["X-Api-Key"] = ValidApiKey
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Api_Request_With_Jwt_Authenticated_User_Passes_Through()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            "Production");

        var jwtUser = CreateAuthenticatedUser("jwt-user-1", "applicant");
        var context = CreateContext(
            "/api/applications",
            new Dictionary<string, string> { ["X-Api-Key"] = ValidApiKey },
            jwtUser);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Api_Request_Without_Jwt_And_No_Headers_Returns_401_In_Production()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            "Production");

        var context = CreateContext("/api/applications", new Dictionary<string, string>
        {
            ["X-Api-Key"] = ValidApiKey
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Api_Request_With_Headers_Returns_401_In_Production()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            "Production");

        var context = CreateContext("/api/applications", new Dictionary<string, string>
        {
            ["X-Api-Key"] = ValidApiKey,
            ["X-User-Id"] = "user-123",
            ["X-User-Role"] = "applicant"
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Api_Request_With_Headers_Sets_ClaimsPrincipal_In_Development()
    {
        ClaimsPrincipal? capturedUser = null;
        var middleware = CreateMiddleware(ctx =>
        {
            capturedUser = ctx.User;
            return Task.CompletedTask;
        });

        var context = CreateContext("/api/applications", new Dictionary<string, string>
        {
            ["X-Api-Key"] = ValidApiKey,
            ["X-User-Id"] = "user-123",
            ["X-User-Email"] = "test@example.com",
            ["X-User-Name"] = "Test User",
            ["X-User-Role"] = "applicant"
        });

        await middleware.InvokeAsync(context);

        capturedUser.Should().NotBeNull();
        capturedUser!.Identity.Should().NotBeNull();
        capturedUser.Identity!.IsAuthenticated.Should().BeTrue();
        capturedUser.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be("user-123");
        capturedUser.FindFirstValue(ClaimTypes.Email).Should().Be("test@example.com");
        capturedUser.FindFirstValue(ClaimTypes.Name).Should().Be("Test User");
        capturedUser.FindFirstValue(ClaimTypes.Role).Should().Be("applicant");
    }

    [Fact]
    public async Task Api_Request_Without_Optional_Headers_Still_Sets_Required_Claims()
    {
        ClaimsPrincipal? capturedUser = null;
        var middleware = CreateMiddleware(ctx =>
        {
            capturedUser = ctx.User;
            return Task.CompletedTask;
        });

        var context = CreateContext("/api/applications", new Dictionary<string, string>
        {
            ["X-Api-Key"] = ValidApiKey,
            ["X-User-Id"] = "user-456",
            ["X-User-Role"] = "processor"
        });

        await middleware.InvokeAsync(context);

        capturedUser.Should().NotBeNull();
        capturedUser!.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be("user-456");
        capturedUser.FindFirstValue(ClaimTypes.Role).Should().Be("processor");
        capturedUser.FindFirstValue(ClaimTypes.Email).Should().BeNull();
        capturedUser.FindFirstValue(ClaimTypes.Name).Should().BeNull();
    }

    [Fact]
    public async Task Api_Request_With_No_Headers_Returns_401_In_Development()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateContext("/api/applications", new Dictionary<string, string>
        {
            ["X-Api-Key"] = ValidApiKey
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Constructor_Throws_When_ApiKey_Not_Configured()
    {
        var config = new ConfigurationBuilder().Build();

        var act = () => new InternalAuthMiddleware(
            _ => Task.CompletedTask,
            config,
            CreateEnvironment());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SERVICE_API_KEY*");
    }
}
