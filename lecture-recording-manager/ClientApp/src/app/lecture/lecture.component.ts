import { Component, OnInit, ViewChild } from '@angular/core';
import { LectureService } from '../lecture.service';
import { Lecture } from '../shared/lecture';
import { ActivatedRoute, Router } from '@angular/router';
import { Recording } from '../shared/recording';
import { RecordingListComponent } from '../recording-list/recording-list.component';
import { SignalRService } from '../services/signal-r.service';
import { Message } from '../shared/message';
import { NzNotificationService } from 'ng-zorro-antd/notification';

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
    private router: Router,
    private lectureService: LectureService,
    private route: ActivatedRoute,
    private signalR: SignalRService,
    private notification: NzNotificationService) {
    this.subscribeToEvents();
  }

  ngOnInit(): void {
    const lectureId = this.route.snapshot.paramMap.get('lectureId');
    this.loadLecture(lectureId);
  }

  private loadLecture(lectureId: string) {
    this.lectureService.getLecture(lectureId).subscribe(x => this.lecture = x);
  }

  private subscribeToEvents(): void {
    this.signalR.statusChanged.subscribe((msg: Message) => {
      if (msg.type === 'UPDATE_LECTURE_RECORDING_STATUS') {
        this.recordingListComponent.loadList();
      }

      if (msg.type === 'UPDATE_LECTURE') {
        this.loadLecture(this.lecture.id.toString());
      }
    })
  }

  doProcess(recording: Recording): void {
    this.lectureService.processRecording(recording).subscribe(x => this.recordingListComponent.loadList());
  }

  doPublish(recording: Recording): void {
    this.lectureService.publishRecording(recording).subscribe(x => this.recordingListComponent.loadList());
  }

  doSynchronize(): void {
    this.notification.success(
      'Synchronization started', 
      'The synchronization of the lecture <b>' + this.lecture.title + '</b> started.',
      { nzPlacement: 'bottomRight'});

    this.lectureService.synchronizeLecture(this.lecture).subscribe();
  }

  doDelete(): void {
    this.lectureService.deleteLecture(this.lecture).subscribe(x => {
      this.router.navigate(['/', 'lectures']);
      this.notification.success(
        'Lecture deleted successfully', 
        'The lecture <b>' + this.lecture.title + '</b> was deleted successfully.',
        { nzPlacement: 'bottomRight'});
    }, () => {
      this.notification.error(
        'Lecture could not be deleted', 
        'The lecture cannot be deleted because it contains recordings.',
        { nzPlacement: 'bottomRight'});
    });
  }

  doBack(): void {
    this.router.navigate(['/', 'lectures']);
  }
}
