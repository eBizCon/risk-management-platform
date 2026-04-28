using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DevinAlertBridge;

public sealed class DevinSessionTrigger
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DevinSessionTrigger> _logger;

    public DevinSessionTrigger(IHttpClientFactory httpClientFactory, ILogger<DevinSessionTrigger> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IResult> HandleAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var invocationId = Guid.NewGuid().ToString("N");
        var configuredToken = Environment.GetEnvironmentVariable("ALERT_WEBHOOK_TOKEN") ?? string.Empty;
        var devinOrgId = Environment.GetEnvironmentVariable("DEVIN_ORG_ID") ?? string.Empty;
        var devinApiKey = Environment.GetEnvironmentVariable("DEVIN_API_KEY") ?? string.Empty;
        var devinApiUrl = string.IsNullOrWhiteSpace(devinOrgId)
            ? string.Empty
            : $"https://api.devin.ai/v3/organizations/{devinOrgId}/sessions";

        _logger.LogInformation(
            "[{InvocationId}] Incoming Azure Monitor alert request. Method={Method}, Path={Path}",
            invocationId,
            context.Request.Method,
            context.Request.Path);

        if (string.IsNullOrWhiteSpace(configuredToken) || string.IsNullOrWhiteSpace(devinOrgId))
        {
            return Results.Problem("Bridge is not configured.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var providedToken = context.Request.Query.TryGetValue("token", out var tokenValue) ? tokenValue.ToString() : string.Empty;
        if (!string.Equals(configuredToken, providedToken, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return Results.BadRequest("Request body is required.");
        }

        _logger.LogInformation(
            "[{InvocationId}] Incoming alert payload (truncated): {IncomingPayload}",
            invocationId,
            Truncate(requestBody, 8000));

        JsonNode? alertPayload;
        try
        {
            alertPayload = JsonNode.Parse(requestBody);
        }
        catch (JsonException)
        {
            return Results.BadRequest("Invalid JSON payload.");
        }

        var essentials = alertPayload?["data"]?["essentials"];
        var alertRule = essentials?["alertRule"]?.GetValue<string>() ?? "unknown";
        var severity = essentials?["severity"]?.GetValue<string>() ?? "unknown";
        var monitorCondition = essentials?["monitorCondition"]?.GetValue<string>() ?? "unknown";
        var firedDateTime = essentials?["firedDateTime"]?.GetValue<string>() ?? "unknown";
        var alertId = essentials?["alertId"]?.GetValue<string>() ?? "unknown";

        _logger.LogInformation(
            "[{InvocationId}] Alert essentials: AlertRule={AlertRule}, Severity={Severity}, MonitorCondition={MonitorCondition}, FiredDateTime={FiredDateTime}",
            invocationId,
            alertRule,
            severity,
            monitorCondition,
            firedDateTime);

        var prompt = BuildSessionPrompt(alertRule, severity, monitorCondition, firedDateTime, alertId, alertPayload);
        var title = $"[Alert:{severity}] {alertRule}";
        // var repos = ParseCsv(Environment.GetEnvironmentVariable("DEVIN_SESSION_REPOS"));
        var repos = "risk-management-platform";
        var bypassApproval = ParseBool(Environment.GetEnvironmentVariable("DEVIN_BYPASS_APPROVAL"), true);

        var sessionRequestPayload = new Dictionary<string, object?>
        {
            ["prompt"] = prompt,
            ["title"] = title,
            ["playbook_id"] = "518ba362c3e6497f97015d052b6060ec",
            ["bypass_approval"] = bypassApproval,
            ["tags"] = new[] { "azure-monitor", "application-insights", "risk-management-platform" }
        };

        // if (repos.Count > 0)
        // {
            sessionRequestPayload["repos"] = repos;
        // }

        var devinPayload = JsonSerializer.Serialize(sessionRequestPayload);

        _logger.LogInformation(
            "[{InvocationId}] Outgoing Devin request prepared. DevinApiUrl={DevinApiUrl}, HasAuthorizationHeader={HasAuthorizationHeader}, Payload={OutgoingPayload}",
            invocationId,
            devinApiUrl,
            !string.IsNullOrWhiteSpace(devinApiKey),
            Truncate(devinPayload, 8000));

        using var outboundRequest = new HttpRequestMessage(HttpMethod.Post, devinApiUrl)
        {
            Content = new StringContent(devinPayload, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(devinApiKey))
        {
            outboundRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", devinApiKey);
        }

        var client = _httpClientFactory.CreateClient("DevinApiClient");
        using var outboundResponse = await client.SendAsync(outboundRequest, cancellationToken);
        var outboundResponseBody = await outboundResponse.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation(
            "[{InvocationId}] Devin response received. StatusCode={StatusCode}, Body={ResponseBody}",
            invocationId,
            (int)outboundResponse.StatusCode,
            Truncate(outboundResponseBody, 8000));

        if (!outboundResponse.IsSuccessStatusCode)
        {
            _logger.LogError("[{InvocationId}] Devin API call failed with status code {StatusCode}", invocationId, outboundResponse.StatusCode);
            return Results.Problem("Failed to trigger Devin session.", statusCode: StatusCodes.Status502BadGateway);
        }

        return Results.Accepted(value: "Devin session trigger accepted.");
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }

    private static List<string> ParseCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var result = new List<string>();
        foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private static string BuildSessionPrompt(
        string alertRule,
        string severity,
        string monitorCondition,
        string firedDateTime,
        string alertId,
        JsonNode? alertPayload)
    {
        var alertJson = alertPayload?.ToJsonString() ?? "{}";
        return $"""
An Azure Monitor alert was triggered and requires investigation.

Alert details:
- Rule: {alertRule}
- Severity: {severity}
- Condition: {monitorCondition}
- FiredAt: {firedDateTime}
- AlertId: {alertId}

Raw Azure alert payload (JSON):
{alertJson}
""";
    }
}
