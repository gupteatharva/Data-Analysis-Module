import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GLOBAL_URL } from 'app/globals';

@Injectable({
  providedIn: 'root'
})
export class ActionService {

  constructor(private http: HttpClient) { }

  fillNa(id: number, pageSize: number, pageNumber: number, actionSourceName: string, sourceType:string, tableName: string =null) {
    return this.http.get<any>(`${GLOBAL_URL}api/Action/fillNa/${id}/${pageSize}/${pageNumber}/${sourceType}/${tableName}?actionSourceName=${actionSourceName}`)
  }

  aggregate(id: number, operations: any, columns: any, pageSize: number, pageNumber: number, actionSourceName: string, sourceType:string, tableName: string) {
    return this.http.get<any>(`${GLOBAL_URL}api/Action/aggregate/${id}/${pageSize}/${pageNumber}/${sourceType}/${tableName}?actionSourceName=${actionSourceName}&operations=${operations}&columns=${columns}`)
  }

  groupby(id: number, operations: any, columns: any, pageSize: number, pageNumber: number, actionSourceName: string, sourceType:string, tableName: string) {
    return this.http.get<any>(`${GLOBAL_URL}api/Action/groupby/${id}/${pageSize}/${pageNumber}/${sourceType}/${tableName}?actionSourceName=${actionSourceName}&column=${columns}&operation=${operations}`)
  }

  concat(id1: number, id2: number, pageSize: number, pageNumber: number) {
    return this.http.get<any>(`${GLOBAL_URL}api/Action/concat/${id1}/${id2}/${pageSize}/${pageNumber}`)
  }

  delCols(id: number, pageSize: number, pageNumber: number, inputData: number, actionSourceName: string, sourceType:string, tableName: string) {
    let string = [];
    string.push(inputData);
    return this.http.post<any>(`${GLOBAL_URL}api/Action/cols/${id}/${pageSize}/${pageNumber}/${sourceType}/${tableName}?actionSourceName=${actionSourceName}`, string)
  }

  average(id: number, column: string, actionSourceName: string, sourceType:string, tableName: string)
  {
    return this.http.get<any>(`${GLOBAL_URL}api/Action/avg/${id}/${column}/${sourceType}/${tableName}?actionSourceName=${actionSourceName}`);
  }

  join(id1: number, id2: number, pageSize: number, pageNumber: number, left: string, right: string, left_on:string, right_on:string, how: string, sourceType1:string, tableName1: string, sourceType2:string, tableName2: string, actionSourceName1: string, actionSourceName2: string)
  {
    tableName1 == "" ? null : tableName1;
    console.log(tableName1);
    tableName2 == "" ? null : tableName2;
    console.log(tableName2);
    return this.http.get<any>(`${GLOBAL_URL}api/Action/join/${id1}/${id2}/${pageSize}/${pageNumber}/${sourceType1}/${tableName1}/${sourceType2}/${tableName2}?left=${left}&right=${right}&left_on=${left_on}&right_on=${right_on}&how=${how}&actionSourceName1=${actionSourceName1}&actionSourceName2=${actionSourceName2}`)
  }
}
