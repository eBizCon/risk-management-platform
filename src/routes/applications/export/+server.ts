import { error } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { getApplicationsByUserForExport } from '$lib/server/services/repositories/application.repository';
import type { Application, ApplicationStatus, TrafficLight } from '$lib/server/db/schema';

const VALID_STATUSES = new Set<ApplicationStatus>(['draft', 'submitted', 'approved', 'rejected']);

const STATUS_LABELS: Record<ApplicationStatus, string> = {
	draft: 'Entwurf',
	submitted: 'Eingereicht',
	approved: 'Genehmigt',
	rejected: 'Abgelehnt'
};

const EMPLOYMENT_STATUS_LABELS: Record<string, string> = {
	employed: 'Angestellt',
	self_employed: 'Selbstständig',
	unemployed: 'Arbeitslos',
	retired: 'Ruhestand'
};

const TRAFFIC_LIGHT_LABELS: Record<TrafficLight, string> = {
	green: 'Grün',
	yellow: 'Gelb',
	red: 'Rot'
};

function formatDate(dateString: string | null): string {
	if (!dateString) return '';
	const date = new Date(dateString);
	const day = String(date.getDate()).padStart(2, '0');
	const month = String(date.getMonth() + 1).padStart(2, '0');
	const year = date.getFullYear();
	const hours = String(date.getHours()).padStart(2, '0');
	const minutes = String(date.getMinutes()).padStart(2, '0');
	return `${day}.${month}.${year} ${hours}:${minutes}`;
}

function escapeCSVField(value: string): string {
	if (value.includes(';') || value.includes('"') || value.includes('\n')) {
		return `"${value.replace(/"/g, '""')}"`;
	}
	return value;
}

function formatCurrency(value: number): string {
	return value.toFixed(2).replace('.', ',');
}

function generateApplicationsCSV(applications: Application[]): string {
	const BOM = '\uFEFF';
	const headers = ['Name', 'Beschäftigungsstatus', 'Status', 'Score', 'Ampel', 'Gewünschte Rate', 'Erstellt am'];
	const headerLine = headers.join(';');

	const rows = applications.map((app) => {
		const fields = [
			escapeCSVField(app.name),
			escapeCSVField(EMPLOYMENT_STATUS_LABELS[app.employmentStatus] ?? app.employmentStatus),
			escapeCSVField(STATUS_LABELS[app.status] ?? app.status),
			app.score !== null ? String(app.score) : '',
			app.trafficLight ? escapeCSVField(TRAFFIC_LIGHT_LABELS[app.trafficLight] ?? '') : '',
			formatCurrency(app.desiredRate),
			escapeCSVField(formatDate(app.createdAt))
		];
		return fields.join(';');
	});

	return BOM + [headerLine, ...rows].join('\r\n');
}

export const GET: RequestHandler = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	if (locals.user.role !== 'applicant') {
		throw error(403, 'Keine Berechtigung');
	}

	const statusParam = url.searchParams.get('status');
	let status: ApplicationStatus | undefined;

	if (statusParam) {
		if (!VALID_STATUSES.has(statusParam as ApplicationStatus)) {
			throw error(400, 'Ungültiger Status-Filter');
		}
		status = statusParam as ApplicationStatus;
	}

	const applications = await getApplicationsByUserForExport(locals.user.id, { status });
	const csv = generateApplicationsCSV(applications);

	return new Response(csv, {
		headers: {
			'Content-Type': 'text/csv; charset=utf-8',
			'Content-Disposition': 'attachment; filename="meine-antraege.csv"'
		}
	});
};
