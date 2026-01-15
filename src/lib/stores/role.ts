import { writable, derived } from 'svelte/store';
import { browser } from '$app/environment';

export type UserRole = 'applicant' | 'processor';

export interface User {
	id: string;
	name: string;
	role: UserRole;
}

const ROLE_STORAGE_KEY = 'risk-management-user-role';

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

function getInitialUser(): User {
	if (browser) {
		const savedRole = localStorage.getItem(ROLE_STORAGE_KEY);
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
			if (browser) {
				localStorage.setItem(ROLE_STORAGE_KEY, role);
			}
			if (role === 'applicant') {
				set(defaultApplicant);
			} else {
				set(defaultProcessor);
			}
		},
		switchRole: () => {
			update((user) => {
				const newRole = user.role === 'applicant' ? 'processor' : 'applicant';
				if (browser) {
					localStorage.setItem(ROLE_STORAGE_KEY, newRole);
				}
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

export const debugMode = writable(true);
