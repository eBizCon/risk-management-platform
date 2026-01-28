import { eq, and, desc, count } from 'drizzle-orm';
import { db, applications } from '../../db';
import type { Application, NewApplication, ApplicationStatus } from '../../db/schema';
import { calculateScore } from '../scoring';

export const PAGE_SIZE = 10;

export async function createApplication(data: Omit<NewApplication, 'id' | 'createdAt' | 'score' | 'trafficLight' | 'scoringReasons'>): Promise<Application> {
	const scoring = calculateScore(
		data.income,
		data.fixedCosts,
		data.desiredRate,
		data.employmentStatus,
		data.hasPaymentDefault
	);

	const result = db.insert(applications).values({
		...data,
		score: scoring.score,
		trafficLight: scoring.trafficLight,
		scoringReasons: JSON.stringify(scoring.reasons),
		createdAt: new Date().toISOString()
	}).returning().get();

	return result;
}

export async function updateApplication(
	id: number,
	data: Partial<Omit<NewApplication, 'id' | 'createdAt' | 'createdBy'>>
): Promise<Application | null> {
	const existing = db.select().from(applications).where(eq(applications.id, id)).get();
	if (!existing || existing.status !== 'draft') {
		return null;
	}

	const updatedData: Record<string, unknown> = { ...data };

	if (data.income !== undefined || data.fixedCosts !== undefined || data.desiredRate !== undefined || 
		data.employmentStatus !== undefined || data.hasPaymentDefault !== undefined) {
		const scoring = calculateScore(
			data.income ?? existing.income,
			data.fixedCosts ?? existing.fixedCosts,
			data.desiredRate ?? existing.desiredRate,
			data.employmentStatus ?? existing.employmentStatus,
			data.hasPaymentDefault ?? existing.hasPaymentDefault
		);
		updatedData.score = scoring.score;
		updatedData.trafficLight = scoring.trafficLight;
		updatedData.scoringReasons = JSON.stringify(scoring.reasons);
	}

	const result = db.update(applications)
		.set(updatedData)
		.where(eq(applications.id, id))
		.returning()
		.get();

	return result ?? null;
}

export async function submitApplication(id: number): Promise<Application | null> {
	const existing = db.select().from(applications).where(eq(applications.id, id)).get();
	if (!existing || existing.status !== 'draft') {
		return null;
	}

	const scoring = calculateScore(
		existing.income,
		existing.fixedCosts,
		existing.desiredRate,
		existing.employmentStatus,
		existing.hasPaymentDefault
	);

	const result = db.update(applications)
		.set({
			status: 'submitted',
			submittedAt: new Date().toISOString(),
			score: scoring.score,
			trafficLight: scoring.trafficLight,
			scoringReasons: JSON.stringify(scoring.reasons)
		})
		.where(eq(applications.id, id))
		.returning()
		.get();

	return result ?? null;
}

export async function processApplication(
	id: number,
	decision: 'approved' | 'rejected',
	comment?: string
): Promise<Application | null> {
	const existing = db.select().from(applications).where(eq(applications.id, id)).get();
	if (!existing || existing.status !== 'submitted') {
		return null;
	}

	const result = db.update(applications)
		.set({
			status: decision,
			processorComment: comment || null,
			processedAt: new Date().toISOString()
		})
		.where(eq(applications.id, id))
		.returning()
		.get();

	return result ?? null;
}

export async function getApplicationById(id: number): Promise<Application | null> {
	const result = db.select().from(applications).where(eq(applications.id, id)).get();
	return result ?? null;
}

export async function getApplicationsByUser(userId: string, status?: ApplicationStatus): Promise<Application[]> {
	if (status) {
		return db.select().from(applications)
			.where(and(eq(applications.createdBy, userId), eq(applications.status, status)))
			.all();
	}
	return db.select().from(applications).where(eq(applications.createdBy, userId)).all();
}

export async function getApplicationsByStatus(status: ApplicationStatus): Promise<Application[]> {
	return db.select().from(applications).where(eq(applications.status, status)).all();
}

export async function getAllApplications(): Promise<Application[]> {
	return db.select().from(applications).all();
}

export async function getProcessorApplicationsPaginated(params: {
	status?: ApplicationStatus;
	page: number;
	pageSize: number;
}): Promise<{ items: Application[]; totalCount: number }> {
	const whereClause = params.status ? eq(applications.status, params.status) : undefined;
	const totalQuery = db.select({ value: count() }).from(applications);
	const totalResult = whereClause ? totalQuery.where(whereClause).get() : totalQuery.get();
	const totalCount = totalResult?.value ?? 0;
	const itemsQuery = db
		.select()
		.from(applications)
		.orderBy(desc(applications.createdAt))
		.limit(params.pageSize)
		.offset((params.page - 1) * params.pageSize);
	const items = whereClause ? itemsQuery.where(whereClause).all() : itemsQuery.all();

	return { items, totalCount };
}

export async function getProcessorApplicationStats(): Promise<{
	total: number;
	submitted: number;
	approved: number;
	rejected: number;
}> {
	const total = db.select({ value: count() }).from(applications).get()?.value ?? 0;
	const submitted = db
		.select({ value: count() })
		.from(applications)
		.where(eq(applications.status, 'submitted'))
		.get()?.value ?? 0;
	const approved = db
		.select({ value: count() })
		.from(applications)
		.where(eq(applications.status, 'approved'))
		.get()?.value ?? 0;
	const rejected = db
		.select({ value: count() })
		.from(applications)
		.where(eq(applications.status, 'rejected'))
		.get()?.value ?? 0;

	return {
		total,
		submitted,
		approved,
		rejected
	};
}

export async function deleteApplication(id: number): Promise<boolean> {
	const existing = db.select().from(applications).where(eq(applications.id, id)).get();
	if (!existing || existing.status !== 'draft') {
		return false;
	}

	db.delete(applications).where(eq(applications.id, id)).run();
	return true;
}
