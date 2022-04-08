import { Injectable } from '@angular/core';
import { HttpHeaders } from '@angular/common/http';
import { HttpClient } from '@angular/common/http'
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AddedFreight } from '../model/AddedFreight';

@Injectable({
  providedIn: 'root'
})
export class UploadService {

  constructor(private http: HttpClient) { }

  url = 'http://localhost:44341/ExcelUpload';

  UploadExcel(formData: FormData) {
    let headers = new HttpHeaders();

    headers.append('Content-Type', 'multipart/form-data');
    headers.append('Accept', 'application/json');

    const httpOptions = { headers: headers };

    return this.http.post(this.url + '/Upload', formData, httpOptions)
  }

  //GetAddedFreightsDetails()
  //{
  //  return this.http.get(this.apiUrl + '/GetAddedFreightsDetails')
  //    .pipe(
  //      map(res => res),
  //      catchError(this.errorHandler)
  //  );
  //}

  //errorHandler(error: Response) {
  //  console.log(error);
  //  return throwError(error);
  //}
}
