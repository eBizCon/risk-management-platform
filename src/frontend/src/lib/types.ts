export type ApplicationStatus =
	| 'draft'
	| 'submitted'
	| 'needs_information'
	| 'resubmitted'
	| 'approved'
	| 'rejected'
	| 'processing'
	| 'failed';
export type EmploymentStatus = 'employed' | 'self_employed' | 'unemployed' | 'retired';
export type TrafficLight = 'red' | 'yellow' | 'green';
export type UserRole = 'applicant' | 'processor' | 'risk_manager';

export type CustomerStatus = 'active' | 'archived';

export interface Customer {
	id: number;
	firstName: string;
	lastName: string;
	email: string | null;
	phone: string;
	dateOfBirth: string;
	street: string;
	city: string;
	zipCode: string;
	country: string;
	employmentStatus: EmploymentStatus;
	status: CustomerStatus;
	createdBy: string;
	createdAt: string;
	updatedAt: string | null;
}

export interface Application {
	id: number;
	customerId: number;
	customerName: string | null;
	income: number;
	fixedCosts: number;
	desiredRate: number;
	loanAmount: number | null;
	loanTerm: number | null;
	employmentStatus: EmploymentStatus;
	hasPaymentDefault: boolean | null;
	creditScore: number | null;
	status: ApplicationStatus;
	score: number | null;
	trafficLight: TrafficLight | null;
	scoringReasons: string | null;
	processorComment: string | null;
	failureReason: string | null;
	createdAt: string;
	submittedAt: string | null;
	processedAt: string | null;
	createdBy: string;
}

export interface User {
	id: string;
	email: string;
	name: string;
	role: UserRole;
}

export interface DashboardStats {
	total: number;
	draft: number;
	submitted: number;
	approved: number;
	rejected: number;
}

export interface ApplicationInquiry {
	id: number;
	applicationId: number;
	inquiryText: string;
	status: string;
	processorEmail: string;
	responseText: string | null;
	createdAt: string;
	respondedAt: string | null;
}

export const employmentStatusLabels: Record<string, string> = {
	employed: 'Angestellt',
	self_employed: 'Selbstständig',
	unemployed: 'Arbeitslos',
	retired: 'Ruhestand'
};

export const statusLabels: Record<string, string> = {
	draft: 'Entwurf',
	submitted: 'Eingereicht',
	needs_information: 'Rückfrage offen',
	resubmitted: 'Erneut eingereicht',
	approved: 'Genehmigt',
	rejected: 'Abgelehnt',
	processing: 'Wird verarbeitet',
	failed: 'Fehlgeschlagen'
};

export const trafficLightLabels: Record<string, string> = {
	green: 'Positiv',
	yellow: 'Prüfung erforderlich',
	red: 'Kritisch'
};

export const customerStatusLabels: Record<string, string> = {
	active: 'Aktiv',
	archived: 'Archiviert'
};
