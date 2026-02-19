// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		type UserRole = 'applicant' | 'processor' | 'admin'

		interface User {
			id: string
			name: string
			role: UserRole
			idToken?: string
		}

		interface Locals {
			user?: User
		}
	}
}

export {};
