import * as client from 'openid-client';
import { env } from '$env/dynamic/private';

const ALLOWED_ROLES: App.UserRole[] = ['applicant', 'processor', 'risk_manager'];

let cachedConfig: client.Configuration | null = null;

export function getOidcEnv() {
	const issuer = env.OIDC_ISSUER;
	const clientId = env.OIDC_CLIENT_ID;
	const clientSecret = env.OIDC_CLIENT_SECRET;
	const scope = env.OIDC_SCOPE ?? 'openid profile email';
	const rolesClaimPath = env.OIDC_ROLES_CLAIM_PATH ?? 'realm_access.roles';
	const postLogoutRedirectUri = env.OIDC_POST_LOGOUT_REDIRECT_URI ?? 'http://localhost:5173';
	const redirectUri = env.OIDC_REDIRECT_URI ?? 'http://localhost:5173/auth/callback';

	if (!issuer) throw new Error('OIDC_ISSUER environment variable is required');
	if (!clientId) throw new Error('OIDC_CLIENT_ID environment variable is required');
	if (!clientSecret) throw new Error('OIDC_CLIENT_SECRET environment variable is required');

	return {
		issuer,
		clientId,
		clientSecret,
		scope,
		rolesClaimPath,
		postLogoutRedirectUri,
		redirectUri
	};
}

export async function getOidcConfig(): Promise<client.Configuration> {
	if (cachedConfig) return cachedConfig;

	const { issuer, clientId, clientSecret } = getOidcEnv();
	const executeOptions = issuer.startsWith('http://')
		? { execute: [client.allowInsecureRequests] }
		: undefined;

	cachedConfig = await client.discovery(
		new URL(issuer),
		clientId,
		clientSecret,
		undefined,
		executeOptions
	);
	return cachedConfig;
}

export function generatePkce(): { codeVerifier: string; codeChallenge: Promise<string> } {
	const codeVerifier = client.randomPKCECodeVerifier();
	const codeChallenge = client.calculatePKCECodeChallenge(codeVerifier);
	return { codeVerifier, codeChallenge };
}

export function generateState(): string {
	return client.randomState();
}

export function generateNonce(): string {
	return client.randomNonce();
}

export function buildAuthorizationUrl(
	config: client.Configuration,
	params: Record<string, string>
): URL {
	return client.buildAuthorizationUrl(config, params);
}

export async function exchangeCodeForTokens(
	config: client.Configuration,
	callbackUrl: URL,
	codeVerifier: string,
	expectedState: string,
	expectedNonce: string
): Promise<client.TokenEndpointResponse> {
	return client.authorizationCodeGrant(config, callbackUrl, {
		pkceCodeVerifier: codeVerifier,
		expectedState,
		expectedNonce
	});
}

export function extractRolesFromAccessToken(
	accessToken: string,
	claimPath: string
): App.UserRole[] {
	try {
		const parts = accessToken.split('.');
		if (parts.length !== 3) return [];

		const payload = JSON.parse(Buffer.from(parts[1], 'base64url').toString('utf8'));

		const pathParts = claimPath.split('.');
		let current: unknown = payload;

		for (const part of pathParts) {
			if (current === null || typeof current !== 'object') return [];
			current = (current as Record<string, unknown>)[part];
		}

		if (Array.isArray(current)) {
			return current.filter(
				(r): r is App.UserRole => typeof r === 'string' && ALLOWED_ROLES.includes(r as App.UserRole)
			);
		}

		if (typeof current === 'string' && ALLOWED_ROLES.includes(current as App.UserRole)) {
			return [current as App.UserRole];
		}

		return [];
	} catch {
		return [];
	}
}

function parseJwtPayload(token: string): Record<string, unknown> {
	const parts = token.split('.');
	if (parts.length !== 3) return {};
	return JSON.parse(Buffer.from(parts[1], 'base64url').toString('utf8'));
}

export function extractUserFromTokens(
	tokens: client.TokenEndpointResponse,
	rolesClaimPath: string
): App.User | null {
	const idToken = tokens.id_token;
	if (!idToken) return null;

	const idClaims = parseJwtPayload(idToken);
	const sub = idClaims.sub as string | undefined;
	const email = (idClaims.email as string) ?? '';
	const name = (idClaims.name as string) ?? (idClaims.preferred_username as string) ?? '';

	const accessToken = tokens.access_token;
	const roles = extractRolesFromAccessToken(accessToken, rolesClaimPath);
	const role = roles[0];

	if (!sub || !role) return null;

	return {
		id: sub,
		email,
		name,
		role,
		idToken,
		accessToken
	};
}

export function getEndSessionUrl(config: client.Configuration, idTokenHint?: string): string {
	const { postLogoutRedirectUri } = getOidcEnv();
	const metadata = config.serverMetadata();
	const endSessionEndpoint = metadata.end_session_endpoint;

	if (!endSessionEndpoint) return postLogoutRedirectUri;

	const url = new URL(endSessionEndpoint);
	if (idTokenHint) url.searchParams.set('id_token_hint', idTokenHint);
	url.searchParams.set('post_logout_redirect_uri', postLogoutRedirectUri);

	return url.toString();
}
