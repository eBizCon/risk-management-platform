import { env } from '$env/dynamic/private';
import crypto from 'node:crypto';
import type { Cookies } from '@sveltejs/kit';

const COOKIE_NAME = 'session';
const ALGORITHM = 'aes-256-gcm';
const IV_LENGTH = 12;
const TAG_LENGTH = 16;
const SESSION_MAX_AGE_SECONDS = 3600;

function getEncryptionKey(): Buffer {
	const secret = env.SESSION_SECRET;
	if (!secret || secret.length < 32) {
		throw new Error('SESSION_SECRET must be set and at least 32 characters long');
	}
	return crypto.createHash('sha256').update(secret).digest();
}

function encrypt(plaintext: string): string {
	const key = getEncryptionKey();
	const iv = crypto.randomBytes(IV_LENGTH);
	const cipher = crypto.createCipheriv(ALGORITHM, key, iv);

	const encrypted = Buffer.concat([cipher.update(plaintext, 'utf8'), cipher.final()]);
	const tag = cipher.getAuthTag();

	const combined = Buffer.concat([iv, tag, encrypted]);
	return combined.toString('base64url');
}

function decrypt(ciphertext: string): string {
	const key = getEncryptionKey();
	const combined = Buffer.from(ciphertext, 'base64url');

	const iv = combined.subarray(0, IV_LENGTH);
	const tag = combined.subarray(IV_LENGTH, IV_LENGTH + TAG_LENGTH);
	const encrypted = combined.subarray(IV_LENGTH + TAG_LENGTH);

	const decipher = crypto.createDecipheriv(ALGORITHM, key, iv);
	decipher.setAuthTag(tag);

	const decrypted = Buffer.concat([decipher.update(encrypted), decipher.final()]);
	return decrypted.toString('utf8');
}

export interface SessionData {
	user: App.User;
	expiresAt: number;
}

export function createSessionCookie(cookies: Cookies, user: App.User): void {
	const session: SessionData = {
		user,
		expiresAt: Date.now() + SESSION_MAX_AGE_SECONDS * 1000
	};

	const encrypted = encrypt(JSON.stringify(session));

	const isProduction = env.NODE_ENV === 'production';
	cookies.set(COOKIE_NAME, encrypted, {
		path: '/',
		httpOnly: true,
		sameSite: 'lax',
		secure: isProduction,
		maxAge: SESSION_MAX_AGE_SECONDS
	});
}

export function readSessionCookie(cookies: Cookies): App.User | null {
	const value = cookies.get(COOKIE_NAME);
	if (!value) return null;

	try {
		const session: SessionData = JSON.parse(decrypt(value));
		if (session.expiresAt < Date.now()) return null;
		return session.user;
	} catch {
		return null;
	}
}

export function clearSessionCookie(cookies: Cookies): void {
	cookies.delete(COOKIE_NAME, { path: '/' });
}
