import { afterEach, describe, expect, it } from 'vitest';
import * as oidcClient from 'openid-client';
import { buildLogoutUrl, extractRolesFromClaims, resetOidcConfigurationCache, setOidcConfigurationForTests } from './oidc';

const createConfig = (withEndSessionEndpoint: boolean) => {
  const metadata: oidcClient.ServerMetadata = {
    issuer: 'https://idp.example.com',
    authorization_endpoint: 'https://idp.example.com/auth',
    token_endpoint: 'https://idp.example.com/token',
    jwks_uri: 'https://idp.example.com/jwks',
    ...(withEndSessionEndpoint ? { end_session_endpoint: 'https://idp.example.com/logout' } : {})
  };

  return new oidcClient.Configuration(metadata, 'client-id');
};

const mockEnv = {
  issuer: 'https://idp.example.com',
  clientId: 'client-id',
  clientSecret: 'secret',
  redirectUri: 'https://app.example.com/auth/callback',
  postLogoutRedirectUri: 'https://app.example.com/logout/callback',
  scope: 'openid profile roles',
  rolesClaimPath: 'resource_access.roles'
};

describe('extractRolesFromClaims', () => {
  it('returns matching roles from array path', () => {
    const claims = { resource_access: { roles: ['applicant', 'other', 'processor'] } };
    const roles = extractRolesFromClaims(claims, 'resource_access.roles', ['applicant', 'processor']);
    expect(roles).toEqual(['applicant', 'processor']);
  });

  it('returns single role when string', () => {
    const claims = { resource_access: { role: 'processor' } };
    const roles = extractRolesFromClaims(claims, 'resource_access.role', ['applicant', 'processor']);
    expect(roles).toEqual(['processor']);
  });

  it('returns empty when path missing', () => {
    const claims = { other: {} };
    const roles = extractRolesFromClaims(claims, 'resource_access.roles', ['applicant', 'processor']);
    expect(roles).toEqual([]);
  });
});

describe('buildLogoutUrl', () => {
  it('returns null when end_session_endpoint missing', async () => {
    setOidcConfigurationForTests({ config: createConfig(false), env: mockEnv });
    const url = await buildLogoutUrl('id-token');
    expect(url).toBeNull();
  });

  it('returns logout url with params', async () => {
    setOidcConfigurationForTests({ config: createConfig(true), env: mockEnv });
    const url = await buildLogoutUrl('id-token');
    expect(url).toContain('post_logout_redirect_uri=');
    expect(url).toContain('id_token_hint=id-token');
  });
});

afterEach(() => {
  resetOidcConfigurationCache();
});
