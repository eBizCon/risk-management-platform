import type { PageLoad } from './$types';
import { handleApiResponse } from '$lib/api';

export interface ScoringConfig {
	id: number;
	version: number;
	greenThreshold: number;
	yellowThreshold: number;
	incomeRatioGood: number;
	incomeRatioModerate: number;
	incomeRatioLimited: number;
	penaltyModerateRatio: number;
	penaltyLimitedRatio: number;
	penaltyCriticalRatio: number;
	rateGood: number;
	rateModerate: number;
	rateHeavy: number;
	penaltyModerateRate: number;
	penaltyHeavyRate: number;
	penaltyExcessiveRate: number;
	penaltySelfEmployed: number;
	penaltyRetired: number;
	penaltyUnemployed: number;
	penaltyPaymentDefault: number;
	createdBy: string;
	createdAt: string;
}

export const load: PageLoad = async ({ fetch, url }) => {
	const res = await fetch('/api/scoring-config');

	if (res.status === 404) {
		return { config: null };
	}

	const config = await handleApiResponse<ScoringConfig>(res, url, 'Fehler beim Laden der Scoring-Konfiguration');
	return { config };
};
