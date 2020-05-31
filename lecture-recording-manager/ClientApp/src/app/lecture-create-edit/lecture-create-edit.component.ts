import { Component, OnInit } from '@angular/core';
import { Lecture } from '../shared/lecture';

import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Semester } from '../shared/semester';
import { SemesterService } from '../semester.service';
import { LectureService } from '../lecture.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-lecture-create',
  templateUrl: './lecture-create-edit.component.html',
  styleUrls: ['./lecture-create-edit.component.scss']
})
export class LectureCreateEditComponent implements OnInit {
  form: FormGroup;

  semesters: Semester[] = [];
  actionType: string;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private lectureService: LectureService,
    private semesterService: SemesterService) {
    this.actionType = 'add';

    this.form = this.fb.group({
      title: [null, [Validators.required]],
      semesterId: [null, [Validators.required]],
      description: [null],
      publish: [false],
      active: [false]
    });
  }

  ngOnInit(): void {
    this.semesterService.getSemesters().subscribe(x => this.semesters = x);
  }

  onSubmit() {
    if (this.actionType === 'add') {
      const lecture: Lecture = Object.assign({}, this.form.value);

      this.lectureService.postLecture(lecture).subscribe(x => {
        this.router.navigate(['/', 'lecture', x.id]);
      });
    }
  }
}
