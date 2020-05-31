import { Component, OnInit } from '@angular/core';
import { LectureService } from '../lecture.service';
import { Lecture } from '../shared/lecture';
import { ActivatedRoute } from '@angular/router';
import { Recording } from '../shared/recording';

@Component({
  selector: 'app-lecture',
  templateUrl: './lecture.component.html',
  styleUrls: ['./lecture.component.scss']
})
export class LectureComponent implements OnInit {
  public lecture: Lecture;

  constructor(
    private lectureService: LectureService,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    const lectureId = this.route.snapshot.paramMap.get('lectureId');
    this.lectureService.getLecture(lectureId).subscribe(x => this.lecture = x);
  }

  doProcess(recording: Recording): void {
    this.lectureService.processRecording(recording).subscribe();
  }
}
