import { Component, OnInit } from '@angular/core';
import { FileDetails } from 'app/file.model';
import { FileService } from 'app/services/file.service';
import {MatInputModule} from '@angular/material/input';

interface Action {
  value: string;
  viewValue: string;
  viewColumns: boolean;
  viewOperations: boolean;
}
@Component({
  selector: 'app-data-analysis',
  templateUrl: './data-analysis.component.html',
  styleUrls: ['./data-analysis.component.css']
})
export class DataAnalysisComponent implements OnInit {
  fileObjects: FileDetails[] = [];
  actionObjects: FileDetails[] = [];
  selectedValue!: string;
  constructor(private fileService: FileService) { }
  actions: Action[] = [
    { value: 'FillNullValues', viewValue: 'Fill Null values', viewColumns: false, viewOperations: false },
    { value: 'Aggregate', viewValue: 'Aggregate', viewColumns: true, viewOperations: true },
    { value: 'GroupBy', viewValue: 'GroupBy', viewColumns: true, viewOperations: true },
    // { value: 'Concat', viewValue: 'Concat', viewColumns: false, viewOperations: false },
    { value: 'DeleteColumns', viewValue: 'Delete Columns', viewColumns: true, viewOperations: false },
    { value: 'Average', viewValue: 'Average', viewColumns: true, viewOperations: false },
    { value: 'Join', viewValue: 'Join', viewColumns: true, viewOperations: false },
  ];
  ngOnInit() {
    this.fileObjects = this.fileService.getFileObjects();
    console.log(this.fileObjects)
    this.actionObjects = this.fileService.getActionObjects();
    console.log(this.actionObjects)

  }
}
