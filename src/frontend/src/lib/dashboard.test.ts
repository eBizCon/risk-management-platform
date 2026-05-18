import { describe, it, expect } from 'vitest';
import { computePercentages, statusEntries } from './dashboard';
import type { DashboardStats } from './types';

describe('computePercentages', () => {
	it('should return correct percentages for typical data', () => {
		const stats: DashboardStats = { total: 12, draft: 4, submitted: 3, approved: 3, rejected: 2 };
		const result = computePercentages(stats);

		expect(result.draft).toBe(33);
		expect(result.submitted).toBe(25);
		expect(result.approved).toBe(25);
		expect(result.rejected).toBe(17);
		expect(result.draft + result.submitted + result.approved + result.rejected).toBe(100);
	});

	it('should return all zeros when total is 0', () => {
		const stats: DashboardStats = { total: 0, draft: 0, submitted: 0, approved: 0, rejected: 0 };
		const result = computePercentages(stats);

		expect(result.draft).toBe(0);
		expect(result.submitted).toBe(0);
		expect(result.approved).toBe(0);
		expect(result.rejected).toBe(0);
	});

	it('should handle single status with all applications', () => {
		const stats: DashboardStats = { total: 5, draft: 5, submitted: 0, approved: 0, rejected: 0 };
		const result = computePercentages(stats);

		expect(result.draft).toBe(100);
		expect(result.submitted).toBe(0);
		expect(result.approved).toBe(0);
		expect(result.rejected).toBe(0);
	});

	it('should always sum to 100 with rounding', () => {
		const stats: DashboardStats = { total: 7, draft: 2, submitted: 2, approved: 2, rejected: 1 };
		const result = computePercentages(stats);

		const sum = result.draft + result.submitted + result.approved + result.rejected;
		expect(sum).toBe(100);
	});

	it('should handle equal distribution', () => {
		const stats: DashboardStats = { total: 4, draft: 1, submitted: 1, approved: 1, rejected: 1 };
		const result = computePercentages(stats);

		expect(result.draft).toBe(25);
		expect(result.submitted).toBe(25);
		expect(result.approved).toBe(25);
		expect(result.rejected).toBe(25);
	});
});

describe('statusEntries', () => {
	it('should have exactly 4 entries', () => {
		expect(statusEntries).toHaveLength(4);
	});

	it('should contain all dashboard status keys', () => {
		const keys = statusEntries.map((e) => e.key);
		expect(keys).toContain('draft');
		expect(keys).toContain('submitted');
		expect(keys).toContain('approved');
		expect(keys).toContain('rejected');
	});

	it('should have German labels', () => {
		const labels = statusEntries.map((e) => e.label);
		expect(labels).toContain('Entwurf');
		expect(labels).toContain('Eingereicht');
		expect(labels).toContain('Genehmigt');
		expect(labels).toContain('Abgelehnt');
	});
});
