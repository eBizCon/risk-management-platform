import { defineConfig, devices } from '@playwright/test';
import { config } from 'dotenv';

config({ path: '.env.test', quiet: true });

export default defineConfig({
	webServer: [
		{
			name: 'backend',
			command: [
				'ASPIRE_TEST_MODE=true',
				'ASPIRE_TEST_RISK_API_PORT=5627',
				'ASPIRE_TEST_CUSTOMER_API_PORT=5400',
				'ASPIRE_TEST_KEYCLOAK_PORT=8181',
				'ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL=http://localhost:29032',
				'ASPIRE_RESOURCE_SERVICE_ENDPOINT_URL=http://localhost:30132',
				'dotnet run --project ../backend/AppHost/AppHost.csproj --launch-profile http-testmode'
			].join(' '),
			url: 'http://localhost:5627/health',
			timeout: 240 * 1000,
			reuseExistingServer: !process.env.CI,
			gracefulShutdown: {
				signal: 'SIGINT',
				timeout: 30 * 1000
			}
		},
		{
			name: 'frontend',
			command: 'npm run build && TEST=true npm run preview',
			env: {
				...process.env,
				RISK_MANAGEMENT_API_URL: 'http://localhost:5627',
				CUSTOMER_SERVICE_URL: 'http://localhost:5400',
				OIDC_ISSUER: 'http://localhost:8181/realms/risk-management'
			},
			url: 'http://localhost:4173',
			timeout: 120 * 1000,
			reuseExistingServer: !process.env.CI
		}
	],
	testDir: 'e2e',
	testMatch: /(.+\.)?(test|spec)\.[jt]s/,
	timeout: 30000,
	fullyParallel: true,
	maxFailures: 2,
	forbidOnly: !!process.env.CI,
	retries: process.env.CI ? 1 : 0,
	workers: 4,
	reporter: 'html',
	use: {
		baseURL: 'http://localhost:4173',
		trace: 'on-first-retry',
		screenshot: 'only-on-failure'
	},
	projects: [
		{
			name: 'chromium',
			use: { ...devices['Desktop Chrome'] }
		}
	]
});
