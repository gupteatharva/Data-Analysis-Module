import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GLOBAL_URL } from 'app/globals';

@Injectable({
  providedIn: 'root'
})
export class VisualizationService {
  
  constructor(private http: HttpClient) { }

  barGraph(id:number, actionSourceName:string, inputX:string, inputY: string, pageSize:number, pageNumber:number, sourceType: string, tableName: string)
  {
    let string = [];
    string.push(inputX);
    string.push(inputY);
    return this.http.post<any>(`${GLOBAL_URL}api/Action/getcols/${id}/${actionSourceName}/${pageSize}/${pageNumber}/${sourceType}/${tableName}`, string) 
  }
}
