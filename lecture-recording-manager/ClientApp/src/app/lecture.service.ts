import { Injectable } from '@angular/core';

import { environment } from 'src/environments/environment';
import { HttpClient, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lecture } from './shared/lecture';
import { Recording } from './shared/recording';

@Injectable({
  providedIn: 'root'
})
export class LectureService {

  constructor(private http: HttpClient) { }

  getLecture(lectureId: string): Observable<Lecture> {
    return this.http.get<Lecture>(environment.apiUrl + '/api/lecture/' + lectureId);
  }

  getLectures(): Observable<Lecture[]> {
    return this.http.get<Lecture[]>(environment.apiUrl + '/api/lecture');
  }

  getLecturesBySemester(semesterId: string): Observable<Lecture[]> {
    return this.http.get<Lecture[]>(environment.apiUrl + '/api/lecture/semester/' + semesterId);
  }

  postLecture(lecture: Lecture): Observable<Lecture> {
    return this.http.post<Lecture>(environment.apiUrl + '/api/lecture', lecture);
  }

  getRecordingsByLecture(lectureId: string): Observable<Recording[]> {
    return this.http.get<Recording[]>(environment.apiUrl + '/api/recording/lecture/' + lectureId);
  }

  postRecording(recording: Recording): Observable<Recording> {
    return this.http.post<Recording>(environment.apiUrl + '/api/recording', recording);
  }

  uploadRecording(recordingId: string, files: any[]) {
    const formData = new FormData();
    files.forEach((file: any) => {
      formData.append('files', file);
    });

    const req = new HttpRequest('POST', environment.apiUrl + '/api/recording/upload/' + recordingId, formData, {
      // reportProgress: true
    });

    return this.http.request(req);
  }
}
