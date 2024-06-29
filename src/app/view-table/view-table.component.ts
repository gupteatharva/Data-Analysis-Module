import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { FileDetails } from 'app/file.model';
import { FileService } from 'app/services/file.service';
import { SendFileService } from 'app/services/send-file.service';

@Component({
  selector: 'app-view-table',
  templateUrl: './view-table.component.html',
  styleUrls: ['./view-table.component.css']
})
export class ViewTableComponent implements OnInit {

  pageIndex = 0;
  actionObjects: FileDetails[] = [];
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

  nextPage(event: PageEvent) {
    console.log("here");
    this.pageIndex = event.pageIndex + 1;
    this.sendFileService.getFile(this.fileId, 25, this.pageIndex).subscribe(res => {
      console.log(res)
      const index = this.actionObjects.findIndex(obj => obj.fileId === this.fileId);
      console.log(index)
      if (index > -1) {
        this.actionObjects[index].finalData = res.data;
        this.actionObjects[index].headers = Object.keys(res.data[0]);
        this.actionObjects[index].totalRows = res.totalRows;
        this.actionObjects[index].dataType = res.header.map((item: any) => item.colType);
        this.actionObjects[index].dataSource = new MatTableDataSource<any>(res.data);
        this.actionObjects[index].sourceName = res.sourcename;
        this.actionObjects[index].actionSourceName = res.sourcename;
        this.actionObjects[index].fileName = res.fileName;
      }
    })
  }

  ngOnInit(): void {
    this.actionObjects = this.fileService.getActionObjects();
    console.log(this.actionObjects);
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
