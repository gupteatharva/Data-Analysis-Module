import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { retry } from 'rxjs';
import { GLOBAL_URL } from '../globals';

@Injectable({
  providedIn: 'root'
})
export class SendFileService {
  fileId!:number;
  constructor(private http:HttpClient) { }

  sendfile(formData:FormData){
    return this.http.post<any>(`${GLOBAL_URL}api/Uploader/UploadFile`,formData)
  }

  getFile(id:number,pagesize:number,pagenumber:number){
    return this.http.get<any>(`${GLOBAL_URL}api/Uploader/GetFile/${id}/${pagesize}/${pagenumber}`)
  }

  sendpyFile(formData: FormData)
  {
    return this.http.post<any>(`${GLOBAL_URL}api/Script/uploadScript`, formData);
  }

  connectToSQL(servername: string, dbname: string, username:string, password:string)
  {
    return this.http.get<any>(`${GLOBAL_URL}api/Uploader/getSqlConn/${servername}/${dbname}/${username}/${password}/25/1`);
  }

  getSQLData(database: string, tableName:string)
  {
    return this.http.get<any>(`${GLOBAL_URL}api/Uploader/getSqlData/${database}/${tableName}/25/1`);
  }
}