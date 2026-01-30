import { describe, expect, it } from 'vitest';
import type { Application } from '$lib/server/db/schema';
import { buildApplicationsCsv } from './applicationCsvExport';

describe('buildApplicationsCsv', () => {
	it('adds UTF-8 BOM and German header row', () => {
		const csv = buildApplicationsCsv([]);
		expect(csv.startsWith('\uFEFF')).toBe(true);
		expect(csv).toContain('ID;Name;Einkommen');
	});

	it('escapes separators, quotes, and line breaks', () => {
		const apps: Application[] = [
			{
				id: 1,
				name: 'A;"B"\nC',
				income: 1000,
				fixedCosts: 100,
				desiredRate: 50,
				employmentStatus: 'employed',
				hasPaymentDefault: false,
				status: 'submitted',
				score: 80,
				trafficLight: 'green',
				scoringReasons: null,
				processorComment: null,
				createdAt: '2026-01-30T10:00:00.000Z',
				submittedAt: null,
				processedAt: null,
				createdBy: 'u1'
			}
		];

		const csv = buildApplicationsCsv(apps);
		expect(csv).toContain('"A;""B""\nC"');
	});

	it('uses German labels for enums', () => {
		const apps: Application[] = [
			{
				id: 1,
				name: 'Test',
				income: 1000,
				fixedCosts: 100,
				desiredRate: 50,
				employmentStatus: 'self_employed',
				hasPaymentDefault: true,
				status: 'approved',
				score: null,
				trafficLight: 'yellow',
				scoringReasons: null,
				processorComment: null,
				createdAt: '2026-01-30T10:00:00.000Z',
				submittedAt: null,
				processedAt: null,
				createdBy: 'u1'
			}
		];

		const csv = buildApplicationsCsv(apps);
		expect(csv).toContain('Selbstständig');
		expect(csv).toContain('Genehmigt');
		expect(csv).toContain('Prüfung erforderlich');
	});
});
