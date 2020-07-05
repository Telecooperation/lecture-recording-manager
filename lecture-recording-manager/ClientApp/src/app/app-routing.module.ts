import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SemesterListComponent } from './semester-list/semester-list.component';
import { LectureListComponent } from './lecture-list/lecture-list.component';
import { LectureCreateEditComponent } from './lecture-create-edit/lecture-create-edit.component';
import { RecordingUploadComponent } from './recording-upload/recording-upload.component';
import { LectureComponent } from './lecture/lecture.component';
import { RecordingComponent } from './recording/recording.component';
import { RecordingEditComponent } from './recording-edit/recording-edit.component';


const routes: Routes = [
  { path: '', component: SemesterListComponent },
  { path: 'semesters', component: SemesterListComponent },

  { path: 'lectures', component: LectureListComponent },
  { path: 'lectures/semester/:semesterId', component: LectureListComponent },
  
  { path: 'lectures/new', component: LectureCreateEditComponent },
  { path: 'lecture/edit/:lectureId', component: LectureCreateEditComponent },

  { path: 'lecture/:lectureId', component: LectureComponent },
  { path: 'lecture/:lectureId/upload', component: RecordingUploadComponent },

  { path: 'recording/:recordingId', component: RecordingComponent },
  { path: 'recording/:recordingId/edit', component: RecordingEditComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
