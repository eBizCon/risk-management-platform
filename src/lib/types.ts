export type { Application, ApplicationStatus, EmploymentStatus, TrafficLight } from './server/db/schema';
export type { UserRole, User } from './stores/role';

export const employmentStatusLabels: Record<string, string> = {
	employed: 'Angestellt',
	self_employed: 'Selbstständig',
	unemployed: 'Arbeitslos',
	retired: 'Ruhestand'
};

export const statusLabels: Record<string, string> = {
	draft: 'Entwurf',
	submitted: 'Eingereicht',
	approved: 'Genehmigt',
	rejected: 'Abgelehnt'
};

export const trafficLightLabels: Record<string, string> = {
	green: 'Positiv',
	yellow: 'Prüfung erforderlich',
	red: 'Kritisch'
};
