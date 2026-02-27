import { env } from '$env/dynamic/private';

export interface JHipsterCustomer {
	id: number;
	bankAccountName: string;
	balance: number;
	userLogin: string;
	userFirstName: string;
	userLastName: string;
	userEmail: string;
}

function getBaseUrl(): string {
	return env.JHIPSTER_API_URL || 'http://localhost:8080';
}

export async function fetchCustomers(): Promise<JHipsterCustomer[]> {
	const baseUrl = getBaseUrl();
	const response = await fetch(`${baseUrl}/api/integration/customers`);

	if (!response.ok) {
		throw new Error(`Failed to fetch customers from JHipster: ${response.status} ${response.statusText}`);
	}

	return response.json();
}

export async function fetchCustomerById(id: number): Promise<JHipsterCustomer | null> {
	const baseUrl = getBaseUrl();
	const response = await fetch(`${baseUrl}/api/integration/customers/${id}`);

	if (response.status === 404) {
		return null;
	}

	if (!response.ok) {
		throw new Error(`Failed to fetch customer from JHipster: ${response.status} ${response.statusText}`);
	}

	return response.json();
}
