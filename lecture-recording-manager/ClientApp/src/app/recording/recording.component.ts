import { Component, ElementRef, OnInit, TemplateRef, ViewChild, ViewContainerRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Recording } from '../shared/recording';
import { LectureService } from '../services/lecture.service';
import { RecordingChapter } from '../shared/recording-chapter';
import { NzModalService, NzNotificationService } from 'ng-zorro-antd';
import { AuthenticationService } from '../services/authentication.service';
import { SignalRService } from '../services/signal-r.service';
import { Message } from '../shared/message';

@Component({
  selector: 'app-recording',
  templateUrl: './recording.component.html',
  styleUrls: ['./recording.component.scss']
})
export class RecordingComponent implements OnInit {
  public recording: Recording;
  public chapters: RecordingChapter[];

  public token: string;

  @ViewChild('videoPlayer', { static: false })
  public videoPlayerRef: TemplateRef<void>;
  public outputIdPlayer: number;

  constructor(
    private modal: NzModalService, 
    private route: ActivatedRoute,
    private router: Router,
    private signalR: SignalRService,
    private authenticationService: AuthenticationService,
    private notifications: NzNotificationService,
    private lectureService: LectureService
  ) {
    this.subscribeToEvents();
  }

  ngOnInit(): void {
    const recordingId = this.route.snapshot.paramMap.get('recordingId');
    this.loadRecording(recordingId);
    this.token = this.authenticationService.currentUserValue.token;
  }

  private subscribeToEvents(): void {
    this.signalR.statusChanged.subscribe((msg: Message) => {
      if (!this.recording) {
        return;
      }

      if (msg.type === 'UPDATE_LECTURE_RECORDING_STATUS') {
        this.loadRecording(this.recording.id.toString());
      }

      if (msg.type === 'UPDATE_LECTURE') {
        this.loadRecording(this.recording.id.toString());
      }
    });
  }

  loadRecording(recordingId: string): void {
    this.lectureService.getRecording(recordingId).subscribe(x => this.recording = x);
    this.lectureService.getRecordingChapters(recordingId).subscribe(x => this.chapters = x);
  }

  hasPreview(): boolean {
    return this.recording?.outputs
      .filter(x => x.jobType === 'LectureRecordingManager.Jobs.PreviewRecordingJob')
      .filter(x => x.status === 2)
      .length > 0;
  }

  getRecordingJobType(type: string): string {
    const obj = JSON.parse(type);

    if (obj.OutputType === 1) {
      return '720p Video Processing';
    } else if (obj.OutputType === 2) {
      return '1080p Video Processing';
    }
  }

  doBack(): void {
    this.router.navigate(['/', 'lecture', this.recording.lectureId]);
  }

  doDelete(): void {
    this.lectureService.deleteRecording(this.recording).subscribe(x => {
      this.notifications.success(
        'Recording deleted successfully',
        'The recording <b>' + this.recording.title + '</b> deleted successfully.',
        { nzPlacement: 'bottomRight'});
      this.router.navigate(['/', 'lecture', this.recording.lectureId]);
    });
  }

  doProcess(): void {
    this.lectureService.processRecording(this.recording).subscribe(x => this.loadRecording(x.id.toString()));
  }

  doPublish(): void {
    this.lectureService.publishRecording(this.recording).subscribe(x => this.loadRecording(x.id.toString()));
  }

  doPreview(): void {
    this.notifications.info(
      'Recording preview started',
      'The recording preview of <b>' + this.recording.title + '</b> will be created.',
      { nzPlacement: 'bottomRight'});

    this.lectureService.processPreview(this.recording).subscribe(x => this.loadRecording(x.id.toString()));
  }

  doOutputPreview(outputId: number):void {
    this.outputIdPlayer = outputId;

    this.modal.create({
      nzWidth: 1328,
      nzContent: this.videoPlayerRef
    });
  }
}
