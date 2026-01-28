import { writable, derived } from 'svelte/store';
import { browser } from '$app/environment';

export type UserRole = 'applicant' | 'processor';

export interface User {
	id: string;
	name: string;
	role: UserRole;
}

const ROLE_COOKIE_NAME = 'risk-management-user-role';

const defaultApplicant: User = {
	id: 'applicant-1',
	name: 'Max Mustermann',
	role: 'applicant'
};

const defaultProcessor: User = {
	id: 'processor-1',
	name: 'Anna Bearbeiter',
	role: 'processor'
};

function getCookie(name: string): string | null {
	if (!browser) return null;
	const cookies = document.cookie.split(';');
	for (const cookie of cookies) {
		const [cookieName, cookieValue] = cookie.trim().split('=');
		if (cookieName === name) {
			return cookieValue;
		}
	}
	return null;
}

function setCookie(name: string, value: string): void {
	if (!browser) return;
	// Set cookie with path=/ to make it available across all pages
	// Set SameSite=Lax for security while allowing same-site navigation
	// Set max-age to 1 year (31536000 seconds)
	document.cookie = `${name}=${value}; path=/; max-age=31536000; SameSite=Lax`;
}

function getInitialUser(): User {
	if (browser) {
		const savedRole = getCookie(ROLE_COOKIE_NAME);
		if (savedRole === 'processor') {
			return defaultProcessor;
		}
	}
	return defaultApplicant;
}

function createUserStore() {
	const { subscribe, set, update } = writable<User>(getInitialUser());

	return {
		subscribe,
		setRole: (role: UserRole) => {
			setCookie(ROLE_COOKIE_NAME, role);
			if (role === 'applicant') {
				set(defaultApplicant);
			} else {
				set(defaultProcessor);
			}
		},
		switchRole: () => {
			update((user) => {
				const newRole = user.role === 'applicant' ? 'processor' : 'applicant';
				setCookie(ROLE_COOKIE_NAME, newRole);
				if (newRole === 'processor') {
					return defaultProcessor;
				}
				return defaultApplicant;
			});
		}
	};
}

export const currentUser = createUserStore();

export const isApplicant = derived(currentUser, ($user) => $user.role === 'applicant');
export const isProcessor = derived(currentUser, ($user) => $user.role === 'processor');

export const debugMode = writable(false);
