import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { Lecture } from '../shared/lecture';
import { LectureService } from '../services/lecture.service';
import { Recording } from '../shared/recording';

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

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

  drop(event: CdkDragDrop<string[]>): void {
    moveItemInArray(this.recordings, event.previousIndex, event.currentIndex);
    this.lectureService.sortRecordings(this._lectureId, this.recordings.map(x => x.id)).subscribe(x => this.recordings = x);
  }
}
