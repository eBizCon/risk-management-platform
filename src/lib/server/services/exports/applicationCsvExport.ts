import type { Application } from '$lib/server/db/schema';
import { employmentStatusLabels, statusLabels, trafficLightLabels } from '$lib/types';

function escapeCsvCell(value: string): string {
	if (value.includes(';') || value.includes('"') || value.includes('\n') || value.includes('\r')) {
		return `"${value.replaceAll('"', '""')}"`;
	}
	return value;
}

function formatDateTimeDe(value: string | null): string {
	if (!value) return '';
	const date = new Date(value);
	if (Number.isNaN(date.getTime())) return '';
	return date.toLocaleString('de-DE', {
		year: 'numeric',
		month: '2-digit',
		day: '2-digit',
		hour: '2-digit',
		minute: '2-digit'
	});
}

function boolToDe(value: boolean): string {
	return value ? 'Ja' : 'Nein';
}

export function buildApplicationsCsv(apps: Application[]): string {
	const header = [
		'ID',
		'Name',
		'Einkommen',
		'Fixkosten',
		'Gewünschte Rate',
		'Beschäftigungsstatus',
		'Zahlungsausfall',
		'Status',
		'Score',
		'Ampel',
		'Scoring Gründe',
		'Kommentar (Sachbearbeitung)',
		'Erstellt am',
		'Eingereicht am',
		'Bearbeitet am',
		'Erstellt von'
	];

	const rows = apps.map((a) => {
		const score = a.score === null ? '' : String(a.score);
		const trafficLight = a.trafficLight ? (trafficLightLabels[a.trafficLight] ?? a.trafficLight) : '';
		const employment = employmentStatusLabels[a.employmentStatus] ?? a.employmentStatus;
		const status = statusLabels[a.status] ?? a.status;

		return [
			String(a.id),
			a.name,
			String(a.income),
			String(a.fixedCosts),
			String(a.desiredRate),
			employment,
			boolToDe(a.hasPaymentDefault),
			status,
			score,
			trafficLight,
			a.scoringReasons ?? '',
			a.processorComment ?? '',
			formatDateTimeDe(a.createdAt),
			formatDateTimeDe(a.submittedAt),
			formatDateTimeDe(a.processedAt),
			a.createdBy
		].map(escapeCsvCell).join(';');
	});

	return `\uFEFF${[header.join(';'), ...rows].join('\n')}\n`;
}
