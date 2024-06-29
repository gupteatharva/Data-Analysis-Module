import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { FileDetails } from 'app/file.model';
import { FileService } from 'app/services/file.service';
import { SendFileService } from 'app/services/send-file.service';
import {
  MatSnackBar,
  MatSnackBarHorizontalPosition,
  MatSnackBarVerticalPosition,
} from '@angular/material/snack-bar';
// import { FormControl } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { NgFor } from '@angular/common';
import { MatSelect, MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
// interface Food {
//   value: string;
//   viewValue: string;
// }
@Component({
  selector: 'app-data-sources',
  templateUrl: './data-sources.component.html',
  styleUrls: ['./data-sources.component.css'],
})
export class DataSourcesComponent implements OnInit {
  // selectedValue2: string;
  // foods: Food[] = [
  //   {value: 'steak-0', viewValue: 'Steak'},
  //   {value: 'pizza-1', viewValue: 'Pizza'},
  //   {value: 'tacos-2', viewValue: 'Tacos'},
  // ];
  selected = '';
  tables: any[] = [];
  dbname!: string;
  selectedTableName!: string;
  toggleDisplay: boolean = false;

  toppingList: string[] = ['Extra cheese', 'Mushroom', 'Onion', 'Pepperoni', 'Sausage', 'Tomato'];
  horizontalPosition: MatSnackBarHorizontalPosition = 'end';
  verticalPosition: MatSnackBarVerticalPosition = 'top';
  panelOpenState = false;
  selectedFile: any;
  fileObjects: FileDetails[] = [];
  visObjects: FileDetails[] = [];
  fileDetialsObj = new FileDetails(0, [], new MatTableDataSource<any>, [], 0, "", "", "", "", [], "", "");
  newDataSource: FileDetails[] = [];
  headers: string[] = []
  dataTypes: string[] = []

  selectedValue!: string
  constructor(private http: HttpClient, private sendFileservice: SendFileService, private fileService: FileService, private _snackBar: MatSnackBar) { }

  source1(event: any) {
    this.selectedFile = event.target.files[0]
    const formData = new FormData();
    formData.append('file', this.selectedFile);
    this.sendFileservice.sendfile(formData).subscribe(res => {
      console.log(res);
      this.fileDetialsObj.fileId = res.id;
      console.log(this.fileDetialsObj.fileId);
      // this.fileId = res.id
    })
  }

  async onUpload(fileInput: HTMLInputElement) {
    try {
      const res = await this.sendFileservice.getFile(this.fileDetialsObj.fileId, 25, 1).toPromise()
      console.log(res);

      this.fileDetialsObj.finalData = res.data;
      this.fileDetialsObj.headers = Object.keys(this.fileDetialsObj.finalData[0]);
      this.fileDetialsObj.dataType = res.header.map((item: any) => item.colType);
      this.fileDetialsObj.totalRows = res.totalRows;
      this.fileDetialsObj.dataSource = new MatTableDataSource<any>(this.fileDetialsObj.finalData);
      this.fileDetialsObj.sourceName = res.sourcename;
      this.fileDetialsObj.actionSourceName = res.sourcename;
      this.fileDetialsObj.fileName = res.fileName;
      this.fileDetialsObj.dataType = res.header.map((column: any) => column.colType)
      this.fileDetialsObj.sourceType = res.sourceType;
      this.fileDetialsObj.tableName = null;
      console.log(this.fileDetialsObj);
      // this.fileObjects.push(new FileDetails(this.fileDetialsObj.fileId, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType));
      // this.newDataSource.push(new FileDetails(this.fileDetialsObj.fileId, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType));
      this.fileService.addFileObject(new FileDetails(this.fileDetialsObj.fileId, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType, this.fileDetialsObj.sourceType, this.fileDetialsObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileDetialsObj.fileId, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType, this.fileDetialsObj.sourceType, this.fileDetialsObj.tableName));
      this.openSnackBar('Data Source Upload Successfully!', true);
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


  async onConnect(servername: string, dbname: string, username: string, password: string) {
    console.log(servername);
    this.dbname = dbname;
    try {
      const res = await this.sendFileservice.connectToSQL(servername, dbname, username, password).toPromise();
      console.log(res);
      res.tables.map(t => this.tables.push(t));
      console.log(this.tables);
      this.openSnackBar('Connected to your SQL Server Successfully!', true);
      setTimeout(() => {
        this.openSnackBar('Please select a table from your database!', true);
      }, 1500);
    }
    catch (error) {
      this.openSnackBar(error.error, false);
    }
  }

  async uploadTable(tableName: string, dbname: string) {
    console.log(tableName);
    try {
      const res = await this.sendFileservice.getSQLData(dbname, tableName).toPromise();
      console.log(res);

      this.fileDetialsObj.finalData = res.data;
      this.fileDetialsObj.headers = Object.keys(this.fileDetialsObj.finalData[0]);
      this.fileDetialsObj.totalRows = res.totalRows;
      this.fileDetialsObj.dataSource = new MatTableDataSource<any>(this.fileDetialsObj.finalData);
      this.fileDetialsObj.sourceName = res.sourcename;
      this.fileDetialsObj.actionSourceName = res.actionSourceName;
      this.fileDetialsObj.fileName = res.fileName;
      this.fileDetialsObj.dataType = res.header.map((column: any) => column.colType)
      this.fileDetialsObj.sourceType = res.sourceType;
      this.fileDetialsObj.tableName = tableName;
      console.log(this.fileDetialsObj);
      this.fileService.addFileObject(new FileDetails(0, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType, this.fileDetialsObj.sourceType, this.fileDetialsObj.tableName));
      this.fileService.addVisObject(new FileDetails(0, this.fileDetialsObj.finalData, this.fileDetialsObj.dataSource, this.fileDetialsObj.headers, this.fileDetialsObj.totalRows, this.fileDetialsObj.sourceName, this.fileDetialsObj.fileName, this.fileDetialsObj.actionSourceName, "", this.fileDetialsObj.dataType, this.fileDetialsObj.sourceType, this.fileDetialsObj.tableName));
      this.toggleDisplay = !this.toggleDisplay;
      this.selected = '';
      this.openSnackBar('SQL Data Source Upload Successfully!', true);
      console.log(this.toggleDisplay);
    }
    catch (error) {
      this.openSnackBar(error.error, false);
    }
  }

  ngOnInit() {
    this.fileObjects = this.fileService.getFileObjects();
    this.headers = this.fileService.getHeaders();
    this.dataTypes = this.fileService.getDatatypes();
  }

}
