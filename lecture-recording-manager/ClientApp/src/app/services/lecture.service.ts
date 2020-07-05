import { Injectable } from '@angular/core';

import { environment } from 'src/environments/environment';
import { HttpClient, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lecture } from '../shared/lecture';
import { Recording } from '../shared/recording';
import { RecordingChapter } from '../shared/recording-chapter';

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

  putLecture(lecture: Lecture): Observable<Lecture> {
    return this.http.put<Lecture>(environment.apiUrl + '/api/lecture/' + lecture.id, lecture);
  }

  synchronizeLecture(lecture: Lecture) {
    return this.http.get(environment.apiUrl + '/api/lecture/synchronize/' + lecture.id);
  }

  deleteLecture(lecture: Lecture) {
    return this.http.delete(environment.apiUrl + '/api/lecture/' + lecture.id);
  }

  getRecordingsByLecture(lectureId: string): Observable<Recording[]> {
    return this.http.get<Recording[]>(environment.apiUrl + '/api/recording/lecture/' + lectureId);
  }

  getRecording(recordingId: string): Observable<Recording> {
    return this.http.get<Recording>(environment.apiUrl + '/api/recording/' + recordingId);
  }

  postRecording(recording: Recording): Observable<Recording> {
    return this.http.post<Recording>(environment.apiUrl + '/api/recording', recording);
  }

  putRecording(recording: Recording): Observable<Recording> {
    return this.http.put<Recording>(environment.apiUrl + '/api/recording/' + recording.id, recording);
  }

  deleteRecording(recording: Recording) {
    return this.http.delete(environment.apiUrl + '/api/recording/' + recording.id);
  }

  uploadRecording(recordingId: string, process: boolean, files: any[]) {
    const formData = new FormData();
    formData.append('process', process ? 'true' : 'false');
    files.forEach((file: any) => {
      formData.append('files', file);
    });

    const req = new HttpRequest('POST', environment.apiUrl + '/api/recording/upload/' + recordingId, formData, {
      // reportProgress: true
    });

    return this.http.request(req);
  }

  processRecording(recording: Recording): Observable<Recording> {
    return this.http.get<Recording>(environment.apiUrl + '/api/recording/process/' + recording.id);
  }

  getRecordingChapters(recordingId: string): Observable<RecordingChapter[]> {
    return this.http.get<RecordingChapter[]>(environment.apiUrl + '/api/recording/' + recordingId + '/chapters');
  }

  publishRecording(recording: Recording): Observable<Recording> {
    return this.http.get<Recording>(environment.apiUrl + '/api/recording/publish/' + recording.id);
  }
}
