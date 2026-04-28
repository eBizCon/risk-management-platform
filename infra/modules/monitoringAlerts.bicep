@description('Name prefix for monitoring resources')
param prefix string

@description('Location for resources')
param location string

@description('Log Analytics workspace resource ID used by Application Insights')
param logAnalyticsWorkspaceId string

@description('Resource ID of the Devin bridge Function App')
param devinFunctionAppResourceId string

@description('Function name inside the Devin bridge Function App')
param devinFunctionName string = 'AlertToDevinSession'

@secure()
@description('HTTP trigger URL of the Devin bridge function')
param devinFunctionTriggerUrl string

resource devinActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${prefix}-devin-ag'
  location: 'global'
  properties: {
    groupShortName: 'devinalert'
    enabled: true
    azureFunctionReceivers: [
      {
        name: 'devin-session-function'
        functionAppResourceId: devinFunctionAppResourceId
        functionName: devinFunctionName
        httpTriggerUrl: devinFunctionTriggerUrl
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
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    autoMitigate: true
    criteria: {
      allOf: [
        {
          query: 'AppExceptions'
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
