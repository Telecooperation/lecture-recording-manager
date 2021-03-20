import { Component, EventEmitter, forwardRef, Input, OnInit, Output } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Observable } from 'rxjs';
import { LectureService } from 'src/app/services/lecture.service';
import { SemesterService } from 'src/app/services/semester.service';
import { Lecture } from 'src/app/shared/lecture';
import { Recording } from 'src/app/shared/recording';
import { Semester } from 'src/app/shared/semester';

@Component({
  selector: 'app-select-recording',
  templateUrl: './select-recording.component.html',
  styleUrls: ['./select-recording.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectRecordingComponent),
      multi: true
    }
  ]
})
export class SelectRecordingComponent implements OnInit, ControlValueAccessor {
  propagateChange = (_: any) => { };

  public semesters: Observable<Semester[]>;
  public lectures: Observable<Lecture[]>;
  public recordings: Observable<Recording[]>;

  public selectedSemester: number;
  public selectedLecture: number;
  public recording: Recording;

  constructor(
    private semesterService: SemesterService,
    private lectureService: LectureService
  ) {
    this.semesters = this.semesterService.getSemesters();
  }

  ngOnInit(): void {

  }

  updateLectures(value: number): void {
    this.lectures = this.lectureService.getLecturesBySemester(this.selectedSemester.toString());
    this.selectedLecture = null;
    this.recording = null;
  }

  updateRecordings(value: number): void {
    this.recordings = this.lectureService.getRecordingsByLecture(this.selectedLecture.toString(), true);
    this.recording = null;
  }

  updateRecording(value: number): void {
    this.propagateChange(this.recording.id);
  }

  writeValue(value: any) {
    if (value !== undefined && value !== null) {
      this.lectureService.getRecording(value).subscribe(x => this.recording = x);
    }
  }

  registerOnChange(fn) {
    this.propagateChange = fn;
  }

  registerOnTouched() { }

}
