import { describe, it, expect } from 'vitest';
import { 
	applicationSchema, 
	applicationWithBusinessRulesSchema, 
	processorDecisionSchema 
} from './validation';

describe('Validation Service', () => {
	describe('applicationSchema', () => {
		it('should validate a valid application', () => {
			const validApplication = {
				name: 'Max Mustermann',
				income: 4000,
				fixedCosts: 1500,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(validApplication);
			expect(result.success).toBe(true);
		});

		it('should reject empty name', () => {
			const invalidApplication = {
				name: '',
				income: 4000,
				fixedCosts: 1500,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
		});

		it('should reject name shorter than 2 characters', () => {
			const invalidApplication = {
				name: 'A',
				income: 4000,
				fixedCosts: 1500,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
			if (!result.success) {
				expect(result.error.issues[0].message).toContain('mindestens');
			}
		});

		it('should reject negative income', () => {
			const invalidApplication = {
				name: 'Max Mustermann',
				income: -100,
				fixedCosts: 1500,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
			if (!result.success) {
				expect(result.error.issues[0].message).toContain('positiv');
			}
		});

		it('should reject zero income', () => {
			const invalidApplication = {
				name: 'Max Mustermann',
				income: 0,
				fixedCosts: 1500,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
		});

		it('should reject negative fixed costs', () => {
			const invalidApplication = {
				name: 'Max Mustermann',
				income: 4000,
				fixedCosts: -500,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
			if (!result.success) {
				expect(result.error.issues[0].message).toContain('nicht negativ');
			}
		});

		it('should allow zero fixed costs', () => {
			const validApplication = {
				name: 'Max Mustermann',
				income: 4000,
				fixedCosts: 0,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(validApplication);
			expect(result.success).toBe(true);
		});

		it('should reject negative desired rate', () => {
			const invalidApplication = {
				name: 'Max Mustermann',
				income: 4000,
				fixedCosts: 1500,
				desiredRate: -100,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
			if (!result.success) {
				expect(result.error.issues[0].message).toContain('positiv');
			}
		});

		it('should reject invalid employment status', () => {
			const invalidApplication = {
				name: 'Max Mustermann',
				income: 4000,
				fixedCosts: 1500,
				desiredRate: 500,
				employmentStatus: 'invalid_status',
				hasPaymentDefault: false
			};
			
			const result = applicationSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
		});

		it('should accept all valid employment statuses', () => {
			const statuses = ['employed', 'self_employed', 'unemployed', 'retired'];
			
			statuses.forEach(status => {
				const application = {
					name: 'Max Mustermann',
					income: 4000,
					fixedCosts: 1500,
					desiredRate: 500,
					employmentStatus: status,
					hasPaymentDefault: false
				};
				
				const result = applicationSchema.safeParse(application);
				expect(result.success).toBe(true);
			});
		});
	});

	describe('applicationWithBusinessRulesSchema', () => {
		it('should reject when desired rate exceeds available income', () => {
			const invalidApplication = {
				name: 'Max Mustermann',
				income: 3000,
				fixedCosts: 2000,
				desiredRate: 1500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationWithBusinessRulesSchema.safeParse(invalidApplication);
			expect(result.success).toBe(false);
			if (!result.success) {
				expect(result.error.issues[0].message).toContain('verfügbare');
			}
		});

		it('should accept when desired rate is within available income', () => {
			const validApplication = {
				name: 'Max Mustermann',
				income: 4000,
				fixedCosts: 1500,
				desiredRate: 500,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationWithBusinessRulesSchema.safeParse(validApplication);
			expect(result.success).toBe(true);
		});

		it('should accept when desired rate equals available income', () => {
			const validApplication = {
				name: 'Max Mustermann',
				income: 3000,
				fixedCosts: 2000,
				desiredRate: 1000,
				employmentStatus: 'employed',
				hasPaymentDefault: false
			};
			
			const result = applicationWithBusinessRulesSchema.safeParse(validApplication);
			expect(result.success).toBe(true);
		});
	});

	describe('processorDecisionSchema', () => {
		it('should accept approved decision without comment', () => {
			const validDecision = {
				decision: 'approved',
				comment: undefined
			};
			
			const result = processorDecisionSchema.safeParse(validDecision);
			expect(result.success).toBe(true);
		});

		it('should accept approved decision with comment', () => {
			const validDecision = {
				decision: 'approved',
				comment: 'Genehmigt aufgrund guter Bonität'
			};
			
			const result = processorDecisionSchema.safeParse(validDecision);
			expect(result.success).toBe(true);
		});

		it('should reject rejected decision without comment', () => {
			const invalidDecision = {
				decision: 'rejected',
				comment: undefined
			};
			
			const result = processorDecisionSchema.safeParse(invalidDecision);
			expect(result.success).toBe(false);
			if (!result.success) {
				expect(result.error.issues[0].message).toContain('Begründung');
			}
		});

		it('should reject rejected decision with empty comment', () => {
			const invalidDecision = {
				decision: 'rejected',
				comment: ''
			};
			
			const result = processorDecisionSchema.safeParse(invalidDecision);
			expect(result.success).toBe(false);
		});

		it('should accept rejected decision with comment', () => {
			const validDecision = {
				decision: 'rejected',
				comment: 'Abgelehnt wegen unzureichender Bonität'
			};
			
			const result = processorDecisionSchema.safeParse(validDecision);
			expect(result.success).toBe(true);
		});

		it('should reject invalid decision value', () => {
			const invalidDecision = {
				decision: 'pending',
				comment: 'Some comment'
			};
			
			const result = processorDecisionSchema.safeParse(invalidDecision);
			expect(result.success).toBe(false);
		});
	});
});
