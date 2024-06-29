import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { FileDetails } from 'app/file.model';
import { GLOBAL_URL } from 'app/globals';
import { ActionService } from 'app/services/action.service';
import { FileService } from 'app/services/file.service';
import { SharedService } from 'app/services/shared.service';
import {
  MatSnackBar,
  MatSnackBarHorizontalPosition,
  MatSnackBarVerticalPosition,
} from '@angular/material/snack-bar';
@Component({
  selector: 'app-actions',
  templateUrl: './actions.component.html',
  styleUrls: ['./actions.component.css']
})
export class ActionsComponent implements OnInit {
  @Input() selectedValue!: any;
  @Input() dataSourceValue!: any;
  horizontalPosition: MatSnackBarHorizontalPosition = 'end';
  verticalPosition: MatSnackBarVerticalPosition = 'top';
  dataSourceName: string[] = [];
  leftDataSourceName!: string;
  rightDataSourceName!: string;
  leftOn!: string;
  rightOn!: string;
  secondDataSourceName: string[] = [];
  fileObjects: FileDetails[] = [];
  actionObjects: FileDetails[] = [];
  visObjects: FileDetails[] = [];
  inputData: string = "";
  fileActionObj = new FileDetails(0, [], new MatTableDataSource<any>, [], 0, "", "", "", "", [], "", "");
  optionSourceName: string = "";
  constructor(public fileService: FileService, private actionService: ActionService, private http: HttpClient, private sharedService: SharedService, private _snackBar: MatSnackBar) { }
  panelOpenState = false

  ngOnInit(): void {
    this.fileObjects = this.fileService.getFileObjects();
    console.log(this.fileObjects.length);
    this.actionObjects = this.fileService.getActionObjects();
    console.log(this.actionObjects)
    console.log(this.selectedValue)
    this.sharedService.getDraggedData().subscribe(data => {
      this.inputData = data[0];
      this.optionSourceName = data[1];
      this.leftOn = data[0]
      this.leftDataSourceName = data[1]
    });
    this.sharedService.getDraggedDataTwo().subscribe(data => {
      this.rightOn = data[0]
      this.rightDataSourceName = data[1]
    });
    this.sharedService.getDataSourceDraggedData().subscribe(data => {
      this.dataSourceName = data;
    })
    this.sharedService.getDataSourceTwoDraggedData().subscribe(data => {
      this.secondDataSourceName = data;
    })
    // this.sharedService.getLeftOnDraggedData().subscribe(data => {
    //   this.leftOn = data[0];
    // })
    // this.sharedService.getRightOnDraggedData().subscribe(data => {
    //   this.rightOn = data[0];
    // })
    console.log(this.optionSourceName)
  }
  clearData() {
    this.sharedService.clearData();
  }
  drop(event: DragEvent) {
    event.preventDefault();
  }

  dragOver(event: DragEvent) {
    event.preventDefault();
  }

  async saveDataSource(actionSourceName: string) {
    console.log(actionSourceName);
    console.log(this.fileObjects);
    console.log(this.actionObjects);
    const result = this.fileObjects.find(obj => obj.actionSourceName === actionSourceName) || this.actionObjects.find(obj => obj.actionSourceName === actionSourceName);
    console.log(result);
    let reqBody = {
      sourceName: result.sourceName,
      actionName: result?.actionName,
      actionSourceName: result?.actionSourceName,
      fileId: result?.fileId,
      fileName: result?.fileName,
      totalRows: result?.totalRows,
      tableName: result?.tableName || "null"
    };
    console.log("reqBody", reqBody);
    try {
      const res = await this.http.post<any>(`${GLOBAL_URL}api/Uploader/CreateNewDataSource`, reqBody).toPromise()
      console.log(res);
      this.fileService.addNewDataSources(res)
      // this.fileService.addFileObject(res)
      console.log(this.fileObjects);
      this.openSnackBar('Data Source Saved Successfully!', true);
    }
    catch (e) {
      this.openSnackBar(e.error, false);
    }
  }

  async performFillNa(dataSourceValue: any) {
    console.log(this.fileObjects);
    console.log(this.actionObjects);
    const result = this.fileObjects.find(obj => obj.sourceName === dataSourceValue) || this.actionObjects.find(obj => obj.actionSourceName === dataSourceValue);
    console.log(result);
    try {

      const res = await this.actionService.fillNa(result!.fileId, 25, 1, result.actionSourceName, result.sourceType, result.tableName).toPromise()
      console.log(res);
      // this.fileActionObj.fileId = result!.fileId;
      this.fileActionObj.finalData = res.data;
      this.fileActionObj.headers = Object.keys(this.fileActionObj.finalData[0]);
      this.fileActionObj.totalRows = res.fileLength
      this.fileActionObj.dataSource = new MatTableDataSource<any>(this.fileActionObj.finalData);
      this.fileActionObj.sourceName = res.sourcename
      this.fileActionObj.actionSourceName = res.actionSourceName
      this.fileActionObj.fileName = res.fileName
      this.fileActionObj.actionName = res.actionName
      this.fileActionObj.dataType = res.header.map((item: any) => item.colType)
      this.fileActionObj.sourceType = res.sourceType;
      this.fileActionObj.tableName = res.tableName || null;
      // this.fileObjects.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType));
      // this.fileService.addFileObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType));
      this.fileService.addActionObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));

      this.openSnackBar('Action Performed Successfully!', true);
    }
    catch (error) {
      this.openSnackBar(error.error, false);
    }
    // this.sharedService.clearData();
  }

  async performAggregate(dataSourceValue: any, operationsValue: any, inputData: any) {
    const result = this.fileObjects.find(obj => obj.sourceName === dataSourceValue) || this.actionObjects.find(obj => obj.actionSourceName === dataSourceValue);
    console.log(result);
    try {
      const res = await this.actionService.aggregate(result!.fileId, operationsValue, inputData, 25, 1, result.actionSourceName, result.sourceType, result.tableName).toPromise();
      console.log(res);
      // this.fileActionObj.fileId = result!.fileId;
      this.fileActionObj.finalData = res.data;
      this.fileActionObj.headers = Object.keys(this.fileActionObj.finalData[0]);
      this.fileActionObj.totalRows = res.fileLength
      this.fileActionObj.dataSource = new MatTableDataSource<any>(this.fileActionObj.finalData);
      this.fileActionObj.sourceName = res.sourcename
      this.fileActionObj.actionSourceName = res.actionSourceName
      this.fileActionObj.fileName = res.fileName
      this.fileActionObj.actionName = res.actionName
      this.fileActionObj.dataType = res.header.map((item: any) => item.colType)
      this.fileActionObj.sourceType = res.sourceType;
      this.fileActionObj.tableName = res.tableName || null;
      console.log(this.fileActionObj)
      // this.fileObjects.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType));
      // console.log(this.fileObjects);
      this.fileService.addActionObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      // this.fileService.addFileObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType));
      // this.newDataSource.push(this.fileActionObj)
      // console.log(this.newDataSource)

      this.openSnackBar('Action Performed Successfully!', true);
    }
    catch (error) {
      console.log(error)
      this.openSnackBar(error.error, false);
    }
    // this.sharedService.clearData();
  }

  async performGroupBy(dataSourceValue: any, operationsValue: any, inputData: any) {
    // const res = this.actionObjects.find(obj => obj.actionSourceName === dataSourceValue);
    // console.log(res)
    const result = this.fileObjects.find(obj => obj.sourceName === dataSourceValue) || this.actionObjects.find(obj => obj.actionSourceName === dataSourceValue);
    console.log(result);
    try {
      const res = await this.actionService.groupby(result!.fileId, operationsValue, inputData, 25, 1, result.actionSourceName, result.sourceType, result.tableName).toPromise();
      console.log(res);
      // this.fileActionObj.fileId = result!.fileId;
      this.fileActionObj.finalData = res.data;
      this.fileActionObj.headers = Object.keys(this.fileActionObj.finalData[0]);
      this.fileActionObj.totalRows = res.fileLength
      this.fileActionObj.dataSource = new MatTableDataSource<any>(this.fileActionObj.finalData);
      this.fileActionObj.sourceName = res.sourcename
      this.fileActionObj.actionSourceName = res.actionSourceName
      this.fileActionObj.fileName = res.fileName
      this.fileActionObj.actionName = res.actionName
      this.fileActionObj.dataType = res.header.map((item: any) => item.colType);
      this.fileActionObj.sourceType = res.sourceType;
      this.fileActionObj.tableName = res.tableName || null;
      console.log(this.fileActionObj)
      // this.fileObjects.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType));
      // console.log(this.fileObjects);
      // this.fileService.addFileObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType));
      this.fileService.addActionObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      // this.newDataSource.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName))
      // console.log(this.newDataSource)
      this.openSnackBar('Action Performed Successfully!', true);
    }
    catch (e) {
      this.openSnackBar(e.error, false);
    }
    // this.sharedService.clearData();
  }

  async performConcat(dataSourceValue: any, dataSourceValue2: any) {
    // sourcetype
    console.log(dataSourceValue)
    console.log(dataSourceValue2)
    const result1 = this.fileObjects.find(obj => obj.sourceName === dataSourceValue);
    const result2 = this.fileObjects.find(obj => obj.sourceName === dataSourceValue2);
    console.log(result1);
    console.log(result2);
    try {
      const res = await this.actionService.concat(result1!.fileId, result2!.fileId, 25, 1).toPromise()
      console.log(res);
      this.fileActionObj.finalData = res.data;
      this.fileActionObj.headers = Object.keys(this.fileActionObj.finalData[0]);
      this.fileActionObj.totalRows = res.fileLength
      this.fileActionObj.dataSource = new MatTableDataSource<any>(this.fileActionObj.finalData);
      this.fileActionObj.sourceName = res.actionName
      this.fileActionObj.actionSourceName = res.actionSourceName
      this.fileActionObj.fileName = res.fileName
      this.fileActionObj.actionName = res.actionName
      this.fileActionObj.dataType = res.header.map((item: any) => item.colType)
      this.fileActionObj.tableName = null;
      console.log(this.fileActionObj)

      // this.fileObjects.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType));
      // console.log(this.fileObjects);
      // this.fileService.addFileObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType));
      this.fileService.addActionObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      // this.newDataSource.push(this.fileActionObj)
      // console.log(this.newDataSource)
      this.openSnackBar('Action Performed Successfully!', true);
    }
    catch (e) {
      this.openSnackBar(e.error, false);
    }
    // this.sharedService.clearData();
  }

  async performDelCols(inputData: any, dataSourceValue: any) {
    const result = this.fileObjects.find(obj => obj.sourceName === dataSourceValue) || this.actionObjects.find(obj => obj.actionSourceName === dataSourceValue);
    console.log(result);
    try {
      const res = await this.actionService.delCols(result!.fileId, 25, 1, inputData, result.actionSourceName, result.sourceType, result.tableName).toPromise()
      console.log(res);
      // this.fileActionObj.fileId = result!.fileId;
      this.fileActionObj.finalData = res.data;
      this.fileActionObj.headers = Object.keys(this.fileActionObj.finalData[0]);
      this.fileActionObj.totalRows = res.fileLength
      this.fileActionObj.dataSource = new MatTableDataSource<any>(this.fileActionObj.finalData);
      this.fileActionObj.sourceName = res.sourcename
      this.fileActionObj.actionSourceName = res.actionSourceName
      this.fileActionObj.fileName = res.fileName
      this.fileActionObj.actionName = res.actionName
      this.fileActionObj.dataType = res.header.map((item: any) => item.colType);
       this.fileActionObj.sourceType = res.sourceType;
       this.fileActionObj.tableName = res.tableName || null;
      console.log(this.fileActionObj)
      // this.fileService.addFileObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType));
      this.fileService.addActionObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      // this.newDataSource.push(this.fileActionObj)
      // this.fileObjects.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType));
      // console.log(this.fileObjects);
      // this.newDataSource.push(this.fileActionObj)
      // console.log(this.newDataSource)
      this.openSnackBar('Action Performed Successfully!', true);
    }
    catch (e) {
      this.openSnackBar(e.error, false);
    }
    // this.sharedService.clearData();
  }

  async performAverage(inputData: any, dataSourceValue: any) {
    const result = this.fileObjects.find(obj => obj.sourceName === dataSourceValue) || this.actionObjects.find(obj => obj.actionSourceName === dataSourceValue);
    console.log(result);
    try {
      const res = await this.actionService.average(result!.fileId, inputData, result.actionSourceName, result.sourceType, result.tableName).toPromise()
      console.log(res);
      // this.fileActionObj.fileId = result!.fileId;
      this.fileActionObj.finalData = res.data;
      this.fileActionObj.headers = Object.keys(this.fileActionObj.finalData[0]);
      this.fileActionObj.totalRows = res.fileLength
      this.fileActionObj.dataSource = new MatTableDataSource<any>(this.fileActionObj.finalData);
      this.fileActionObj.sourceName = res.sourcename
      this.fileActionObj.actionSourceName = res.actionSourceName
      this.fileActionObj.fileName = res.fileName
      this.fileActionObj.actionName = res.actionName
      this.fileActionObj.dataType = res.header.map((item: any) => item.colType);
      this.fileActionObj.sourceType = res.sourceType;
      this.fileActionObj.tableName = res.tableName || null;
      console.log(this.fileActionObj)
      // this.fileService.addFileObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType));
      this.fileService.addActionObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      // this.newDataSource.push(this.fileActionObj)
      // this.fileObjects.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType));
      // console.log(this.fileObjects);
      // this.newDataSource.push(this.fileActionObj)
      // console.log(this.newDataSource)
      this.openSnackBar('Action Performed Successfully!', true);
    }
    catch (e) {
      this.openSnackBar(e.error, false);
    }
    // this.sharedService.clearData();
  }

  async performJoin(leftDataSource: string, rightDataSource: string, leftOn: string, rightOn: string, how: string) {
    // sourcetype
    const result1 = this.fileObjects.find(obj => obj.sourceName === leftDataSource);
    const result2 = this.fileObjects.find(obj => obj.sourceName === rightDataSource);
    console.log(result1);
    console.log(result2);
    try {
      const res = await this.actionService.join(result1!.fileId, result2!.fileId, 25, 1, leftDataSource, rightDataSource, leftOn, rightOn, how, result1.sourceType, result1.tableName, result2.sourceType, result2.tableName, result1.actionSourceName, result2.actionSourceName).toPromise()
      console.log(res);
      this.fileActionObj.finalData = res.data;
      this.fileActionObj.headers = Object.keys(this.fileActionObj.finalData[0]);
      this.fileActionObj.totalRows = res.fileLength;
      this.fileActionObj.dataSource = new MatTableDataSource<any>(this.fileActionObj.finalData);
      this.fileActionObj.sourceName = res.sourcename;
      this.fileActionObj.actionSourceName = res.actionSourceName;
      this.fileActionObj.fileName = res.fileName;
      this.fileActionObj.actionName = res.actionName;
      this.fileActionObj.dataType = res.header.map((item: any) => item.colType);
      this.fileActionObj.tableName = res.tableName || null;
      this.fileActionObj.sourceType = "CSV";
      console.log(this.fileActionObj);

      // this.fileObjects.push(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType));
      // console.log(this.fileObjects);
      // this.fileService.addFileObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, "", this.fileActionObj.dataType));
      this.fileService.addActionObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType,  this.fileActionObj.sourceType, this.fileActionObj.tableName));
      this.fileService.addVisObject(new FileDetails(this.fileActionObj.fileId, this.fileActionObj.finalData, this.fileActionObj.dataSource, this.fileActionObj.headers, this.fileActionObj.totalRows, this.fileActionObj.sourceName, this.fileActionObj.fileName, this.fileActionObj.actionSourceName, this.fileActionObj.actionName, this.fileActionObj.dataType, this.fileActionObj.sourceType, this.fileActionObj.tableName));
      // this.newDataSource.push(this.fileActionObj)
      // console.log(this.newDataSource)
      this.openSnackBar('Action Performed Successfully!', true);
    }
    catch (e) {
      this.openSnackBar(e.error, false);
    }
    // this.sharedService.clearData();
  }

  openSnackBar(msg: string, isSuccess: boolean) {
    var panelClass = ''
    if (isSuccess === true) {
      panelClass = 'success-snackbar'
    }
    else {
      panelClass = 'error-snackbar'
    }
    const snackBarRef = this._snackBar.open(msg, '', {
      duration: 2500,
      horizontalPosition: this.horizontalPosition,
      verticalPosition: this.verticalPosition,
      panelClass: panelClass
    });

    setTimeout(() => {
      snackBarRef.dismiss();
    }, 2500);
  }
}
