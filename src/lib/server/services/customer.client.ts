import { env } from '$env/dynamic/private';

export interface Customer {
	id: number;
	name: string;
	balance: number;
	userLogin: string | null;
}

function getJhipsterUrl(): string {
	return env.JHIPSTER_URL || 'http://localhost:8080';
}

export async function fetchCustomers(): Promise<Customer[]> {
	const baseUrl = getJhipsterUrl();
	try {
		const response = await fetch(`${baseUrl}/api/public/customers`);

		if (!response.ok) {
			console.error(`Failed to fetch customers from JHipster: ${response.status} ${response.statusText}`);
			return [];
		}

		return response.json();
	} catch (err) {
		console.error('Failed to connect to JHipster:', err);
		return [];
	}
}

export async function fetchCustomerById(id: number): Promise<Customer | null> {
	const baseUrl = getJhipsterUrl();
	try {
		const response = await fetch(`${baseUrl}/api/public/customers/${id}`);

		if (!response.ok) {
			if (response.status === 404) {
				return null;
			}
			console.error(`Failed to fetch customer ${id} from JHipster: ${response.status} ${response.statusText}`);
			return null;
		}

		return response.json();
	} catch (err) {
		console.error(`Failed to connect to JHipster for customer ${id}:`, err);
		return null;
	}
}
