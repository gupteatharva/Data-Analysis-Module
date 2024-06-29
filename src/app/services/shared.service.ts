import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SharedService {

  constructor() { }

  private draggedDataSubject = new BehaviorSubject<string[]>([]);
  private draggedDataSubjectTwo = new BehaviorSubject<string[]>([]);
  private draggedDataSourceSubject = new BehaviorSubject<string[]>([]);
  private draggedDataSourceTwoSubject = new BehaviorSubject<string[]>([]);
  private leftOnSubject = new BehaviorSubject<string[]>([]);
  private rightOnSubject = new BehaviorSubject<string[]>([]);

  setDraggedData(data: string[]) {
    if (this.draggedDataSubject.value.length !== 0) {
      this.draggedDataSubjectTwo.next(data);
    }
    else {
      this.draggedDataSubject.next(data);
    }
      // this.draggedDataSubject.next(data);
      console.log(data)
  }

  setDataSourceDraggedData(data: string[]) {
    if (this.draggedDataSourceSubject.value.length !== 0) {
      this.draggedDataSourceTwoSubject.next(data);
    }
    else {

      this.draggedDataSourceSubject.next(data);
    }
    console.log(this.draggedDataSourceSubject.value.length)
  }

  setDataSourceTwoDraggedData(data: string[]) {
    this.draggedDataSourceTwoSubject.next(data);
    console.log(data)
  }

  getDraggedData() {
    this.draggedDataSubjectTwo.next([]);
    return this.draggedDataSubject.asObservable();
  }

  getDraggedDataTwo() {
    this.draggedDataSubject.next([]);
    return this.draggedDataSubjectTwo.asObservable();
  }

  // getLeftOnDraggedData() {
  //   return this.leftOnSubject.asObservable();
  // }

  // getRightOnDraggedData() {
  //   return this.rightOnSubject.asObservable();
  // }

  getDataSourceDraggedData() {
    return this.draggedDataSourceSubject.asObservable();
  }

  getDataSourceTwoDraggedData() {
    return this.draggedDataSourceTwoSubject.asObservable();
  }

  clearData()
  {
    this.draggedDataSubject.next([]);
    this.draggedDataSubjectTwo.next([]);
    this.draggedDataSourceSubject.next([]);
    this.draggedDataSourceTwoSubject.next([]);
  }
}
