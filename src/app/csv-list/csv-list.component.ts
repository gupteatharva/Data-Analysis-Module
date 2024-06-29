import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FileDetails } from 'app/file.model';
import { DownloadCSVService } from 'app/services/download-csv.service';
import { FileService } from 'app/services/file.service';
import { SharedService } from 'app/services/shared.service';
import { saveAs } from 'file-saver';
@Component({
  selector: 'app-csv-list',
  templateUrl: './csv-list.component.html',
  styleUrls: ['./csv-list.component.css']
})
export class CsvListComponent {
  @Input() dataObjects!: any
  @Input() downloadBtn: boolean =false
  @Input() isDraggable: boolean =true
  constructor(public fileService: FileService, private sharedService: SharedService, private downloadCsvService: DownloadCSVService) { }
   fileObjects: FileDetails[] = [];

  headers: string[] = [];
  sourceName!: string;
  dataTypes: string[] = [];
  // panelOpenState = false;
  panelOpenState = false;

  @Output() dataDropped: EventEmitter<string> = new EventEmitter<string>();

  drop(event: CdkDragDrop<string[]>) {
    // this.sharedService.clearData();
    console.log(event.item.data)
    this.sharedService.setDraggedData(event.item.data);
  }

  dropDataSourceName(event: CdkDragDrop<string[]>)
  {
    console.log(event.item.data);
    this.sharedService.setDataSourceDraggedData(event.item.data);
  }

  todo = this.headers;

  dragStart(event: DragEvent) {
    event.dataTransfer!.setData('text/plain', 'test');
  }
  ngOnInit(): void {
    this.fileObjects = this.fileService.getFileObjects();
    console.log(this.fileObjects)
    console.log(this.dataTypes)
    this.headers = this.fileService.getHeaders();
    this.dataTypes = this.fileService.getDatatypes();
    }

    downloadCSV(sourceName: string, actionSourceName: string)
    {
      let reqBody= {
        sourceName: sourceName,
        actionSourceName: actionSourceName
      };
      this.downloadCsvService.dlCSV(reqBody).subscribe((blob: Blob) => {
        let fileName = actionSourceName.replace(/\//g, "");
        saveAs(blob, fileName+".csv");
      })

    }

}
