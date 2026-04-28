using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DevinAlertBridge.Function;

public sealed class AlertToDevinFunction(
    IHttpClientFactory httpClientFactory,
    ILogger<AlertToDevinFunction> logger)
{
    [Function("AlertToDevinSession")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alerts/devin")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var invocationId = Guid.NewGuid().ToString("N");
        var configuredToken = Environment.GetEnvironmentVariable("ALERT_WEBHOOK_TOKEN") ?? string.Empty;
        var devinOrgId = Environment.GetEnvironmentVariable("DEVIN_ORG_ID") ?? string.Empty;
        var devinApiKey = Environment.GetEnvironmentVariable("DEVIN_API_KEY") ?? string.Empty;
        var devinApiUrl = string.IsNullOrWhiteSpace(devinOrgId)
            ? string.Empty
            : $"https://api.devin.ai/v3/organizations/{devinOrgId}/sessions";

        logger.LogInformation(
            "[{InvocationId}] Incoming Azure Monitor alert request. Method={Method}, Url={Url}",
            invocationId,
            request.Method,
            request.Url);

        if (string.IsNullOrWhiteSpace(configuredToken) || string.IsNullOrWhiteSpace(devinOrgId))
        {
            var misconfiguredResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
            await misconfiguredResponse.WriteStringAsync("Function app is not configured.", cancellationToken);
            return misconfiguredResponse;
        }

        var query = HttpUtility.ParseQueryString(request.Url.Query);
        var providedToken = query.Get("token") ?? string.Empty;
        if (!string.Equals(configuredToken, providedToken, StringComparison.Ordinal))
        {
            var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorizedResponse.WriteStringAsync("Unauthorized.", cancellationToken);
            return unauthorizedResponse;
        }

        var requestBody = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Request body is required.", cancellationToken);
            return badRequestResponse;
        }

        logger.LogInformation(
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
            var invalidJsonResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await invalidJsonResponse.WriteStringAsync("Invalid JSON payload.", cancellationToken);
            return invalidJsonResponse;
        }

        var essentials = alertPayload?["data"]?["essentials"];
        var alertRule = essentials?["alertRule"]?.GetValue<string>() ?? "unknown";
        var severity = essentials?["severity"]?.GetValue<string>() ?? "unknown";
        var monitorCondition = essentials?["monitorCondition"]?.GetValue<string>() ?? "unknown";
        var firedDateTime = essentials?["firedDateTime"]?.GetValue<string>() ?? "unknown";
        var alertId = essentials?["alertId"]?.GetValue<string>() ?? "unknown";

        logger.LogInformation(
            "[{InvocationId}] Alert essentials: AlertRule={AlertRule}, Severity={Severity}, MonitorCondition={MonitorCondition}, FiredDateTime={FiredDateTime}",
            invocationId,
            alertRule,
            severity,
            monitorCondition,
            firedDateTime);

        var prompt = BuildSessionPrompt(alertRule, severity, monitorCondition, firedDateTime, alertId, alertPayload);
        var title = $"[Alert:{severity}] {alertRule}";
        var repos = ParseCsv(Environment.GetEnvironmentVariable("DEVIN_SESSION_REPOS"));
        var bypassApproval = ParseBool(Environment.GetEnvironmentVariable("DEVIN_BYPASS_APPROVAL"), true);

        var sessionRequestPayload = new Dictionary<string, object?>
        {
            ["prompt"] = prompt,
            ["title"] = title,
            ["bypass_approval"] = bypassApproval,
            ["tags"] = new[] { "azure-monitor", "application-insights", "risk-management-platform" }
        };

        if (repos.Count > 0)
        {
            sessionRequestPayload["repos"] = repos;
        }

        var devinPayload = JsonSerializer.Serialize(sessionRequestPayload);

        logger.LogInformation(
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

        var client = httpClientFactory.CreateClient("DevinApiClient");
        using var outboundResponse = await client.SendAsync(outboundRequest, cancellationToken);
        var outboundResponseBody = await outboundResponse.Content.ReadAsStringAsync(cancellationToken);

        logger.LogInformation(
            "[{InvocationId}] Devin response received. StatusCode={StatusCode}, Body={ResponseBody}",
            invocationId,
            (int)outboundResponse.StatusCode,
            Truncate(outboundResponseBody, 8000));

        if (!outboundResponse.IsSuccessStatusCode)
        {
            logger.LogError("Devin API call failed with status code {StatusCode}", outboundResponse.StatusCode);
            var failedResponse = request.CreateResponse(HttpStatusCode.BadGateway);
            await failedResponse.WriteStringAsync("Failed to trigger Devin session.", cancellationToken);
            return failedResponse;
        }

        var successResponse = request.CreateResponse(HttpStatusCode.Accepted);
        await successResponse.WriteStringAsync("Devin session trigger accepted.", cancellationToken);
        return successResponse;
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

Tasks:
1. Analyze probable root cause from the alert context.
2. Propose immediate mitigation steps.
3. Propose a long-term fix and verification plan.

Raw Azure alert payload (JSON):
{alertJson}
""";
    }
}
