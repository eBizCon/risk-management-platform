// Mock für $env/static/private zur Verwendung in Tests
export const DATABASE_URL =
	process.env.DATABASE_URL || 'postgresql://risk:risk@localhost:5432/risk_management_test';
export const OIDC_ISSUER = 'https://idp.example.com';
export const OIDC_CLIENT_ID = 'client-id';
export const OIDC_REDIRECT_URI = 'https://app.example.com/auth/callback';
export const OIDC_POST_LOGOUT_REDIRECT_URI = 'https://app.example.com/logout/callback';
export const OIDC_SCOPE = 'openid profile email roles';
export const OIDC_ROLES_CLAIM_PATH = 'resource_access.roles';
