using Microsoft.Extensions.DependencyInjection;
using Moq;
using SharedKernel.Common;
using SharedKernel.Dispatching;
using SharedKernel.Results;

namespace RiskManagement.Api.Tests;

public record TestCommand(string Value) : ICommand<string>;

public record TestQuery(string Value) : IQuery<string>;

public record TestEvent(string Value) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<Result<string>> HandleAsync(TestCommand command, CancellationToken ct = default)
    {
        return Task.FromResult(Result<string>.Success($"handled:{command.Value}"));
    }
}

public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<Result<string>> HandleAsync(TestQuery query, CancellationToken ct = default)
    {
        return Task.FromResult(Result<string>.Success($"queried:{query.Value}"));
    }
}

public class DispatcherTests
{
    private static Dispatcher CreateDispatcher(IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        return new Dispatcher(provider);
    }

    [Fact]
    public async Task SendAsync_Resolves_Correct_CommandHandler()
    {
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandler>();
        var dispatcher = CreateDispatcher(services);

        var result = await dispatcher.SendAsync(new TestCommand("foo"));

        Assert.True(result.IsSuccess);
        Assert.Equal("handled:foo", result.Value);
    }

    [Fact]
    public async Task QueryAsync_Resolves_Correct_QueryHandler()
    {
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();
        var dispatcher = CreateDispatcher(services);

        var result = await dispatcher.QueryAsync(new TestQuery("bar"));

        Assert.True(result.IsSuccess);
        Assert.Equal("queried:bar", result.Value);
    }

    [Fact]
    public async Task PublishAsync_Fans_Out_To_Multiple_Handlers()
    {
        var handler1 = new Mock<IDomainEventHandler<TestEvent>>();
        var handler2 = new Mock<IDomainEventHandler<TestEvent>>();

        var services = new ServiceCollection();
        services.AddScoped(_ => handler1.Object);
        services.AddScoped(_ => handler2.Object);
        var dispatcher = CreateDispatcher(services);

        var evt = new TestEvent("test");
        await dispatcher.PublishAsync(evt);

        handler1.Verify(h => h.HandleAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.HandleAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_Tolerates_Zero_Handlers()
    {
        var services = new ServiceCollection();
        var dispatcher = CreateDispatcher(services);

        var exception = await Record.ExceptionAsync(() => dispatcher.PublishAsync(new TestEvent("no-handlers")));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendAsync_Throws_On_Missing_Handler()
    {
        var services = new ServiceCollection();
        var dispatcher = CreateDispatcher(services);

        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.SendAsync(new TestCommand("missing")));
    }

    [Fact]
    public async Task QueryAsync_Throws_On_Missing_Handler()
    {
        var services = new ServiceCollection();
        var dispatcher = CreateDispatcher(services);

        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.QueryAsync(new TestQuery("missing")));
    }

}