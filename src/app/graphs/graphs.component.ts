import { Component, Input, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { FileDetails } from 'app/file.model';
import { GraphDetails } from 'app/graphs.model';
import { FileService } from 'app/services/file.service';
import { VisualizationService } from 'app/services/visualization.service';
import { Chart, registerables } from 'chart.js';

@Component({
  selector: 'app-graphs',
  templateUrl: './graphs.component.html',
  styleUrls: ['./graphs.component.css']
})
export class GraphsComponent implements OnInit {
  @Input() graphValue!: string
  @Input() dataSourceValue!: any;
  @Input() inputDataX!: string;
  @Input() inputDataY!: string;
  // @Input() onSelectChange!: () => boolean;
  canvas: any;
  ctx: any;
  test: boolean = false
  graphType!: any;
  graphX!: any;
  graphY!: any;
  headers!: any;
  fileObjects: FileDetails[] = [];
  visObjects: FileDetails[] = [];
  columnOptionsObj = new FileDetails(0, [], new MatTableDataSource<any>, [], 0, "", "", "", "", [], "", "");
  graphObjects: GraphDetails[] = [];
  graphDetailsObj = new GraphDetails('', [], [], []);
  constructor(private fileService: FileService, private visualizationService: VisualizationService) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.fileObjects = this.fileService.getFileObjects();
    // console.log(this.fileObjects.length);
    this.visObjects = this.fileService.getVisObjects();
    console.log(this.visObjects);
  }

  drop(event: DragEvent) {
    event.preventDefault();
    // Handle the data here and update the input field
  }

  dragOver(event: DragEvent) {
    event.preventDefault();
  }

  changeOption(sourceName: string) {
    console.log(sourceName);
    this.columnOptionsObj = this.visObjects.find(item => item.actionSourceName === sourceName);
    console.log(this.columnOptionsObj);
  }

  performBarGraph(dataSourceValue: any, inputDataX: string, inputDataY: string) {
    console.log(dataSourceValue);

    const result = this.visObjects.find(obj => obj.actionSourceName === dataSourceValue);
    console.log(result);
    this.visualizationService.barGraph(result.fileId, result?.actionSourceName, inputDataX, inputDataY, 25, 1, result.sourceType, result.tableName == "" ? null : result.tableName).subscribe(res => {
      console.log(res);
      this.graphDetailsObj.type = 'bar';
      this.graphDetailsObj.xData = res.dataOne;
      this.graphDetailsObj.yData = res.datatwo;
      this.graphDetailsObj.headers = res.header;
      this.graphType = this.graphDetailsObj.type;
      this.graphX = this.graphDetailsObj.xData;
      this.graphY = this.graphDetailsObj.yData;
      this.headers = this.graphDetailsObj.headers;
    });
    console.log(this.graphDetailsObj);
    console.log(this.headers);
    this.test = true;
  }
}
