import { pgTable, text, serial, doublePrecision, integer, boolean, bigint } from 'drizzle-orm/pg-core';

export const applications = pgTable('applications', {
	id: serial('id').primaryKey(),
	name: text('name').notNull(),
	income: doublePrecision('income').notNull(),
	fixedCosts: doublePrecision('fixed_costs').notNull(),
	desiredRate: doublePrecision('desired_rate').notNull(),
	employmentStatus: text('employment_status', {
		enum: ['employed', 'self_employed', 'unemployed', 'retired']
	}).notNull(),
	hasPaymentDefault: boolean('has_payment_default').notNull(),
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

export const sessions = pgTable('sessions', {
	id: text('id').primaryKey(),
	userId: text('user_id').notNull(),
	userEmail: text('user_email').notNull(),
	userName: text('user_name').notNull(),
	userRole: text('user_role', { enum: ['applicant', 'processor'] }).notNull(),
	userIdToken: text('user_id_token'),
	expiresAt: bigint('expires_at', { mode: 'number' }).notNull()
});

export type Application = typeof applications.$inferSelect;
export type NewApplication = typeof applications.$inferInsert;
export type ApplicationStatus = 'draft' | 'submitted' | 'approved' | 'rejected';
export type EmploymentStatus = 'employed' | 'self_employed' | 'unemployed' | 'retired';
export type TrafficLight = 'red' | 'yellow' | 'green';

export type Session = typeof sessions.$inferSelect;
export type NewSession = typeof sessions.$inferInsert;
