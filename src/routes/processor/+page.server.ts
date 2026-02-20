import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { z } from 'zod';
import {
	getProcessorApplicationsPaginated,
	getProcessorApplicationStats,
	PAGE_SIZE
} from '$lib/server/services/repositories/application.repository';
import type { ApplicationStatus, TrafficLight } from '$lib/server/db/schema';

const trafficLightSchema = z.enum(['green', 'yellow', 'red']);

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

	const rawTrafficLightParams = url.searchParams.getAll('trafficLight');
	const trafficLightFilter: TrafficLight[] = rawTrafficLightParams.filter(
		(v) => trafficLightSchema.safeParse(v).success
	) as TrafficLight[];

	const rawPage = Number.parseInt(url.searchParams.get('page') ?? '', 10);
	const safePage = Number.isFinite(rawPage) && rawPage > 0 ? rawPage : 1;

	const initialResult = await getProcessorApplicationsPaginated({
		status: statusFilter ?? undefined,
		trafficLight: trafficLightFilter.length > 0 ? trafficLightFilter : undefined,
		page: safePage,
		pageSize: PAGE_SIZE
	});

	const totalPages = Math.max(1, Math.ceil(initialResult.totalCount / PAGE_SIZE));
	const currentPage = Math.min(Math.max(safePage, 1), totalPages);

	const result =
		currentPage === safePage
			? initialResult
			: await getProcessorApplicationsPaginated({
					status: statusFilter ?? undefined,
					trafficLight: trafficLightFilter.length > 0 ? trafficLightFilter : undefined,
					page: currentPage,
					pageSize: PAGE_SIZE
				});

	const stats = await getProcessorApplicationStats();

	return {
		applications: result.items,
		statusFilter,
		trafficLightFilter,
		stats,
		pagination: {
			page: currentPage,
			pageSize: PAGE_SIZE,
			totalItems: result.totalCount,
			totalPages
		}
	};
};
