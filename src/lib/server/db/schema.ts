import { sqliteTable, text, integer, real } from 'drizzle-orm/sqlite-core';

export const scoringConfig = sqliteTable('scoring_config', {
	id: integer('id').primaryKey(),
	trafficLightGreen: integer('traffic_light_green').notNull(),
	trafficLightYellow: integer('traffic_light_yellow').notNull(),
	incomeRatioThresholds: text('income_ratio_thresholds').notNull(),
	affordabilityThresholds: text('affordability_thresholds').notNull(),
	employmentDeductions: text('employment_deductions').notNull(),
	paymentDefaultDeduction: integer('payment_default_deduction').notNull(),
	createdAt: integer('created_at', { mode: 'timestamp' }).notNull(),
	updatedAt: integer('updated_at', { mode: 'timestamp' }).notNull()
});

export type ScoringConfig = typeof scoringConfig.$inferSelect;
export type NewScoringConfig = typeof scoringConfig.$inferInsert;

export const applications = sqliteTable('applications', {
	id: integer('id').primaryKey({ autoIncrement: true }),
	name: text('name').notNull(),
	income: real('income').notNull(),
	fixedCosts: real('fixed_costs').notNull(),
	desiredRate: real('desired_rate').notNull(),
	employmentStatus: text('employment_status', {
		enum: ['employed', 'self_employed', 'unemployed', 'retired']
	}).notNull(),
	hasPaymentDefault: integer('has_payment_default', { mode: 'boolean' }).notNull(),
	status: text('status', {
		enum: ['draft', 'submitted', 'approved', 'rejected']
	})
		.notNull()
		.default('draft'),
	score: integer('score'),
	trafficLight: text('traffic_light', { enum: ['red', 'yellow', 'green'] }),
	scoringReasons: text('scoring_reasons'),
	processorComment: text('processor_comment'),
	createdAt: text('created_at')
		.notNull()
		.$defaultFn(() => new Date().toISOString()),
	submittedAt: text('submitted_at'),
	processedAt: text('processed_at'),
	createdBy: text('created_by').notNull()
});

export type Application = typeof applications.$inferSelect;
export type NewApplication = typeof applications.$inferInsert;
export type ApplicationStatus = 'draft' | 'submitted' | 'approved' | 'rejected';
export type EmploymentStatus = 'employed' | 'self_employed' | 'unemployed' | 'retired';
export type TrafficLight = 'red' | 'yellow' | 'green';
