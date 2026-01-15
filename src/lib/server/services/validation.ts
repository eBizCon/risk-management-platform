import { z } from 'zod';

export const applicationSchema = z.object({
	name: z
		.string()
		.min(2, 'Name muss mindestens 2 Zeichen lang sein')
		.max(100, 'Name darf maximal 100 Zeichen lang sein'),
	income: z
		.number()
		.positive('Einkommen muss positiv sein')
		.max(10000000, 'Einkommen scheint unrealistisch hoch'),
	fixedCosts: z
		.number()
		.min(0, 'Fixkosten können nicht negativ sein')
		.max(10000000, 'Fixkosten scheinen unrealistisch hoch'),
	desiredRate: z
		.number()
		.positive('Gewünschte Rate muss positiv sein')
		.max(1000000, 'Gewünschte Rate scheint unrealistisch hoch'),
	employmentStatus: z.enum(['employed', 'self_employed', 'unemployed', 'retired'], {
		message: 'Bitte wählen Sie einen gültigen Beschäftigungsstatus'
	}),
	hasPaymentDefault: z.boolean({
		message: 'Bitte geben Sie an, ob frühere Zahlungsverzüge vorliegen'
	})
});

export const applicationWithBusinessRulesSchema = applicationSchema.refine(
	(data) => data.fixedCosts < data.income,
	{
		message: 'Fixkosten müssen geringer als das Einkommen sein',
		path: ['fixedCosts']
	}
).refine(
	(data) => data.desiredRate <= (data.income - data.fixedCosts),
	{
		message: 'Gewünschte Rate kann nicht höher sein als das verfügbare Einkommen (Einkommen minus Fixkosten)',
		path: ['desiredRate']
	}
);

export const processorDecisionSchema = z.object({
	decision: z.enum(['approved', 'rejected'], {
		message: 'Bitte wählen Sie eine Entscheidung'
	}),
	comment: z.string().optional()
}).refine(
	(data) => data.decision !== 'rejected' || (data.comment && data.comment.trim().length > 0),
	{
		message: 'Bei Ablehnung ist eine Begründung erforderlich',
		path: ['comment']
	}
);

export type ApplicationInput = z.infer<typeof applicationSchema>;
export type ProcessorDecision = z.infer<typeof processorDecisionSchema>;
