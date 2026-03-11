import {
	boolean,
	doublePrecision,
	integer,
	pgTable,
	serial,
	text,
	timestamp
} from 'drizzle-orm/pg-core';

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
		enum: ['draft', 'submitted', 'needs_information', 'resubmitted', 'approved', 'rejected']
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

export const applicationInquiries = pgTable('application_inquiries', {
	id: serial('id').primaryKey(),
	applicationId: integer('application_id')
		.notNull()
		.references(() => applications.id, { onDelete: 'cascade' }),
	inquiryText: text('inquiry_text').notNull(),
	status: text('status', {
		enum: ['open', 'answered']
	})
		.notNull()
		.default('open'),
	processorEmail: text('processor_email').notNull(),
	responseText: text('response_text'),
	createdAt: timestamp('created_at', { withTimezone: true }).notNull().defaultNow(),
	respondedAt: timestamp('responded_at', { withTimezone: true })
});

export type Application = typeof applications.$inferSelect;
export type ApplicationInquiry = typeof applicationInquiries.$inferSelect;
export type NewApplicationInquiry = typeof applicationInquiries.$inferInsert;
export type NewApplication = typeof applications.$inferInsert;
export type ApplicationStatus =
	| 'draft'
	| 'submitted'
	| 'needs_information'
	| 'resubmitted'
	| 'approved'
	| 'rejected';
export type ApplicationInquiryStatus = 'open' | 'answered';
export type EmploymentStatus = 'employed' | 'self_employed' | 'unemployed' | 'retired';
export type TrafficLight = 'red' | 'yellow' | 'green';
