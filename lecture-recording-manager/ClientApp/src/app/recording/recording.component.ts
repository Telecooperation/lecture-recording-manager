import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Recording } from '../shared/recording';
import { LectureService } from '../services/lecture.service';
import { RecordingChapter } from '../shared/recording-chapter';
import { NzNotificationService } from 'ng-zorro-antd';

@Component({
  selector: 'app-recording',
  templateUrl: './recording.component.html',
  styleUrls: ['./recording.component.scss']
})
export class RecordingComponent implements OnInit {
  public recording: Recording;
  public chapters: RecordingChapter[];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private notifications: NzNotificationService,
    private lectureService: LectureService
  ) { }

  ngOnInit(): void {
    const recordingId = this.route.snapshot.paramMap.get('recordingId');
    this.loadRecording(recordingId);
  }

  loadRecording(recordingId: string): void {
    this.lectureService.getRecording(recordingId).subscribe(x => this.recording = x);
    this.lectureService.getRecordingChapters(recordingId).subscribe(x => this.chapters = x);
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
}
