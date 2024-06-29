import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { SaveChartService } from 'app/services/save-chart.service';
import { Chart, registerables } from 'chart.js';
import { ChartData } from 'app/chart.model';

@Component({
  selector: 'app-graph-vis',
  templateUrl: './graph-vis.component.html',
  styleUrls: ['./graph-vis.component.css']
})
export class GraphVisComponent implements OnInit {
  myChart!: Chart;
  ctx: any;
  @Input() graphType!: any;
  @Input() graphValue!: any;
  @Input() test!: boolean;
  @Input() graphX: any[] = [];
  @Input() headers: any[] = [];
  @Input() graphY: any[] = [];
  // @Input() onSelectChange!: () => boolean;
  @Input() chartOptions!: any;
  lineChart!: any

  constructor(private chartService: SaveChartService) {
    Chart.register(...registerables);
  }
  ngOnChanges(changes: SimpleChanges): void {
    console.log(changes.headers.currentValue);
    if (changes.graphX && changes.graphY) {
      if (this.myChart) {
        // Update the chart data
        this.myChart.data.labels = this.graphX;
        this.myChart.data.datasets[0].data = this.graphY;
        this.myChart.data.datasets[0].label = this.headers[0] + " vs " + this.headers[1];
        // this.myChart.options.scales[x].title = this.headers[1];
        this.myChart.update();
        if (this.myChart.data.labels.length > 0 && this.myChart.data.datasets[0].data.length > 0) {
          // Update the chart in the chart service list
          const chartData: ChartData = {
            id: new Date().getTime(),
            type: this.graphType,
            data: this.myChart.data.datasets[0].data,
            labels: this.myChart.data.labels,
            headerLabel: this.myChart.data.datasets[0].label,
            backgroundColor: [
              'rgb(255, 99, 132)',
              'rgb(54, 162, 235)',
              'rgb(255, 205, 86)'
            ],
            borderWidth: 1,
            tension: 0.1
          };

          this.chartService.addChart(chartData);
          console.log(chartData);
        }
      } else {
        var data = []
        for (var i = 0; i < this.graphX.length; i++) {
          data.push({
            label: this.graphX[i],
            value: this.graphY[i]
          })
        }
        // Create the chart
        console.log(this.headers);
        console.log(this.graphX);
        const canvas = <HTMLCanvasElement>document.getElementById('myChart');
        this.ctx = canvas.getContext('2d');
        this.myChart = new Chart(this.ctx, {
          type: this.graphType,
          data: {
            labels: data.map(item => item.label),
            datasets: [{
              label: this.headers[0] + ' vs ' + this.headers[1],
              data: data.map(item => item.value),
              // backgroundColor: [
              //   '#029eb1',
              //   '#f5700c',
              //   '#d22824'
              // ],
              backgroundColor: [
                'rgb(255, 99, 132)',
                'rgb(54, 162, 235)',
                'rgb(255, 205, 86)',
                'rgb(255, 144, 99)',
                'rgb(210, 255, 99)',
                'rgb(155, 209, 245)',
                'rgb(255, 224, 149)',
              ],
              borderWidth: 1,
              tension: 0.1
            }]
          },
          options: {
            scales: {
              x: {
                ticks: {
                  color: 'white'
                }
              },
              y: {
                ticks: {
                  color: 'white'
                }
              }
              // xAxis: [{
              //   display: true,
              //   title: {
              //     display: true,
              //     text: this.headers[0]
              //   }
              // }],
              // yAxes: [{
              //   display: true,
              //   title: {
              //     display: true,
              //     text: this.headers[1]
              //   }
              // }]
            },
            plugins: {
              legend: {
                labels: {
                  color: 'white'
                }
              },
              elements: {
                line: {
                  borderColor: 'white' // Set the color of the lines to white
                }
              },
              tooltip: {
                callbacks: {
                  title: function (context) {
                    console.log(context);
                  }
                }
              }
            }
          }
        });
        if (this.myChart.data.labels.length > 0 && this.myChart.data.datasets[0].data.length > 0) {
          // Update the chart in the chart service list
          const chartData: ChartData = {
            id: new Date().getTime(),
            type: this.graphType,
            data: this.myChart.data.datasets[0].data,
            labels: this.myChart.data.labels,
            headerLabel: this.myChart.data.datasets[0].label,
            backgroundColor: [
              'rgb(255, 99, 132)',
              'rgb(54, 162, 235)',
              'rgb(255, 205, 86)',
              'rgb(255, 144, 99)',
              'rgb(210, 255, 99)',
              'rgb(155, 209, 245)',
              'rgb(255, 224, 149)',
            ],
            borderWidth: 1,
            tension: 0.1
          };

          this.chartService.addChart(chartData);
          console.log(chartData);
        }
      }
    }
  }

  ngOnInit(): void {
  }
  // ngAfterViewInit(): void {
  //   const canvas = <HTMLCanvasElement>document.getElementById('myChart');

  //   console.log('in func');
  //   console.log(this.graphX);
  //   console.log(this.graphY);
  //   console.log(this.graphType);
  //   this.ctx = canvas.getContext('2d');
  //   var data = []
  //   // for (var i = 0; i < this.graphX.length; i++) {
  //   //   data.push({
  //   //     x: this.graphX[i],
  //   //     y: this.graphY[i]
  //   //   })
  //   // }
  //   for(var i = 0; i< this.graphX.length; i++)
  //   {
  //     data.push({
  //       label: this.graphX[i],
  //       value: this.graphY[i]
  //     })
  //   }
  //   // console.log(data)
  //   // this.myChart = new Chart(this.ctx, {
  //   //   type: 'scatter',
  //   //   data: {
  //   //     datasets: [{
  //   //       label: 'test',
  //   //       data: data,
  //   //       pointBackgroundColor: 'rgba(255, 99, 132, 0.2)',
  //   //       pointBorderColor: 'rgba(255, 99, 132, 1)',
  //   //       pointBorderWidth: 1,
  //   //       pointRadius: 5
  //   //     }]
  //   //   },
  //   //   options: {
  //   //     responsive: true,
  //   //     scales: {
  //   //       x: {
  //   //         type: 'linear',
  //   //         position: 'bottom',
  //   //           beginAtZero: true
  //   //       },
  //   //       y: {
  //   //         type: 'linear',
  //   //           beginAtZero: true
  //   //       }
  //   //     }
  //   //   }
  //   // });
  //   this.myChart = new Chart(this.ctx, {
  //     type: this.graphType,
  //     data: {
  //       labels: data.map(item => item.label),
  //       datasets: [{
  //         label: '',
  //         data: data.map(item => item.value),
  //         backgroundColor: [
  //           'rgb(255, 99, 132)',
  //           'rgb(54, 162, 235)',
  //           'rgb(255, 205, 86)'
  //         ],
  //         borderWidth: 1,
  //         tension: 0.1
  //       }]
  //     },
  //     options: {
  //       scales: {
  //         y: {
  //           beginAtZero: true
  //         }
  //       }
  //     }
  //   });
  //   // this.chartService.setChart(this.myChart);
  // }

}
