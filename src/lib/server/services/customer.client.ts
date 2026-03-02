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
	const response = await fetch(`${baseUrl}/api/public/customers`);

	if (!response.ok) {
		console.error(`Failed to fetch customers from JHipster: ${response.status} ${response.statusText}`);
		return [];
	}

	return response.json();
}

export async function fetchCustomerById(id: number): Promise<Customer | null> {
	const baseUrl = getJhipsterUrl();
	const response = await fetch(`${baseUrl}/api/public/customers/${id}`);

	if (!response.ok) {
		if (response.status === 404) {
			return null;
		}
		console.error(`Failed to fetch customer ${id} from JHipster: ${response.status} ${response.statusText}`);
		return null;
	}

	return response.json();
}
