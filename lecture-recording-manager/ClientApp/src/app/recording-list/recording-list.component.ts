import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { Lecture } from '../shared/lecture';
import { LectureService } from '../lecture.service';
import { Recording } from '../shared/recording';

@Component({
  selector: 'app-recording-list',
  templateUrl: './recording-list.component.html',
  styleUrls: ['./recording-list.component.scss']
})
export class RecordingListComponent implements OnInit {
  @Output() processClick = new EventEmitter<Recording>();
  @Output() publishClick = new EventEmitter<Recording>();

  private _lectureId: number;
  public recordings: Recording[];

  constructor(
    private lectureService: LectureService) { }

  @Input() set lecture(value: Lecture) {
    this._lectureId = value.id;
    this.loadList();
  }

  ngOnInit(): void {
  }

  loadList(): void {
    this.lectureService.getRecordingsByLecture(this._lectureId.toString()).subscribe(x => this.recordings = x);
  }

  doProcess(recording: Recording): void {
    this.processClick.emit(recording);
  }

  doPublish(recording: Recording): void {
    this.publishClick.emit(recording);
  }
}
