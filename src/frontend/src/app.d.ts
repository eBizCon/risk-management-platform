// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		type UserRole = 'applicant' | 'processor' | 'risk_manager';

		interface User {
			id: string;
			email: string;
			name: string;
			role: UserRole;
			idToken?: string;
		}
	}
}

export {};
