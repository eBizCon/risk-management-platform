using DevinAlertBridge;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("DevinApiClient");
builder.Services.AddSingleton<DevinSessionTrigger>();

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok("ok"));

app.MapPost("/api/alerts/devin", async (HttpContext context, DevinSessionTrigger trigger, CancellationToken cancellationToken) =>
{
    return await trigger.HandleAsync(context, cancellationToken);
});

app.Run();
