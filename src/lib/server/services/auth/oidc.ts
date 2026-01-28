import { dev } from '$app/environment';
import * as client from 'openid-client';
import { OIDC_ALLOWED_ROLES, getOidcEnv, type OidcEnvConfig } from './env';

export type AuthorizationRequest = {
	url: string;
	codeVerifier: string;
	state: string;
};

export type TokenExchangeResult = {
	tokens: client.TokenEndpointResponse & client.TokenEndpointResponseHelpers;
	idToken: string;
	roles: App.UserRole[];
};

type OidcConfiguration = {
	config: client.Configuration;
	env: OidcEnvConfig;
};

let configCache: Promise<OidcConfiguration> | null = null;

export const getOidcConfiguration = async (): Promise<OidcConfiguration> => {
	if (!configCache) {
		configCache = (async () => {
			const env = getOidcEnv();
			const configuration = await client.discovery(
				new URL(env.issuer),
				env.clientId,
				env.clientSecret,
				undefined,
				dev ? { execute: [client.allowInsecureRequests] } : undefined
			);
			return { config: configuration, env };
		})();
	}

	return configCache;
};

export const createAuthorizationRequest = async (): Promise<AuthorizationRequest> => {
	const { config, env } = await getOidcConfiguration();

	const codeVerifier = client.randomPKCECodeVerifier();
	const codeChallenge = await client.calculatePKCECodeChallenge(codeVerifier);
	const state = client.randomState();

	const url = client.buildAuthorizationUrl(config, {
		redirect_uri: env.redirectUri,
		scope: env.scope,
		code_challenge: codeChallenge,
		code_challenge_method: 'S256',
		state
	});

	return { url: url.toString(), codeVerifier, state };
};

export const exchangeAuthorizationCode = async (
	request: Request,
	params: { codeVerifier: string; expectedState: string }
): Promise<TokenExchangeResult> => {
	const { config, env } = await getOidcConfiguration();

	const tokens = await client.authorizationCodeGrant(config, request, {
		pkceCodeVerifier: params.codeVerifier,
		expectedState: params.expectedState
	});

	const idToken = tokens.id_token;
	if (!idToken) {
		throw new Error('Missing id_token in token response');
	}

	const accessToken = tokens.access_token;
	if (!accessToken) {
		throw new Error('Missing access_token in token response');
	}

	const accessClaims = decodeJwtPayload(accessToken);
	const roles = extractRolesFromClaims(accessClaims, env.rolesClaimPath, OIDC_ALLOWED_ROLES);
	return { tokens, idToken, roles };
};

export const buildLogoutUrl = async (idToken: string): Promise<string | null> => {
	const { config, env } = await getOidcConfiguration();
	const metadata = config.serverMetadata?.();

	if (!metadata?.end_session_endpoint) {
		return null;
	}

	const url = client.buildEndSessionUrl(config, {
		post_logout_redirect_uri: env.postLogoutRedirectUri,
		id_token_hint: idToken
	});

	return url.toString();
};

const decodeJwtPayload = (token: string): unknown => {
	const parts = token.split('.');
	if (parts.length < 2) {
		throw new Error('Invalid JWT format for access_token');
	}

	const payload = parts[1];
	const padded = payload.padEnd(payload.length + ((4 - (payload.length % 4)) % 4), '=');
	const normalized = padded.replace(/-/g, '+').replace(/_/g, '/');
	const json = Buffer.from(normalized, 'base64').toString('utf8');
	return JSON.parse(json) as unknown;
};

export const extractRolesFromClaims = (
	claims: unknown,
	claimPath: string,
	allowedRoles: readonly string[]
): App.UserRole[] => {
	if (!claims || typeof claimPath !== 'string' || claimPath.length === 0) {
		return [];
	}

	const parts = claimPath.split('.');
	let value: unknown = claims as Record<string, unknown>;

	for (const part of parts) {
		if (!value || typeof value !== 'object' || Array.isArray(value)) {
			return [];
		}

		value = (value as Record<string, unknown>)[part];
	}

	if (Array.isArray(value)) {
		const normalized = value
			.map((entry) => normalizeRoleValue(entry))
			.filter((role): role is string => Boolean(role));

		return normalized.filter((role): role is App.UserRole => allowedRoles.includes(role));
	}

	const normalized = normalizeRoleValue(value);
	if (!normalized) {
		return [];
	}

	return allowedRoles.includes(normalized) ? [normalized as App.UserRole] : [];
};

const normalizeRoleValue = (value: unknown): string | null => {
	if (typeof value === 'string') {
		return value;
	}

	return null;
};

export const resetOidcConfigurationCache = (): void => {
	configCache = null;
};

export const setOidcConfigurationForTests = (configuration: OidcConfiguration | null): void => {
	configCache = configuration ? Promise.resolve(configuration) : null;
};
