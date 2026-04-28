@description('Name prefix for monitoring resources')
param prefix string

@description('Location for resources')
param location string

@description('Log Analytics workspace resource ID used by Application Insights')
param logAnalyticsWorkspaceId string

@secure()
@description('Webhook URL of the Devin bridge that starts a session')
param devinSessionWebhookUrl string

resource devinActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${prefix}-devin-ag'
  location: 'global'
  properties: {
    groupShortName: 'devinalert'
    enabled: true
    webhookReceivers: [
      {
        name: 'devin-session-webhook'
        serviceUri: devinSessionWebhookUrl
        useCommonAlertSchema: true
      }
    ]
  }
}

resource appInsightsExceptionAlert 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: '${prefix}-appinsights-exceptions'
  location: location
  properties: {
    description: 'Triggers Devin session webhook when Application Insights exceptions are detected.'
    displayName: '${prefix} Application Insights exceptions'
    enabled: true
    severity: 2
    scopes: [
      logAnalyticsWorkspaceId
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1M'
    autoMitigate: true
    criteria: {
      allOf: [
        {
          query: 'AppExceptions | where AppRoleName  == "riskmgmt-dev-risk-api" |  where Properties has "Unhandled exception while processing request"'
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [
        devinActionGroup.id
      ]
      customProperties: {
        source: 'application-insights'
        trigger: 'exceptions'
      }
    }
  }
}
