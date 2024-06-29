import { Injectable } from '@angular/core';
import { FileDetails } from 'app/file.model';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private visObjects: FileDetails[] = [];
  private fileObjects: FileDetails[] = [];
  private actionObjects: FileDetails[] = [];
  private newDataSources: FileDetails[] = [];
  headers: string[] = [];
  dataTypes: string[] = [];
  totalRows!: number;
  fileId!: number;
  dataSource!: any
  finalData!: any
  actionName!: string
  actionSourceName!: string
  fileName!: string
  private sourceName: string = "";
  constructor() { }

  addNewDataSources(fileObj: any)
  {
    this.newDataSources.push(fileObj);
    console.log(this.newDataSources)
  }

  addFileObject(fileObj: FileDetails) {
    this.fileObjects.push(fileObj);
    this.headers = fileObj.headers;
    this.dataTypes = fileObj.dataType;
    this.fileId = fileObj.fileId;
    this.totalRows = fileObj.totalRows;
    this.dataSource = fileObj.dataSource;
    this.finalData = fileObj.finalData;
    this.sourceName = fileObj.sourceName
    this.actionName = fileObj.actionName;
    this.actionSourceName = fileObj.actionSourceName;
    this.fileName = fileObj.fileName;
    console.log(this.sourceName)
  }
  
  addActionObject(actionObj: FileDetails) {
    this.actionObjects.push(actionObj);
    this.headers = actionObj.headers;
    this.dataTypes = actionObj.dataType;
    this.fileId = actionObj.fileId;
    this.totalRows = actionObj.totalRows;
    this.dataSource = actionObj.dataSource;
    this.finalData = actionObj.finalData;
    this.sourceName = actionObj.sourceName
    this.actionName = actionObj.actionName;
    this.actionSourceName = actionObj.actionSourceName;
    this.fileName = actionObj.fileName;
    console.log(this.sourceName)
  }

  addVisObject(visObj: FileDetails) {
    this.visObjects.push(visObj);
    this.headers = visObj.headers;
    this.dataTypes = visObj.dataType;
    this.fileId = visObj.fileId;
    this.totalRows = visObj.totalRows;
    this.dataSource = visObj.dataSource;
    this.finalData = visObj.finalData;
    this.sourceName = visObj.sourceName
    this.actionName = visObj.actionName;
    this.actionSourceName = visObj.actionSourceName;
    this.fileName = visObj.fileName;
    console.log(this.sourceName);
  }

  getFileObjects(): FileDetails[] {
    return this.fileObjects;
  }
  
  getActionObjects(): FileDetails[] {
    return this.actionObjects;
  }

  getVisObjects(): FileDetails[] {
    return this.visObjects;
  }

  getHeaders() {
    return this.headers;
  }

  getDatatypes() {
    return this.dataTypes;
  }

  getFileId() {
    return this.fileId;
  }

  getTotalRows() {
    return this.totalRows;
  }

  getDataSource() {
    return this.dataSource;
  }

  getFinalData() {
    return this.finalData;
  }

  getSourceName() {
    return this.sourceName;
  }

  getActionName() {
    return this.actionName;
  }

  getActionSourceName() {
    return this.actionSourceName;
  }
  
  getFileName() {
    return this.fileName;
  }

  clearFileObjects() {
    return this.fileObjects = [];
  }

  clearActionObjects() {
    return this.actionObjects = [];
  }

  clearVisObjects() {
    return this.visObjects = [];
  }

}
