import { describe, it, expect, vi, beforeEach } from 'vitest';
import type { Application } from '../db/schema';
import {
	buildCsvContent,
	buildExportFilename,
	validateProcessorRole,
	validateExportStatus,
	requestCsvExport
} from './csv-export.service';

vi.mock('./repositories/application.repository', () => ({
	getApplicationsForExport: vi.fn()
}));

import { getApplicationsForExport } from './repositories/application.repository';

const mockGetApplicationsForExport = vi.mocked(getApplicationsForExport);

function makeApplication(overrides: Partial<Application> = {}): Application {
	return {
		id: 1,
		name: 'Max Mustermann',
		income: 4000,
		fixedCosts: 1500,
		desiredRate: 500,
		employmentStatus: 'employed',
		hasPaymentDefault: false,
		status: 'submitted',
		score: 75,
		trafficLight: 'green',
		scoringReasons: null,
		processorComment: null,
		createdAt: '2025-03-10T10:00:00.000Z',
		submittedAt: '2025-03-10T11:00:00.000Z',
		processedAt: null,
		createdBy: 'applicant@example.com',
		...overrides
	};
}

describe('validateProcessorRole', () => {
	it('returns null for processor role', () => {
		expect(validateProcessorRole('processor')).toBeNull();
	});

	it('returns error for applicant role', () => {
		const result = validateProcessorRole('applicant');
		expect(result).not.toBeNull();
		expect(result!.status).toBe(403);
	});

	it('returns error for undefined role', () => {
		const result = validateProcessorRole(undefined);
		expect(result).not.toBeNull();
		expect(result!.status).toBe(403);
	});
});

describe('validateExportStatus', () => {
	it('returns null for undefined status (no filter)', () => {
		expect(validateExportStatus(undefined)).toBeNull();
	});

	it.each(['draft', 'submitted', 'needs_information', 'resubmitted', 'approved', 'rejected'])(
		'returns null for valid status: %s',
		(status) => {
			expect(validateExportStatus(status)).toBeNull();
		}
	);

	it('returns error for invalid status', () => {
		const result = validateExportStatus('invalid_status');
		expect(result).not.toBeNull();
		expect(result!.status).toBe(400);
	});
});

describe('buildCsvContent', () => {
	it('produces UTF-8 BOM at the start', () => {
		const csv = buildCsvContent([]);
		expect(csv.charCodeAt(0)).toBe(0xfeff);
	});

	it('contains semicolon-separated header row', () => {
		const csv = buildCsvContent([]);
		const firstLine = csv.replace('\uFEFF', '').split('\r\n')[0];
		expect(firstLine).toBe(
			'ID;Name;Status;Beschäftigungsstatus;Einkommen;Fixkosten;Gewünschte Rate;Zahlungsausfall;Score;Ampel;Kommentar;Erstellt am;Eingereicht am;Bearbeitet am'
		);
	});

	it('maps application data to CSV row with German labels', () => {
		const app = makeApplication({
			status: 'submitted',
			employmentStatus: 'employed',
			trafficLight: 'green',
			hasPaymentDefault: false
		});

		const csv = buildCsvContent([app]);
		const lines = csv.replace('\uFEFF', '').split('\r\n');
		expect(lines).toHaveLength(2);

		const dataRow = lines[1];
		expect(dataRow).toContain('Eingereicht');
		expect(dataRow).toContain('Angestellt');
		expect(dataRow).toContain('Positiv');
		expect(dataRow).toContain('Nein');
	});

	it('renders hasPaymentDefault as Ja/Nein', () => {
		const appYes = makeApplication({ hasPaymentDefault: true });
		const appNo = makeApplication({ id: 2, hasPaymentDefault: false });

		const csv = buildCsvContent([appYes, appNo]);
		const lines = csv.replace('\uFEFF', '').split('\r\n');
		expect(lines[1]).toContain('Ja');
		expect(lines[2]).toContain('Nein');
	});

	it('renders null values as empty strings', () => {
		const app = makeApplication({ processorComment: null, processedAt: null });
		const csv = buildCsvContent([app]);
		const lines = csv.replace('\uFEFF', '').split('\r\n');
		const fields = lines[1].split(';');
		const commentIndex = 10;
		const processedAtIndex = 13;
		expect(fields[commentIndex]).toBe('');
		expect(fields[processedAtIndex]).toBe('');
	});

	it('escapes fields containing semicolons', () => {
		const app = makeApplication({ processorComment: 'Halb;fertig' });
		const csv = buildCsvContent([app]);
		expect(csv).toContain('"Halb;fertig"');
	});

	it('escapes fields containing double quotes', () => {
		const app = makeApplication({ processorComment: 'Sagt "Hallo"' });
		const csv = buildCsvContent([app]);
		expect(csv).toContain('"Sagt ""Hallo"""');
	});
});

describe('buildExportFilename', () => {
	it('follows the pattern antraege-export-YYYY-MM-DD.csv', () => {
		const filename = buildExportFilename();
		expect(filename).toMatch(/^antraege-export-\d{4}-\d{2}-\d{2}\.csv$/);
	});

	it('uses today date', () => {
		const now = new Date();
		const expected = `antraege-export-${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}.csv`;
		expect(buildExportFilename()).toBe(expected);
	});
});

describe('requestCsvExport', () => {
	beforeEach(() => {
		vi.resetAllMocks();
	});

	it('returns 403 if role is not processor', async () => {
		const result = await requestCsvExport({ userRole: 'applicant' });
		expect(result.success).toBe(false);
		if (!result.success) {
			expect(result.status).toBe(403);
		}
	});

	it('returns 400 for invalid status', async () => {
		const result = await requestCsvExport({
			userRole: 'processor',
			status: 'invalid' as never
		});
		expect(result.success).toBe(false);
		if (!result.success) {
			expect(result.status).toBe(400);
		}
	});

	it('returns CSV and filename on success', async () => {
		mockGetApplicationsForExport.mockResolvedValue([makeApplication()]);

		const result = await requestCsvExport({ userRole: 'processor', status: 'submitted' });
		expect(result.success).toBe(true);
		if (result.success) {
			expect(result.csv).toContain('\uFEFF');
			expect(result.csv).toContain('Max Mustermann');
			expect(result.filename).toMatch(/^antraege-export-.*\.csv$/);
		}
	});

	it('passes status filter to repository', async () => {
		mockGetApplicationsForExport.mockResolvedValue([]);

		await requestCsvExport({ userRole: 'processor', status: 'approved' });
		expect(mockGetApplicationsForExport).toHaveBeenCalledWith('approved');
	});

	it('passes undefined status to repository when no filter', async () => {
		mockGetApplicationsForExport.mockResolvedValue([]);

		await requestCsvExport({ userRole: 'processor' });
		expect(mockGetApplicationsForExport).toHaveBeenCalledWith(undefined);
	});

	it('returns 500 on repository error', async () => {
		mockGetApplicationsForExport.mockRejectedValue(new Error('DB connection failed'));

		const result = await requestCsvExport({ userRole: 'processor' });
		expect(result.success).toBe(false);
		if (!result.success) {
			expect(result.status).toBe(500);
		}
	});
});
