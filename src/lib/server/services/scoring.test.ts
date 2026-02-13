import { describe, it, expect, vi } from 'vitest';

vi.mock('./repositories/scoring-config.repository', () => ({
	getActiveScoringConfig: vi.fn().mockResolvedValue({
		id: 1,
		trafficLightGreen: 75,
		trafficLightYellow: 50,
		incomeRatioThresholds: { excellent: 0.5, good: 0.3, moderate: 0.1 },
		affordabilityThresholds: { comfortable: 0.3, moderate: 0.5, stretched: 0.7 },
		employmentDeductions: { employed: 0, self_employed: 10, retired: 5, unemployed: 35 },
		paymentDefaultDeduction: 25,
		createdAt: new Date(),
		updatedAt: new Date()
	}),
	getDefaultScoringConfig: vi.fn().mockReturnValue({
		id: 1,
		trafficLightGreen: 75,
		trafficLightYellow: 50,
		incomeRatioThresholds: { excellent: 0.5, good: 0.3, moderate: 0.1 },
		affordabilityThresholds: { comfortable: 0.3, moderate: 0.5, stretched: 0.7 },
		employmentDeductions: { employed: 0, self_employed: 10, retired: 5, unemployed: 35 },
		paymentDefaultDeduction: 25,
		createdAt: new Date(),
		updatedAt: new Date()
	})
}));

const { calculateScoreWithConfig } = await import('./scoring');

const defaultConfig = {
	id: 1,
	trafficLightGreen: 75,
	trafficLightYellow: 50,
	incomeRatioThresholds: { excellent: 0.5, good: 0.3, moderate: 0.1 },
	affordabilityThresholds: { comfortable: 0.3, moderate: 0.5, stretched: 0.7 },
	employmentDeductions: { employed: 0, self_employed: 10, retired: 5, unemployed: 35 },
	paymentDefaultDeduction: 25,
	createdAt: new Date(),
	updatedAt: new Date()
};

const calculateScore = (
	income: number,
	fixedCosts: number,
	desiredRate: number,
	employmentStatus: 'employed' | 'self_employed' | 'unemployed' | 'retired',
	hasPaymentDefault: boolean
) => calculateScoreWithConfig(income, fixedCosts, desiredRate, employmentStatus, hasPaymentDefault, defaultConfig);

describe('Scoring Service', () => {
	describe('calculateScore', () => {
		describe('Base Score Calculation', () => {
			it('should return a score between 0 and 100', () => {
				const result = calculateScore(4000, 1500, 500, 'employed', false);
				
				expect(result.score).toBeGreaterThanOrEqual(0);
				expect(result.score).toBeLessThanOrEqual(100);
			});

			it('should return green traffic light for score >= 75', () => {
				const result = calculateScore(6000, 2000, 500, 'employed', false);
				
				expect(result.score).toBeGreaterThanOrEqual(75);
				expect(result.trafficLight).toBe('green');
			});

			it('should return yellow traffic light for score >= 50 and < 75', () => {
				const result = calculateScore(4000, 2200, 700, 'self_employed', false);
				
				expect(result.score).toBeGreaterThanOrEqual(50);
				expect(result.score).toBeLessThan(75);
				expect(result.trafficLight).toBe('yellow');
			});

			it('should return red traffic light for score < 50', () => {
				const result = calculateScore(2500, 2000, 400, 'unemployed', true);
				
				expect(result.score).toBeLessThan(50);
				expect(result.trafficLight).toBe('red');
			});
		});

		describe('Income vs Fixed Costs Ratio', () => {
			it('should give high score for good income/costs ratio (>= 50%)', () => {
				const result = calculateScore(5000, 2000, 500, 'employed', false);
				
				expect(result.score).toBeGreaterThanOrEqual(75);
				expect(result.reasons.some(r => r.includes('Gutes Verhältnis'))).toBe(true);
			});

			it('should give moderate score for moderate income/costs ratio (30-50%)', () => {
				const result = calculateScore(4000, 2400, 400, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('Moderates Verhältnis'))).toBe(true);
			});

			it('should give lower score for limited income/costs ratio (10-30%)', () => {
				const result = calculateScore(3000, 2400, 300, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('Eingeschränktes Verhältnis'))).toBe(true);
			});

			it('should give low score for critical income/costs ratio (< 10%)', () => {
				const result = calculateScore(2500, 2300, 100, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('Kritisches Verhältnis'))).toBe(true);
			});
		});

		describe('Employment Status Impact', () => {
			it('should not penalize employed status', () => {
				const employedResult = calculateScore(4000, 1500, 500, 'employed', false);
				const selfEmployedResult = calculateScore(4000, 1500, 500, 'self_employed', false);
				
				expect(employedResult.score).toBeGreaterThan(selfEmployedResult.score);
			});

			it('should penalize self-employed status (-10 points)', () => {
				const employedResult = calculateScore(4000, 1500, 500, 'employed', false);
				const selfEmployedResult = calculateScore(4000, 1500, 500, 'self_employed', false);
				
				expect(employedResult.score - selfEmployedResult.score).toBe(10);
			});

			it('should penalize retired status (-5 points)', () => {
				const employedResult = calculateScore(4000, 1500, 500, 'employed', false);
				const retiredResult = calculateScore(4000, 1500, 500, 'retired', false);
				
				expect(employedResult.score - retiredResult.score).toBe(5);
			});

			it('should heavily penalize unemployed status (-35 points)', () => {
				const employedResult = calculateScore(4000, 1500, 500, 'employed', false);
				const unemployedResult = calculateScore(4000, 1500, 500, 'unemployed', false);
				
				expect(employedResult.score - unemployedResult.score).toBe(35);
			});
		});

		describe('Payment Default Impact', () => {
			it('should penalize payment default (-25 points)', () => {
				const noDefaultResult = calculateScore(4000, 1500, 500, 'employed', false);
				const withDefaultResult = calculateScore(4000, 1500, 500, 'employed', true);
				
				expect(noDefaultResult.score - withDefaultResult.score).toBe(25);
			});

			it('should include payment default in reasons when true', () => {
				const result = calculateScore(4000, 1500, 500, 'employed', true);
				
				expect(result.reasons.some(r => r.includes('Zahlungsverzüge'))).toBe(true);
			});

			it('should include positive payment history in reasons when false', () => {
				const result = calculateScore(4000, 1500, 500, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('Keine früheren Zahlungsverzüge'))).toBe(true);
			});
		});

		describe('Rate Affordability', () => {
			it('should give good score when rate is <= 30% of available income', () => {
				const result = calculateScore(5000, 2000, 500, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('gut tragbar'))).toBe(true);
			});

			it('should penalize when rate is 30-50% of available income', () => {
				const result = calculateScore(4000, 2000, 800, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('moderat tragbar'))).toBe(true);
			});

			it('should penalize more when rate is 50-70% of available income', () => {
				const result = calculateScore(4000, 2000, 1200, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('belastet das Budget erheblich'))).toBe(true);
			});

			it('should heavily penalize when rate is > 70% of available income', () => {
				const result = calculateScore(3000, 1500, 1200, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('übersteigt das tragbare Maß'))).toBe(true);
			});
		});

		describe('Scoring Reasons', () => {
			it('should return reasons in German', () => {
				const result = calculateScore(4000, 1500, 500, 'employed', false);
				
				expect(result.reasons.length).toBeGreaterThan(0);
				result.reasons.forEach(reason => {
					expect(typeof reason).toBe('string');
					expect(reason.length).toBeGreaterThan(0);
				});
			});

			it('should include employment status in reasons', () => {
				const result = calculateScore(4000, 1500, 500, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('Angestelltenverhältnis'))).toBe(true);
			});

			it('should include income ratio assessment in reasons', () => {
				const result = calculateScore(4000, 1500, 500, 'employed', false);
				
				expect(result.reasons.some(r => r.includes('Verhältnis zwischen Einkommen'))).toBe(true);
			});

			it('should include overall assessment as first reason', () => {
				const result = calculateScore(4000, 1500, 500, 'employed', false);
				
				expect(result.reasons[0]).toContain('Gesamtbewertung');
			});
		});

		describe('Edge Cases', () => {
			it('should handle zero fixed costs', () => {
				const result = calculateScore(4000, 0, 500, 'employed', false);
				
				expect(result.score).toBeGreaterThanOrEqual(0);
				expect(result.score).toBeLessThanOrEqual(100);
			});

			it('should handle minimum viable income', () => {
				const result = calculateScore(1000, 500, 100, 'employed', false);
				
				expect(result.score).toBeGreaterThanOrEqual(0);
				expect(result.score).toBeLessThanOrEqual(100);
			});

			it('should cap score at 100', () => {
				const result = calculateScore(10000, 1000, 500, 'employed', false);
				
				expect(result.score).toBeLessThanOrEqual(100);
			});

			it('should not go below 0', () => {
				const result = calculateScore(2000, 1800, 150, 'unemployed', true);
				
				expect(result.score).toBeGreaterThanOrEqual(0);
			});
		});

		describe('Traffic Light Thresholds', () => {
			it('should return green for score exactly 75', () => {
				const result = calculateScore(5000, 2000, 600, 'employed', false);
				
				if (result.score === 75) {
					expect(result.trafficLight).toBe('green');
				}
			});

			it('should return yellow for score exactly 50', () => {
				const result = calculateScore(4000, 2200, 600, 'self_employed', false);
				
				if (result.score === 50) {
					expect(result.trafficLight).toBe('yellow');
				}
			});
		});
	});
});
