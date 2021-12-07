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

  postSemester(semester: Semester): Observable<Semester> {
    return this.http.post<Semester>(environment.apiUrl + '/api/semester', semester);
  }

  putSemester(id: number, semester: Semester): Observable<Semester> {
    return this.http.put<Semester>(environment.apiUrl + '/api/semester/' + id, semester);
  }

  synchronize(): Observable<void> {
    return this.http.get<void>(environment.apiUrl + '/api/semester/synchronize');
  }
}
