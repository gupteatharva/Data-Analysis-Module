import { Injectable } from '@angular/core';
import { ChartData } from 'app/chart.model';

@Injectable({
  providedIn: 'root'
})
export class SaveChartService {
  savedCharts: ChartData[] = [];
  constructor() { }
  addChart(chart: ChartData) {
    this.savedCharts.push(chart);
  }

  getCharts(): ChartData[] {
    return this.savedCharts;
  }

  clearCharts()
  {
return this.savedCharts = [];
  }
}
