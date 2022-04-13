import { Injectable } from '@angular/core';
import { HttpHeaders } from '@angular/common/http';
import { HttpClient } from '@angular/common/http'
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AddedFreight } from '../model/AddedFreight';
import { DisplayMonths } from '../model/DisplayMonths';

@Injectable({
  providedIn: 'root'
})
export class UploadService {

  constructor(private http: HttpClient) { }

  url = 'http://localhost:44341/ExcelUpload';
  apiUrl = 'https://localhost:44341';

  UploadExcel(formData: FormData) {
    let headers = new HttpHeaders();

    headers.append('Content-Type', 'multipart/form-data');
    headers.append('Accept', 'application/json');

    const httpOptions = { headers: headers };

    return this.http.post(this.url + '/Upload', formData, httpOptions)
  }

  PostAddedFreightsDetails(addedFreight) {
    let headers = new HttpHeaders();
    headers.append('Content-Type', 'multipart/form-data');
    headers.append('Accept', 'application/json');

    const httpOptions = { headers: headers };
    return this.http.post('https://localhost:44341/PostAddedFreightsDetails', addedFreight, httpOptions);
  }

  UpdateDisplayMonthsDetails(displayMonths: DisplayMonths): Observable<string> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };
    return this.http.put<string>(`${this.apiUrl}/UpdateDisplayMonthsDetails`, displayMonths, httpOptions);
  }
}
