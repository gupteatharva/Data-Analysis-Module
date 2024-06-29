import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SharedServiceService {

  constructor() { }

  private draggedDataSubject = new BehaviorSubject<string>('');

  setDraggedData(data: string) {
    this.draggedDataSubject.next(data);
  }

  getDraggedData() {
    return this.draggedDataSubject.asObservable();
  }
}
