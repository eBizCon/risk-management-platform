import { and, asc, desc, eq } from 'drizzle-orm';
import { applicationInquiries, db } from '../../db';
import type { ApplicationInquiry, NewApplicationInquiry } from '../../db/schema';

export async function getApplicationInquiries(applicationId: number): Promise<ApplicationInquiry[]> {
	return db
		.select()
		.from(applicationInquiries)
		.where(eq(applicationInquiries.applicationId, applicationId))
		.orderBy(asc(applicationInquiries.createdAt), asc(applicationInquiries.id));
}

export async function getLatestOpenInquiry(
	applicationId: number
): Promise<ApplicationInquiry | null> {
	const [inquiry] = await db
		.select()
		.from(applicationInquiries)
		.where(
			and(
				eq(applicationInquiries.applicationId, applicationId),
				eq(applicationInquiries.status, 'open')
			)
		)
		.orderBy(desc(applicationInquiries.createdAt), desc(applicationInquiries.id))
		.limit(1);

	return inquiry ?? null;
}

export async function createApplicationInquiry(
	data: Omit<
		NewApplicationInquiry,
		'id' | 'createdAt' | 'respondedAt' | 'responseText' | 'status'
	>
): Promise<ApplicationInquiry> {
	const [inquiry] = await db
		.insert(applicationInquiries)
		.values({
			...data,
			status: 'open'
		})
		.returning();

	return inquiry;
}

export async function answerApplicationInquiry(
	inquiryId: number,
	responseText: string
): Promise<ApplicationInquiry | null> {
	const [inquiry] = await db
		.update(applicationInquiries)
		.set({
			responseText,
			respondedAt: new Date(),
			status: 'answered'
		})
		.where(eq(applicationInquiries.id, inquiryId))
		.returning();

	return inquiry ?? null;
}
