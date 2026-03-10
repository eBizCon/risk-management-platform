import { count } from 'drizzle-orm';
import type { NodePgDatabase } from 'drizzle-orm/node-postgres';
import { applications } from './schema';
import type { ApplicationStatus, EmploymentStatus, NewApplication } from './schema';
import { calculateScore } from '../services/scoring';

interface SeedApplicationTemplate {
	name: string;
	income: number;
	fixedCosts: number;
	desiredRate: number;
	employmentStatus: EmploymentStatus;
	hasPaymentDefault: boolean;
}

const SEED_CREATED_BY = 'applicant@example.com';

const STATUSES: ApplicationStatus[] = ['draft', 'submitted', 'approved', 'rejected'];

const APPROVED_COMMENTS = [
	'Bonität geprüft, Antrag genehmigt.',
	'Einkommensverhältnisse wurden positiv bewertet.',
	'Antrag nach manueller Prüfung freigegeben.',
	'Risiko ist gering, Genehmigung erteilt.'
];

const REJECTED_COMMENTS = [
	'Zu hohes Risiko aufgrund bestehender Zahlungsverzüge.',
	'Verfügbares Einkommen reicht für die gewünschte Rate nicht aus.',
	'Kritisches Verhältnis zwischen Einkommen und Fixkosten.',
	'Antrag aufgrund negativer Gesamtbewertung abgelehnt.'
];

const SEED_TEMPLATES: SeedApplicationTemplate[] = [
	{
		name: 'Max Mustermann',
		income: 5200,
		fixedCosts: 1900,
		desiredRate: 700,
		employmentStatus: 'employed',
		hasPaymentDefault: false
	},
	{
		name: 'Sofia Wagner',
		income: 4000,
		fixedCosts: 2200,
		desiredRate: 700,
		employmentStatus: 'self_employed',
		hasPaymentDefault: false
	},
	{
		name: 'Jonas Becker',
		income: 3300,
		fixedCosts: 2100,
		desiredRate: 500,
		employmentStatus: 'employed',
		hasPaymentDefault: false
	},
	{
		name: 'Elena Fischer',
		income: 2800,
		fixedCosts: 2100,
		desiredRate: 450,
		employmentStatus: 'unemployed',
		hasPaymentDefault: true
	},
	{
		name: 'Noah Klein',
		income: 6100,
		fixedCosts: 2400,
		desiredRate: 850,
		employmentStatus: 'employed',
		hasPaymentDefault: false
	},
	{
		name: 'Mila Schmitt',
		income: 3900,
		fixedCosts: 1900,
		desiredRate: 850,
		employmentStatus: 'retired',
		hasPaymentDefault: false
	},
	{
		name: 'Paul Neumann',
		income: 3100,
		fixedCosts: 2300,
		desiredRate: 420,
		employmentStatus: 'self_employed',
		hasPaymentDefault: true
	},
	{
		name: 'Lea Hartmann',
		income: 2700,
		fixedCosts: 2200,
		desiredRate: 300,
		employmentStatus: 'unemployed',
		hasPaymentDefault: false
	}
];

const buildSeedRows = (): Omit<NewApplication, 'id'>[] => {
	const rows: Omit<NewApplication, 'id'>[] = [];
	const totalRows = 32;
	const now = Date.now();

	for (let index = 0; index < totalRows; index += 1) {
		const template = SEED_TEMPLATES[index % SEED_TEMPLATES.length];
		const status = STATUSES[index % STATUSES.length];
		const scoring = calculateScore(
			template.income,
			template.fixedCosts,
			template.desiredRate,
			template.employmentStatus,
			template.hasPaymentDefault
		);

		const createdAtDate = new Date(now - (totalRows - index) * 24 * 60 * 60 * 1000);
		const submittedAtDate = new Date(createdAtDate.getTime() + 2 * 60 * 60 * 1000);
		const processedAtDate = new Date(submittedAtDate.getTime() + 6 * 60 * 60 * 1000);

		const processorComment =
			status === 'approved'
				? APPROVED_COMMENTS[index % APPROVED_COMMENTS.length]
				: status === 'rejected'
					? REJECTED_COMMENTS[index % REJECTED_COMMENTS.length]
					: null;

		rows.push({
			name: `${template.name} ${Math.floor(index / SEED_TEMPLATES.length) + 1}`,
			income: template.income,
			fixedCosts: template.fixedCosts,
			desiredRate: template.desiredRate,
			employmentStatus: template.employmentStatus,
			hasPaymentDefault: template.hasPaymentDefault,
			status,
			score: scoring.score,
			trafficLight: scoring.trafficLight,
			scoringReasons: JSON.stringify(scoring.reasons),
			processorComment,
			createdAt: createdAtDate.toISOString(),
			submittedAt: status === 'draft' ? null : submittedAtDate.toISOString(),
			processedAt:
				status === 'approved' || status === 'rejected' ? processedAtDate.toISOString() : null,
			createdBy: SEED_CREATED_BY
		});
	}

	return rows;
};

export async function seedDatabase(database: NodePgDatabase<Record<string, unknown>>): Promise<void> {
	const [existingCount] = await database.select({ value: count() }).from(applications);

	if ((existingCount?.value ?? 0) > 0) {
		return;
	}

	const rows = buildSeedRows();
	await database.insert(applications).values(rows);
}
