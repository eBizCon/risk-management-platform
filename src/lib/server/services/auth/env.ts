import { z } from 'zod';
import {
	OIDC_ISSUER,
	OIDC_CLIENT_ID,
	OIDC_REDIRECT_URI,
	OIDC_POST_LOGOUT_REDIRECT_URI,
	OIDC_SCOPE,
	OIDC_ROLES_CLAIM_PATH
} from '$env/static/private';
import { env as privateEnv } from '$env/dynamic/private';

const oidcEnvSchema = z.object({
  OIDC_ISSUER: z.string().url(),
  OIDC_CLIENT_ID: z.string().min(1),
  OIDC_CLIENT_SECRET: z.string().optional(),
  OIDC_REDIRECT_URI: z.string().url(),
  OIDC_POST_LOGOUT_REDIRECT_URI: z.string().url(),
  OIDC_SCOPE: z.string().min(1),
  OIDC_ROLES_CLAIM_PATH: z.string().min(1)
});

export const OIDC_ALLOWED_ROLES = ['applicant', 'processor', 'admin'] as const;

export type OidcEnvConfig = {
  issuer: string;
  clientId: string;
  clientSecret?: string;
  redirectUri: string;
  postLogoutRedirectUri: string;
  scope: string;
  rolesClaimPath: string;
};

export const getOidcEnv = (): OidcEnvConfig => {
  const env = oidcEnvSchema.parse({
    OIDC_ISSUER,
    OIDC_CLIENT_ID,
    OIDC_CLIENT_SECRET: privateEnv.OIDC_CLIENT_SECRET,
    OIDC_REDIRECT_URI,
    OIDC_POST_LOGOUT_REDIRECT_URI,
    OIDC_SCOPE,
    OIDC_ROLES_CLAIM_PATH
  });

  return {
    issuer: env.OIDC_ISSUER,
    clientId: env.OIDC_CLIENT_ID,
    clientSecret: env.OIDC_CLIENT_SECRET,
    redirectUri: env.OIDC_REDIRECT_URI,
    postLogoutRedirectUri: env.OIDC_POST_LOGOUT_REDIRECT_URI,
    scope: env.OIDC_SCOPE,
    rolesClaimPath: env.OIDC_ROLES_CLAIM_PATH
  };
};
