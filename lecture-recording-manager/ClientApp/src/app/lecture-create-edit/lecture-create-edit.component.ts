import { Component, OnInit } from '@angular/core';
import { Lecture } from '../shared/lecture';

import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Semester } from '../shared/semester';
import { SemesterService } from '../semester.service';
import { LectureService } from '../lecture.service';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-lecture-create',
  templateUrl: './lecture-create-edit.component.html',
  styleUrls: ['./lecture-create-edit.component.scss']
})
export class LectureCreateEditComponent implements OnInit {
  form: FormGroup;
  lecture: Lecture;

  semesters: Semester[] = [];
  actionType: string;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private lectureService: LectureService,
    private semesterService: SemesterService) {
    this.actionType = 'add';

    this.form = this.fb.group({
      title: [null, [Validators.required]],
      semesterId: [null, [Validators.required]],
      description: [null],
      publishPath: [null],
      sourcePath: [null],
      publish: [false],
      active: [false]
    });
  }

  ngOnInit(): void {
    const lectureId = this.route.snapshot.paramMap.get('lectureId');
    if (lectureId !== undefined) {
      this.lectureService.getLecture(lectureId).subscribe(x => {
        this.actionType = 'edit';
        this.lecture = x;

        this.form = this.fb.group({
          title: [x.title, [Validators.required]],
          semesterId: [x.semesterId, [Validators.required]],
          description: [x.description],
          publishPath: [x.publishPath],
          sourcePath: [x.sourcePath],
          publish: [x.publish],
          active: [x.active]
        });
      });
    }

    this.semesterService.getSemesters().subscribe(x => this.semesters = x);
  }

  onSubmit() {
    if (this.actionType === 'add') {
      const lecture: Lecture = Object.assign({}, this.form.value);

      this.lectureService.postLecture(lecture).subscribe(x => {
        this.router.navigate(['/', 'lecture', x.id]);
      });
    } else {
      const lecture: Lecture = Object.assign({}, this.form.value);
      lecture.id = this.lecture.id;

      this.lectureService.putLecture(lecture).subscribe(x => {
        this.router.navigate(['/', 'lecture', this.lecture.id]);
      });
    }
  }

  doCancel() {
    if (this.actionType === 'add') {
      this.router.navigate(['/', 'lectures']);
    } else {
      this.router.navigate(['/', 'lecture', this.lecture.id]);
    }
  }
}
