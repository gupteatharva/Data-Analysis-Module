import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { MatSnackBar, MatSnackBarHorizontalPosition, MatSnackBarVerticalPosition } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { FileDetails } from 'app/file.model';
import { FileService } from 'app/services/file.service';
import { ScriptService } from 'app/services/script.service';
import { SendFileService } from 'app/services/send-file.service';
import { ChartData } from 'app/chart.model';
import { SaveChartService } from 'app/services/save-chart.service';
import { Chart } from 'chart.js';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  @ViewChild('chartContainer') chartContainer!: ElementRef;
  @ViewChild('allChartContainer', { static: true }) allChartContainer!: ElementRef;

  horizontalPosition: MatSnackBarHorizontalPosition = 'end';
  verticalPosition: MatSnackBarVerticalPosition = 'top';
  selectedFile: any;
  actionObjects: FileDetails[] = [];
  fileObjects: FileDetails[] = [];
  visObjects: FileDetails[] = [];
  fileDetialsObj = new FileDetails(0, [], new MatTableDataSource<any>, [], 0, "", "", "", "", [], "", "");
  savedCharts: ChartData[];
  selectedChart: ChartData | null = null;
  chartInstance: Chart | null = null;
  allChartInstance: Chart | null = null;
  setCharts: boolean = false;
  checked: boolean = false;
  constructor(private scriptService: ScriptService, private fileService: FileService, private sendFileservice: SendFileService, private _snackBar: MatSnackBar, private chartService: SaveChartService) { }

  ngOnInit() {
    this.actionObjects = this.fileService.getActionObjects();
    console.log(this.actionObjects);
    this.savedCharts = this.chartService.getCharts();
    console.log(this.savedCharts);
    this.fileObjects = this.fileService.getFileObjects();
  }
  // ngAfterViewInit() {
  //   this.onChartSelect();
  // }

  ngOnDestroy() {
    // if (this.chartInstance) {
    //   this.destroyChart();
    // }
    // this.destroyAllChart();
    console.log('destoryed');
    this.checked = false;
    this.setCharts = false;
  }

  generateScript() {
    this.scriptService.genScript().subscribe(res => {
      const blob = new Blob([res], { type: 'application/octet-stream' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'script.py';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    })
  }

  source1(event: any) {
    this.selectedFile = event.target.files[0]
  }

  async onUpload(fileInput: HTMLInputElement) {
    const formData = new FormData();
    formData.append('file', this.selectedFile);
    try {
      const res = await this.sendFileservice.sendpyFile(formData).toPromise();
      console.log(res);
      this.actionObjects = this.fileService.clearActionObjects();
      this.fileObjects = this.fileService.clearFileObjects();
      this.visObjects = this.fileService.clearVisObjects();
      this.savedCharts = this.chartService.clearCharts();
      res.dataSources.forEach(ds => {
        console.log(ds);
        this.fileDetialsObj.finalData = ds.rowData,
          this.fileDetialsObj.headers = Object.keys(this.fileDetialsObj.finalData[0]);
        console.log(this.fileDetialsObj.headers);
        this.fileDetialsObj.dataType = ds.headers.map((item: any) => item.colType);
        this.fileDetialsObj.totalRows = ds.fileLength;
        this.fileDetialsObj.dataSource = new MatTableDataSource<any>(this.fileDetialsObj.finalData);
        this.fileDetialsObj.sourceName = ds.sourceName;
        this.fileDetialsObj.actionSourceName = ds.actionSourceName;
        this.fileDetialsObj.fileName = ds.fileName;
        this.fileDetialsObj.sourceType = ds.sourceType;
        this.fileService.addActionObject(new FileDetails(this.fileDetialsObj.fileId, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType, this.fileDetialsObj.sourceType, ""));
        this.fileService.addVisObject(new FileDetails(this.fileDetialsObj.fileId, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType, this.fileDetialsObj.sourceType, ""));
        this.openSnackBar('Python Script Uploaded Successfully!', true);
      });
      console.log(this.fileDetialsObj);
    }
    catch (error) {
      this.openSnackBar(error.error, false);
    }
    fileInput.value = '';
  }

  openSnackBar(msg: string, isSuccess: boolean) {
    var panelClass = ''
    if (isSuccess == true) {
      panelClass = 'success-snackbar'
    }
    else {
      panelClass = 'error-snackbar'
    }
    const snackBarRef = this._snackBar.open(msg, '', {
      duration: 2500,
      horizontalPosition: this.horizontalPosition,
      verticalPosition: this.verticalPosition,
      panelClass: 'success-snackbar'
    });

    setTimeout(() => {
      snackBarRef.dismiss();
    }, 2500);
  }

  onChartSelect() {
    console.log(this.selectedChart);
    this.setCharts = true;
    this.checked = false;
    // const result = this.savedCharts.find(obj => obj.headerLabel === this.selectedChart);
    // console.log(result);
    if (this.selectedChart) {
      this.destroyChart();
      const canvas = document.createElement('canvas');
      canvas.width = 400;
      canvas.height = 400;
      canvas.id = `chart-${this.selectedChart.id}`;
      this.chartContainer.nativeElement.appendChild(canvas);
      const ctx = canvas.getContext('2d');
      // const canvas = document.getElementById('chartCanvas') as HTMLCanvasElement;
      // const ctx = canvas.getContext('2d');
      if (ctx) {
        this.chartInstance = new Chart(ctx, {
          type: this.selectedChart.type,
          data: {
            labels: this.selectedChart.labels,
            datasets: [{
              label: this.selectedChart.headerLabel,
              data: this.selectedChart.data,
              backgroundColor: this.selectedChart.backgroundColor,
              borderWidth: this.selectedChart.borderWidth,
              tension: this.selectedChart.tension
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
            },
            plugins: {
              legend: {
                labels: {
                  color: 'white'
                }
              }
            },
            elements: {
              line: {
                borderColor: 'white' // Set the color of the lines to white
              }
            }
          }
        });
      }
    }
  }

  destroyChart() {
    if (this.chartInstance) {
      this.chartInstance.destroy();
      this.chartInstance = null;
      this.chartContainer.nativeElement.innerHTML = '';
    }
  }

  destroyAllChart() {
    if (this.allChartInstance) {
      this.allChartInstance.destroy();
      this.allChartInstance = null;
      this.allChartContainer.nativeElement.innerHTML = '';
    }
  }

  toggleShowAllCharts() {
    this.checked = true;
    console.log(this.checked);
    this.setCharts = false;
    // if (this.allChartContainer) {
    console.log(this.savedCharts);
    this.savedCharts.forEach(chart => {
      this.destroyAllChart();
      const canvas = document.createElement('canvas');
      // canvas.width = 400;
      // canvas.height = 400;
      canvas.id = `chart-${chart.id}`;
      // canvas.style.marginRight = '15px';
      canvas.className ='col-md-6';
      canvas.style.height='100%';
      canvas.style.width = '100%';
      canvas.style.display = 'inline';
      this.allChartContainer.nativeElement.appendChild(canvas);
      const ctx = canvas.getContext('2d');
      if (ctx) {
        this.chartInstance = new Chart(ctx, {
          type: chart.type,
          data: {
            labels: chart.labels,
            datasets: [{
              label: chart.headerLabel,
              data: chart.data,
              backgroundColor: chart.backgroundColor,
              borderWidth: chart.borderWidth,
              tension: chart.tension
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
            },
            plugins: {
              legend: {
                labels: {
                  color: 'white',
                  font: {
                    size: 11
                  }
                }
              }
            },
            elements: {
              line: {
                borderColor: 'white' // Set the color of the lines to white
              }
            }
          }
        });
      }
    });
    // }
  }
}
