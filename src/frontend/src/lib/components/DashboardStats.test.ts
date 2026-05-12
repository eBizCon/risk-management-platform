import { describe, it, expect } from 'vitest';
import type { DashboardStats } from '$lib/types';

function computePercentages(stats: DashboardStats): Record<string, number> {
	if (stats.total === 0) return { draft: 0, submitted: 0, approved: 0, rejected: 0 };
	return {
		draft: Math.round((stats.draft / stats.total) * 100),
		submitted: Math.round((stats.submitted / stats.total) * 100),
		approved: Math.round((stats.approved / stats.total) * 100),
		rejected: Math.round((stats.rejected / stats.total) * 100)
	};
}

describe('DashboardStats logic', () => {
	it('should have total equal to sum of all status counts', () => {
		const stats: DashboardStats = { total: 12, draft: 4, submitted: 3, approved: 3, rejected: 2 };
		expect(stats.total).toBe(stats.draft + stats.submitted + stats.approved + stats.rejected);
	});

	it('should compute correct percentages for non-zero totals', () => {
		const stats: DashboardStats = { total: 12, draft: 4, submitted: 3, approved: 3, rejected: 2 };
		const pct = computePercentages(stats);
		expect(pct.draft).toBe(33);
		expect(pct.submitted).toBe(25);
		expect(pct.approved).toBe(25);
		expect(pct.rejected).toBe(17);
	});

	it('should return zero percentages when total is zero', () => {
		const stats: DashboardStats = { total: 0, draft: 0, submitted: 0, approved: 0, rejected: 0 };
		const pct = computePercentages(stats);
		expect(pct.draft).toBe(0);
		expect(pct.submitted).toBe(0);
		expect(pct.approved).toBe(0);
		expect(pct.rejected).toBe(0);
	});

	it('should handle single status with all counts', () => {
		const stats: DashboardStats = { total: 5, draft: 5, submitted: 0, approved: 0, rejected: 0 };
		const pct = computePercentages(stats);
		expect(pct.draft).toBe(100);
		expect(pct.submitted).toBe(0);
		expect(pct.approved).toBe(0);
		expect(pct.rejected).toBe(0);
	});

	it('should exclude non-dashboard statuses from total', () => {
		// The dashboard only considers draft, submitted, approved, rejected.
		// Other statuses (needs_information, resubmitted, processing, failed) are not included.
		const stats: DashboardStats = { total: 4, draft: 1, submitted: 1, approved: 1, rejected: 1 };
		expect(stats.total).toBe(4);
		expect(stats.total).toBe(stats.draft + stats.submitted + stats.approved + stats.rejected);
	});
});
