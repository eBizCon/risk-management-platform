import type { DashboardStats } from './types';

export interface StatusEntry {
	key: keyof Omit<DashboardStats, 'total'>;
	label: string;
	color: string;
	bgColor: string;
	borderColor: string;
}

export const statusEntries: StatusEntry[] = [
	{
		key: 'draft',
		label: 'Entwurf',
		color: '#3B82F6',
		bgColor: 'rgb(239 246 255)',
		borderColor: 'rgb(191 219 254)'
	},
	{
		key: 'submitted',
		label: 'Eingereicht',
		color: '#F59E0B',
		bgColor: 'rgb(255 251 235)',
		borderColor: 'rgb(253 230 138)'
	},
	{
		key: 'approved',
		label: 'Genehmigt',
		color: '#22C55E',
		bgColor: 'rgb(240 253 244)',
		borderColor: 'rgb(187 247 208)'
	},
	{
		key: 'rejected',
		label: 'Abgelehnt',
		color: '#EF4444',
		bgColor: 'rgb(254 242 242)',
		borderColor: 'rgb(254 202 202)'
	}
];

export function computePercentages(stats: DashboardStats): Record<string, number> {
	const result: Record<string, number> = {};
	if (stats.total === 0) {
		for (const entry of statusEntries) {
			result[entry.key] = 0;
		}
		return result;
	}

	let remaining = 100;
	const rawPercentages = statusEntries.map((entry) => ({
		key: entry.key,
		raw: (stats[entry.key] / stats.total) * 100
	}));

	const floored = rawPercentages.map((p) => ({
		...p,
		floored: Math.floor(p.raw),
		remainder: p.raw - Math.floor(p.raw)
	}));

	const flooredSum = floored.reduce((sum, p) => sum + p.floored, 0);
	remaining = 100 - flooredSum;

	const sorted = [...floored].sort((a, b) => b.remainder - a.remainder);
	for (let i = 0; i < remaining; i++) {
		sorted[i].floored += 1;
	}

	for (const p of sorted) {
		result[p.key] = p.floored;
	}

	return result;
}
