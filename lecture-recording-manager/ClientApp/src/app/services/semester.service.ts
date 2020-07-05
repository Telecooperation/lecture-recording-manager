import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { Semester } from '../shared/semester';

import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SemesterService {

  constructor(private http: HttpClient) { }

  getSemesters(): Observable<Semester[]> {
    return this.http.get<Semester[]>(environment.apiUrl + '/api/semester');
  }

  getSemester(id: number): Observable<Semester> {
    return this.http.get<Semester>(environment.apiUrl + '/api/semester/' + id);
  }
}
