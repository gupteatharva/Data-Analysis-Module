import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GLOBAL_URL } from 'app/globals';

@Injectable({
  providedIn: 'root'
})
export class ScriptService {

  constructor(private http: HttpClient) { }

  genScript()
  {
    return this.http.get(`${GLOBAL_URL}api/Script/script`, { responseType: 'blob' });
  }
}
