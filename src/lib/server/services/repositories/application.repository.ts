import { eq, and } from 'drizzle-orm';
import { db, applications } from '../../db';
import type { Application, NewApplication, ApplicationStatus } from '../../db/schema';
import { calculateScore } from '../scoring';

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

export async function deleteApplication(id: number): Promise<boolean> {
	const existing = db.select().from(applications).where(eq(applications.id, id)).get();
	if (!existing || existing.status !== 'draft') {
		return false;
	}

	db.delete(applications).where(eq(applications.id, id)).run();
	return true;
}
