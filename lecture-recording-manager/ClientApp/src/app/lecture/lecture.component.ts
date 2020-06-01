import { Component, OnInit, ViewChild } from '@angular/core';
import { LectureService } from '../lecture.service';
import { Lecture } from '../shared/lecture';
import { ActivatedRoute } from '@angular/router';
import { Recording } from '../shared/recording';
import { RecordingListComponent } from '../recording-list/recording-list.component';
import { SignalRService } from '../services/signal-r.service';
import { Message } from '../shared/message';

@Component({
  selector: 'app-lecture',
  templateUrl: './lecture.component.html',
  styleUrls: ['./lecture.component.scss']
})
export class LectureComponent implements OnInit {
  public lecture: Lecture;

  @ViewChild(RecordingListComponent)
  private recordingListComponent: RecordingListComponent;

  constructor(
    private lectureService: LectureService,
    private route: ActivatedRoute,
    private signalR: SignalRService) {
    this.subscribeToEvents();
  }

  ngOnInit(): void {
    const lectureId = this.route.snapshot.paramMap.get('lectureId');
    this.lectureService.getLecture(lectureId).subscribe(x => this.lecture = x);
  }

  private subscribeToEvents(): void {
    this.signalR.statusChanged.subscribe((msg: Message) => {
      if (msg.type === 'UPDATE_LECTURE_RECORDING_STATUS') {
        this.recordingListComponent.loadList();
      }
    })
  }

  doProcess(recording: Recording): void {
    this.lectureService.processRecording(recording).subscribe(x => this.recordingListComponent.loadList());
  }

  doPublish(recording: Recording): void {
    this.lectureService.publishRecording(recording).subscribe(x => this.recordingListComponent.loadList());
  }
}
