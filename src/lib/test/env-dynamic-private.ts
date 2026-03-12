// Mock für $env/dynamic/private zur Verwendung in Tests
export const env = {
  DATABASE_URL: process.env.DATABASE_URL || 'postgresql://risk:risk@localhost:5432/risk_management_test',
  OIDC_ISSUER: 'https://idp.example.com',
  OIDC_CLIENT_ID: 'client-id',
  OIDC_CLIENT_SECRET: 'test-secret',
  OIDC_REDIRECT_URI: 'https://app.example.com/auth/callback',
  OIDC_POST_LOGOUT_REDIRECT_URI: 'https://app.example.com/logout/callback',
  OIDC_SCOPE: 'openid profile email roles',
  OIDC_ROLES_CLAIM_PATH: 'resource_access.roles'
};
