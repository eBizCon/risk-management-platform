import { writable, derived } from 'svelte/store';

export type UserRole = 'applicant' | 'processor';

export interface User {
	id: string;
	name: string;
	role: UserRole;
}

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

function createUserStore() {
	const { subscribe, set, update } = writable<User>(defaultApplicant);

	return {
		subscribe,
		setRole: (role: UserRole) => {
			if (role === 'applicant') {
				set(defaultApplicant);
			} else {
				set(defaultProcessor);
			}
		},
		switchRole: () => {
			update((user) => {
				if (user.role === 'applicant') {
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
