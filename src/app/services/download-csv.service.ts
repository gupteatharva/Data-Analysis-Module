import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GLOBAL_URL } from 'app/globals';

@Injectable({
  providedIn: 'root'
})
export class DownloadCSVService {

  constructor(private http: HttpClient) { }

  dlCSV(reqBody: any)
  {
    return this.http.post(`${GLOBAL_URL}api/Uploader/download/csv`, reqBody, {responseType: 'blob'});
  }
}
