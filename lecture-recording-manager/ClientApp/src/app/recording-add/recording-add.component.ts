import { Component, OnInit } from '@angular/core';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { environment } from '../../environments/environment';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { LectureService } from '../services/lecture.service';
import { ActivatedRoute, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { HttpResponse } from '@angular/common/http';
import { Recording } from '../shared/recording';
import { Lecture } from '../shared/lecture';

@Component({
  selector: 'app-recording-add',
  templateUrl: './recording-add.component.html',
  styleUrls: ['./recording-add.component.scss']
})
export class RecordingAddComponent implements OnInit {
  public form: FormGroup;
  public lecture: Lecture;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private notifications: NzNotificationService,
    private lectureService: LectureService
  ) {
    this.form = this.fb.group({
      linkedRecording: [null, [Validators.required]],
      title: [null, [Validators.required]],
      description: [null],
      publishDate: [null]
    });
  }

  ngOnInit(): void {
    const lectureId = this.route.snapshot.paramMap.get('lectureId');
    this.lectureService.getLecture(lectureId).subscribe(x => this.lecture = x);
  }

  onSubmit(): void {
    const lectureId = this.route.snapshot.paramMap.get('lectureId');

    const recording: Recording = Object.assign({}, this.form.value);
    recording.lectureId = +lectureId;

    this.lectureService.postRecording(recording).subscribe(x => {
      this.router.navigate(['/', 'lecture', lectureId]);
    });
  }
}
