import { Component, OnInit } from '@angular/core';
import { UploadFile } from 'ng-zorro-antd';
import { environment } from '../../environments/environment';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { LectureService } from '../lecture.service';
import { ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'app-recording-upload',
  templateUrl: './recording-upload.component.html',
  styleUrls: ['./recording-upload.component.scss']
})
export class RecordingUploadComponent implements OnInit {
  public fileList: UploadFile[] = []
  public uploadUrl: string = environment.apiUrl + '/api/recording/upload';

  public form: FormGroup;
  public uploading = false;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private lectureService: LectureService) {
    this.form = this.fb.group({
      title: [null, [Validators.required]],
      description: [null],
      publishDate: [null]
    });
  }

  ngOnInit(): void {
  }

  onSubmit(): void {
    this.lectureService.uploadRecording('2', this.fileList)
      .pipe(filter(e => e instanceof HttpResponse))
      .subscribe(
        () => {
          this.uploading = false;
          this.fileList = [];
          //this.msg.success('upload successfully.');
        },
        () => {
          this.uploading = false;
          //this.msg.error('upload failed.');
        }
      );
  }

  beforeUpload = (file: UploadFile): boolean => {
    this.fileList = this.fileList.concat(file);
    return false;
  };
}
