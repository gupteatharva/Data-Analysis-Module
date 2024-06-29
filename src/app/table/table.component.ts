import { HttpClient } from '@angular/common/http';
import { Component, Input, Output, EventEmitter, ViewChild, OnInit, OnChanges, ChangeDetectorRef  } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { FileDetails } from 'app/file.model';
import { FileService } from 'app/services/file.service';
import { SendFileService } from 'app/services/send-file.service';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})
export class TableComponent implements OnInit {
  pageIndex = 0;
  fileObjects: FileDetails[] = [];
  headers: string[] = [];
  dataTypes: string[] = [];
  totalRows!: number;
  fileId!: number;
  dataSource!: any
  finalData!: any
  actionName!: string
  actionSourceName!: string
  fileName!: string
  nextPageData = new FileDetails(0, [], new MatTableDataSource<any>, [], 0, "", "", "", "", [], "", "");
  
  constructor(private sendFileService: SendFileService, public fileService: FileService, private cdr: ChangeDetectorRef) { }

  nextPage(event: PageEvent, id: number) {
    console.log("here", id);
    this.pageIndex = event.pageIndex + 1;
    this.sendFileService.getFile(this.fileId, 25, this.pageIndex).subscribe(res => {
      console.log(res)
      const index = this.fileObjects.findIndex(obj => obj.fileId === this.fileId);
      console.log(index)
      if (index > -1) {
        this.fileObjects[index].finalData = res.data;
        this.fileObjects[index].headers = Object.keys(res.data[0]);
        this.fileObjects[index].totalRows = res.totalRows;
        this.fileObjects[index].dataType = res.header.map((item: any) => item.colType);
        this.fileObjects[index].dataSource = new MatTableDataSource<any>(res.data);
        this.fileObjects[index].sourceName = res.sourcename;
        this.fileObjects[index].actionSourceName = res.sourcename;
        this.fileObjects[index].fileName = res.fileName;
      }
    })
  }

  ngOnInit(): void {
    this.fileObjects = this.fileService.getFileObjects();
    console.log(this.fileObjects);
    this.headers = this.fileService.getHeaders();
    this.dataTypes = this.fileService.getDatatypes();
    this.totalRows = this.fileService.getTotalRows();
    this.fileId = this.fileService.getFileId();
    this.dataSource = this.fileService.getDataSource();
    console.log(this.dataSource)
    this.finalData = this.fileService.getFinalData();
    this.actionName = this.fileService.getActionName();
    this.actionSourceName = this.fileService.getActionSourceName();
    this.fileName = this.fileService.getFileName();
  }
  

}
