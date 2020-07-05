import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SemesterListComponent } from './semester-list/semester-list.component';
import { HttpClientModule } from '@angular/common/http';

import { NgZorroAntdModule, NZ_I18N, en_US } from 'ng-zorro-antd';
import { LectureListComponent } from './lecture-list/lecture-list.component';
import { LectureCreateEditComponent } from './lecture-create-edit/lecture-create-edit.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LectureComponent } from './lecture/lecture.component';
import { RecordingListComponent } from './recording-list/recording-list.component';
import { RecordingUploadComponent } from './recording-upload/recording-upload.component';
import { DurationPipe } from './pipes/duration.pipe';
import { RecordingComponent } from './recording/recording.component';
import { RecordingEditComponent } from './recording-edit/recording-edit.component';

@NgModule({
  declarations: [
    AppComponent,
    SemesterListComponent,
    LectureListComponent,
    LectureCreateEditComponent,
    LectureComponent,
    RecordingListComponent,
    RecordingUploadComponent,
    DurationPipe,
    RecordingComponent,
    RecordingEditComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    HttpClientModule,

    FormsModule,
    ReactiveFormsModule,

    NgZorroAntdModule
  ],
  providers: [{ provide: NZ_I18N, useValue: en_US }],
  bootstrap: [AppComponent]
})
export class AppModule { }
