export type ApplicationStatus =
	| 'draft'
	| 'submitted'
	| 'needs_information'
	| 'resubmitted'
	| 'approved'
	| 'rejected';
export type EmploymentStatus = 'employed' | 'self_employed' | 'unemployed' | 'retired';
export type TrafficLight = 'red' | 'yellow' | 'green';
export type UserRole = 'applicant' | 'processor' | 'risk_manager';

export interface Application {
	id: number;
	name: string;
	income: number;
	fixedCosts: number;
	desiredRate: number;
	employmentStatus: EmploymentStatus;
	hasPaymentDefault: boolean;
	status: ApplicationStatus;
	score: number | null;
	trafficLight: TrafficLight | null;
	scoringReasons: string | null;
	processorComment: string | null;
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
	rejected: 'Abgelehnt'
};

export const trafficLightLabels: Record<string, string> = {
	green: 'Positiv',
	yellow: 'Prüfung erforderlich',
	red: 'Kritisch'
};
