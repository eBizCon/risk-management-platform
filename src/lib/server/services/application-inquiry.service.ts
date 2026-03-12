import { error } from '@sveltejs/kit';
import type { Application, ApplicationInquiry } from '../db/schema';
import {
	answerApplicationInquiry,
	createApplicationInquiry,
	getApplicationInquiries,
	getLatestOpenInquiry
} from './repositories/application-inquiry.repository';
import {
	getApplicationById,
	updateApplicationStatus
} from './repositories/application.repository';

const ensureInquiryCreationAllowed = (application: Application) => {
	if (application.status !== 'submitted' && application.status !== 'resubmitted') {
		throw error(409, 'Für diesen Antrag kann aktuell keine Rückfrage erstellt werden');
	}
};

const ensureInquiryResponseAllowed = (application: Application) => {
	if (application.status !== 'needs_information') {
		throw error(409, 'Für diesen Antrag kann aktuell keine Rückfrage beantwortet werden');
	}
};

export async function getApplicationInquiryTimeline(
	applicationId: number
): Promise<ApplicationInquiry[]> {
	return getApplicationInquiries(applicationId);
}

export async function createInquiryForApplication(params: {
	applicationId: number;
	processorEmail: string;
	inquiryText: string;
}): Promise<ApplicationInquiry> {
	const application = await getApplicationById(params.applicationId);

	if (!application) {
		throw error(404, 'Antrag nicht gefunden');
	}

	ensureInquiryCreationAllowed(application);

	const existingOpenInquiry = await getLatestOpenInquiry(params.applicationId);
	if (existingOpenInquiry) {
		throw error(409, 'Es existiert bereits eine offene Rückfrage für diesen Antrag');
	}

	const inquiry = await createApplicationInquiry({
		applicationId: params.applicationId,
		inquiryText: params.inquiryText,
		processorEmail: params.processorEmail
	});

	await updateApplicationStatus(params.applicationId, 'needs_information');

	return inquiry;
}

export async function answerInquiryForApplication(params: {
	applicationId: number;
	applicantEmail: string;
	responseText: string;
}): Promise<ApplicationInquiry> {
	const application = await getApplicationById(params.applicationId);

	if (!application) {
		throw error(404, 'Antrag nicht gefunden');
	}

	if (application.createdBy !== params.applicantEmail) {
		throw error(403, 'Keine Berechtigung');
	}

	ensureInquiryResponseAllowed(application);

	const openInquiry = await getLatestOpenInquiry(params.applicationId);
	if (!openInquiry) {
		throw error(409, 'Es existiert keine offene Rückfrage für diesen Antrag');
	}

	const inquiry = await answerApplicationInquiry(openInquiry.id, params.responseText);

	if (!inquiry) {
		throw error(500, 'Die Rückfrageantwort konnte nicht gespeichert werden');
	}

	await updateApplicationStatus(params.applicationId, 'resubmitted');

	return inquiry;
}
