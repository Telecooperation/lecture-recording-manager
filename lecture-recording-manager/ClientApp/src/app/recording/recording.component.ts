import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Recording } from '../shared/recording';
import { LectureService } from '../services/lecture.service';
import { RecordingChapter } from '../shared/recording-chapter';

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
    private lectureService: LectureService
  ) { }

  ngOnInit(): void {
    const recordingId = this.route.snapshot.paramMap.get('recordingId');
    this.lectureService.getRecording(recordingId).subscribe(x => this.recording = x);
    this.lectureService.getRecordingChapters(recordingId).subscribe(x => this.chapters = x);
  }

  doBack(): void {
    this.router.navigate(['/', 'lecture', this.recording.lectureId]);
  }
}
