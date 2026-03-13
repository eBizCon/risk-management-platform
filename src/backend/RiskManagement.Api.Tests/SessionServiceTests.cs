using Microsoft.AspNetCore.Http;
using RiskManagement.Api.Models;
using RiskManagement.Api.Services;

namespace RiskManagement.Api.Tests;

public class SessionServiceTests
{
    private readonly SessionService _sessionService = new();

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Headers.Clear();
        return context;
    }

    [Fact]
    public void Creates_Session_And_Sets_Cookie()
    {
        var context = CreateHttpContext();
        var user = new UserSession
        {
            Id = "user-1",
            Email = "test@example.com",
            Name = "Test User",
            Role = "applicant"
        };

        var sessionId = _sessionService.CreateSession(context, user);

        Assert.NotNull(sessionId);
        Assert.NotEmpty(sessionId);

        // Verify the session can be retrieved
        var retrieved = _sessionService.GetSession(sessionId);
        Assert.NotNull(retrieved);
        Assert.Equal("user-1", retrieved!.Id);
        Assert.Equal("test@example.com", retrieved.Email);
        Assert.Equal("Test User", retrieved.Name);
        Assert.Equal("applicant", retrieved.Role);
    }

    [Fact]
    public void Returns_Null_For_Missing_Session()
    {
        Assert.Null(_sessionService.GetSession(null));
        Assert.Null(_sessionService.GetSession("unknown"));
    }

    [Fact]
    public void Deletes_Session_And_Cookie()
    {
        var context = CreateHttpContext();
        var user = new UserSession
        {
            Id = "user-1",
            Email = "test@example.com",
            Name = "Test User",
            Role = "applicant"
        };

        var sessionId = _sessionService.CreateSession(context, user);
        Assert.NotNull(_sessionService.GetSession(sessionId));

        _sessionService.DeleteSession(context, sessionId);
        Assert.Null(_sessionService.GetSession(sessionId));
    }

    [Fact]
    public void Clears_All_Sessions()
    {
        var context = CreateHttpContext();
        var user = new UserSession
        {
            Id = "user-1",
            Email = "test@example.com",
            Name = "Test User",
            Role = "applicant"
        };

        var sessionId1 = _sessionService.CreateSession(context, user);
        var sessionId2 = _sessionService.CreateSession(context, user);

        Assert.NotNull(_sessionService.GetSession(sessionId1));
        Assert.NotNull(_sessionService.GetSession(sessionId2));

        _sessionService.ClearSessions();

        Assert.Null(_sessionService.GetSession(sessionId1));
        Assert.Null(_sessionService.GetSession(sessionId2));
    }
}
