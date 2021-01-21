import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SemesterListComponent } from './semester-list/semester-list.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { NZ_I18N, en_US } from 'ng-zorro-antd/i18n';

import { LectureListComponent } from './lecture-list/lecture-list.component';
import { LectureCreateEditComponent } from './lecture-create-edit/lecture-create-edit.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LectureComponent } from './lecture/lecture.component';
import { RecordingListComponent } from './recording-list/recording-list.component';
import { RecordingUploadComponent } from './recording-upload/recording-upload.component';
import { DurationPipe } from './pipes/duration.pipe';
import { RecordingComponent } from './recording/recording.component';
import { RecordingEditComponent } from './recording-edit/recording-edit.component';
import { LoginComponent } from './authentication/login/login.component';
import { ErrorInterceptor } from './authentication/auth.interceptor';
import { JwtInterceptor } from './authentication/jwt.interceptor';
import { DragDropModule } from '@angular/cdk/drag-drop';

import { AuthPipe } from './authentication/auth.pipe';
import { UsersComponent } from './authentication/users/users.component';
import { UserAddComponent } from './authentication/user-add/user-add.component';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { NzNotificationModule } from 'ng-zorro-antd/notification';
import { NzIconModule } from 'ng-zorro-antd/icon';


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
    RecordingEditComponent,
    LoginComponent,

    AuthPipe,

    UsersComponent,

    UserAddComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    HttpClientModule,

    FormsModule,
    ReactiveFormsModule,

    NzIconModule,
    NzButtonModule,
    NzUploadModule,
    NzLayoutModule,
    NzPageHeaderModule,
    NzSelectModule,
    NzDropDownModule,
    NzDescriptionsModule,
    NzTableModule,
    NzTagModule,
    NzFormModule,
    NzDatePickerModule,
    NzModalModule,
    NzSwitchModule,
    NzNotificationModule,

    DragDropModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },

    { provide: NZ_I18N, useValue: en_US }],
  bootstrap: [AppComponent]
})
export class AppModule { }
