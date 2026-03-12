import type { Application, ApplicationStatus } from '../db/schema';
import { getApplicationsForExport } from './repositories/application.repository';
import { employmentStatusLabels, statusLabels, trafficLightLabels } from '$lib/types';

const ALLOWED_EXPORT_STATUSES: ApplicationStatus[] = [
	'draft',
	'submitted',
	'needs_information',
	'resubmitted',
	'approved',
	'rejected'
];

const CSV_SEPARATOR = ';';
const UTF8_BOM = '\uFEFF';

interface CsvExportResult {
	success: true;
	csv: string;
	filename: string;
}

interface CsvExportError {
	success: false;
	reason: string;
	status: number;
}

export type CsvExportOutcome = CsvExportResult | CsvExportError;

const CSV_COLUMNS: { key: keyof Application; header: string }[] = [
	{ key: 'id', header: 'ID' },
	{ key: 'name', header: 'Name' },
	{ key: 'status', header: 'Status' },
	{ key: 'employmentStatus', header: 'Beschäftigungsstatus' },
	{ key: 'income', header: 'Einkommen' },
	{ key: 'fixedCosts', header: 'Fixkosten' },
	{ key: 'desiredRate', header: 'Gewünschte Rate' },
	{ key: 'hasPaymentDefault', header: 'Zahlungsausfall' },
	{ key: 'score', header: 'Score' },
	{ key: 'trafficLight', header: 'Ampel' },
	{ key: 'processorComment', header: 'Kommentar' },
	{ key: 'createdAt', header: 'Erstellt am' },
	{ key: 'submittedAt', header: 'Eingereicht am' },
	{ key: 'processedAt', header: 'Bearbeitet am' }
];

function escapeCsvField(value: string): string {
	if (value.includes(CSV_SEPARATOR) || value.includes('"') || value.includes('\n')) {
		return `"${value.replace(/"/g, '""')}"`;
	}
	return value;
}

function formatCellValue(key: keyof Application, value: unknown): string {
	if (value === null || value === undefined) {
		return '';
	}

	switch (key) {
		case 'status':
			return statusLabels[value as string] ?? String(value);
		case 'employmentStatus':
			return employmentStatusLabels[value as string] ?? String(value);
		case 'trafficLight':
			return trafficLightLabels[value as string] ?? String(value);
		case 'hasPaymentDefault':
			return value === true ? 'Ja' : 'Nein';
		default:
			return String(value);
	}
}

export function validateExportStatus(status?: string): CsvExportError | null {
	if (status && !ALLOWED_EXPORT_STATUSES.includes(status as ApplicationStatus)) {
		return {
			success: false,
			reason: `Ungültiger Status-Filter: ${status}`,
			status: 400
		};
	}
	return null;
}

export function validateProcessorRole(role?: string): CsvExportError | null {
	if (role !== 'processor') {
		return {
			success: false,
			reason: 'Nur Processor dürfen Exporte durchführen',
			status: 403
		};
	}
	return null;
}

export function buildCsvContent(applications: Application[]): string {
	const headerRow = CSV_COLUMNS.map((col) => escapeCsvField(col.header)).join(CSV_SEPARATOR);

	const dataRows = applications.map((app) =>
		CSV_COLUMNS.map((col) => escapeCsvField(formatCellValue(col.key, app[col.key]))).join(
			CSV_SEPARATOR
		)
	);

	return UTF8_BOM + [headerRow, ...dataRows].join('\r\n');
}

export function buildExportFilename(): string {
	const now = new Date();
	const yyyy = now.getFullYear();
	const mm = String(now.getMonth() + 1).padStart(2, '0');
	const dd = String(now.getDate()).padStart(2, '0');
	return `antraege-export-${yyyy}-${mm}-${dd}.csv`;
}

export async function requestCsvExport(params: {
	userRole?: string;
	status?: ApplicationStatus;
}): Promise<CsvExportOutcome> {
	const roleError = validateProcessorRole(params.userRole);
	if (roleError) {
		return roleError;
	}

	const statusError = validateExportStatus(params.status);
	if (statusError) {
		return statusError;
	}

	try {
		const applications = await getApplicationsForExport(params.status);
		const csv = buildCsvContent(applications);
		const filename = buildExportFilename();

		return { success: true, csv, filename };
	} catch {
		return {
			success: false,
			reason: 'Export konnte nicht generiert werden',
			status: 500
		};
	}
}
