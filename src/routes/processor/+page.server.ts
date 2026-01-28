import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import {
	getProcessorApplicationsPaginated,
	getProcessorApplicationStats,
	PAGE_SIZE
} from '$lib/server/services/repositories/application.repository';
import type { ApplicationStatus } from '$lib/server/db/schema';

export const load: PageServerLoad = async ({ url, locals }) => {
	if (!locals.user) {
		throw error(401, 'Login erforderlich');
	}

	if (locals.user.role !== 'processor') {
		throw error(403, 'Keine Berechtigung');
	}

	const allowedStatuses: ApplicationStatus[] = ['submitted', 'approved', 'rejected', 'draft'];
	const statusParam = url.searchParams.get('status');
	const statusFilter = allowedStatuses.includes(statusParam as ApplicationStatus)
		? (statusParam as ApplicationStatus)
		: null;

	const rawPage = Number.parseInt(url.searchParams.get('page') ?? '', 10);
	const safePage = Number.isFinite(rawPage) && rawPage > 0 ? rawPage : 1;

	const initialResult = await getProcessorApplicationsPaginated({
		status: statusFilter ?? undefined,
		page: safePage,
		pageSize: PAGE_SIZE
	});

	const totalPages = Math.max(1, Math.ceil(initialResult.totalCount / PAGE_SIZE));
	const currentPage = Math.min(Math.max(safePage, 1), totalPages);

	const result = currentPage === safePage
		? initialResult
		: await getProcessorApplicationsPaginated({
			status: statusFilter ?? undefined,
			page: currentPage,
			pageSize: PAGE_SIZE
		});

	const stats = await getProcessorApplicationStats();

	return {
		applications: result.items,
		statusFilter,
		stats,
		pagination: {
			page: currentPage,
			pageSize: PAGE_SIZE,
			totalItems: result.totalCount,
			totalPages
		}
	};
};
