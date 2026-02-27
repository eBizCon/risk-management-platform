import { sqliteTable, text, integer, real } from 'drizzle-orm/sqlite-core';

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

export const scoringConfig = sqliteTable('scoring_config', {
	id: integer('id').primaryKey({ autoIncrement: true }),
	thresholdGreen: integer('threshold_green').notNull().default(75),
	thresholdYellow: integer('threshold_yellow').notNull().default(50),
	weightIncome: real('weight_income').notNull().default(0.4),
	weightEmployment: real('weight_employment').notNull().default(0.3),
	weightPaymentDefault: real('weight_payment_default').notNull().default(0.3),
	updatedAt: text('updated_at').notNull(),
	updatedBy: text('updated_by').notNull(),
});

export type Application = typeof applications.$inferSelect;
export type NewApplication = typeof applications.$inferInsert;
export type ScoringConfig = typeof scoringConfig.$inferSelect;
export type NewScoringConfig = typeof scoringConfig.$inferInsert;
export type ApplicationStatus = 'draft' | 'submitted' | 'approved' | 'rejected';
export type EmploymentStatus = 'employed' | 'self_employed' | 'unemployed' | 'retired';
export type TrafficLight = 'red' | 'yellow' | 'green';
