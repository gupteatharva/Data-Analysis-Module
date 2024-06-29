import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { FileDetails } from 'app/file.model';
import { FileService } from 'app/services/file.service';
import { SendFileService } from 'app/services/send-file.service';

@Component({
  selector: 'app-table-list',
  templateUrl: './table-list.component.html',
  styleUrls: ['./table-list.component.css']
})
export class TableListComponent implements OnInit {
  fileObjects: FileDetails[] = [];
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  constructor(public fileService: FileService) { }
  ngOnInit() {
    this.fileObjects = this.fileService.getFileObjects();
  }


}
