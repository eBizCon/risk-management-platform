import {
	Chart as ChartJS,
	BarController,
	PieController,
	CategoryScale,
	LinearScale,
	BarElement,
	ArcElement,
	Title,
	Tooltip,
	Legend
} from 'chart.js';

ChartJS.register(
	BarController,
	PieController,
	CategoryScale,
	LinearScale,
	BarElement,
	ArcElement,
	Title,
	Tooltip,
	Legend
);

export { ChartJS };
