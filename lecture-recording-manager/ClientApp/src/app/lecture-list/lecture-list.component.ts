import { Component, OnInit } from '@angular/core';
import { Lecture } from '../shared/lecture';
import { LectureService } from '../services/lecture.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-lecture-list',
  templateUrl: './lecture-list.component.html',
  styleUrls: ['./lecture-list.component.scss']
})
export class LectureListComponent implements OnInit {
  public lectures: Lecture[] = []

  constructor(
    private lectureService: LectureService,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    const semesterId = this.route.snapshot.paramMap.get('semesterId');

    if (semesterId === null) {
      this.lectureService.getLectures().subscribe(x => this.lectures = x);
    } else {
      this.lectureService.getLecturesBySemester(semesterId).subscribe(x => this.lectures = x);
    }
  }

}
