import { Component, OnInit, Input } from '@angular/core';
import { Lecture } from '../shared/lecture';
import { LectureService } from '../lecture.service';
import { Recording } from '../shared/recording';

@Component({
  selector: 'app-recording-list',
  templateUrl: './recording-list.component.html',
  styleUrls: ['./recording-list.component.scss']
})
export class RecordingListComponent implements OnInit {
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

}
