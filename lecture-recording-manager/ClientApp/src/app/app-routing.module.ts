import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SemesterListComponent } from './semester-list/semester-list.component';
import { LectureListComponent } from './lecture-list/lecture-list.component';
import { LectureCreateEditComponent } from './lecture-create-edit/lecture-create-edit.component';
import { RecordingUploadComponent } from './recording-upload/recording-upload.component';
import { LectureComponent } from './lecture/lecture.component';
import { RecordingComponent } from './recording/recording.component';
import { RecordingEditComponent } from './recording-edit/recording-edit.component';
import { LoginComponent } from './authentication/login/login.component';
import { AuthGuard } from './authentication/auth.guard';
import { UsersComponent } from './authentication/users/users.component';
import { UserAddComponent } from './authentication/user-add/user-add.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },

  { path: '', component: SemesterListComponent, canActivate: [AuthGuard] },
  { path: 'semesters', component: SemesterListComponent, canActivate: [AuthGuard] },

  { path: 'lectures', component: LectureListComponent, canActivate: [AuthGuard] },
  { path: 'lectures/semester/:semesterId', component: LectureListComponent, canActivate: [AuthGuard] },

  { path: 'lectures/new', component: LectureCreateEditComponent, canActivate: [AuthGuard] },
  { path: 'lecture/edit/:lectureId', component: LectureCreateEditComponent, canActivate: [AuthGuard] },

  { path: 'lecture/:lectureId', component: LectureComponent, canActivate: [AuthGuard] },
  { path: 'lecture/:lectureId/upload', component: RecordingUploadComponent, canActivate: [AuthGuard] },

  { path: 'recording/:recordingId', component: RecordingComponent, canActivate: [AuthGuard] },
  { path: 'recording/:recordingId/edit', component: RecordingEditComponent, canActivate: [AuthGuard] },

  { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
  { path: 'users/add', component: UserAddComponent, canActivate: [AuthGuard] },

  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
