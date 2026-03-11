import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import {
	getApplicationsByUserPaginated,
	APPLICANT_PAGE_SIZE
} from '$lib/server/services/repositories/application.repository';
import type { ApplicationStatus } from '$lib/server/db/schema';

export const load: PageServerLoad = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	const allowedStatuses: ApplicationStatus[] = ['draft', 'submitted', 'approved', 'rejected'];
	const statusParam = url.searchParams.get('status');
	const statusFilter = allowedStatuses.includes(statusParam as ApplicationStatus)
		? (statusParam as ApplicationStatus)
		: null;

	const rawPage = Number.parseInt(url.searchParams.get('page') ?? '', 10);
	const safePage = Number.isFinite(rawPage) && rawPage > 0 ? rawPage : 1;

	const initialResult = await getApplicationsByUserPaginated({
		userEmail: locals.user.email,
		status: statusFilter ?? undefined,
		page: safePage,
		pageSize: APPLICANT_PAGE_SIZE
	});

	const totalPages = Math.max(1, Math.ceil(initialResult.totalCount / APPLICANT_PAGE_SIZE));
	const currentPage = Math.min(Math.max(safePage, 1), totalPages);

	const result =
		currentPage === safePage
			? initialResult
			: await getApplicationsByUserPaginated({
					userEmail: locals.user.email,
					status: statusFilter ?? undefined,
					page: currentPage,
					pageSize: APPLICANT_PAGE_SIZE
				});

	return {
		applications: result.items,
		statusFilter,
		pagination: {
			page: currentPage,
			pageSize: APPLICANT_PAGE_SIZE,
			totalItems: result.totalCount,
			totalPages
		}
	};
};
