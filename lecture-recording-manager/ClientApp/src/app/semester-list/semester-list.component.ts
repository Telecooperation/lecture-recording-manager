import { Component, OnInit } from '@angular/core';
import { SemesterService } from '../services/semester.service';
import { Semester } from '../shared/semester';
import { NzNotificationService } from 'ng-zorro-antd/notification';

@Component({
  selector: 'app-semester-list',
  templateUrl: './semester-list.component.html',
  styleUrls: ['./semester-list.component.scss']
})
export class SemesterListComponent implements OnInit {
  public semesters: Semester[] = [];

  public createSemester: Semester = {
    name: '',
    dateStart: new Date(),
    dateEnd: new Date(),
    active: false,
    published: false
  };
  public isCreateModalVisible = false;

  public editSemester: Semester = {
    name: '',
    dateStart: new Date(),
    dateEnd: new Date(),
    active: false,
    published: false
  };
  public isEditModalVisible = false;

  constructor(
    private semesterService: SemesterService,
    private notifications: NzNotificationService) { }

  ngOnInit(): void {
    this.semesterService.getSemesters().subscribe(x => this.semesters = x);
  }

  showCreate(): void {
    this.isCreateModalVisible = true;
  }

  closeCreate(): void {
    this.isCreateModalVisible = false;
  }

  doCreate(): void {
    this.semesterService.postSemester(this.createSemester).subscribe(x => {
      this.isCreateModalVisible = false;
      this.ngOnInit();
    });
  }

  showEdit(semester: Semester): void {
    this.editSemester = { ...semester };
    this.isEditModalVisible = true;
  }

  closeEdit(): void {
    this.isEditModalVisible = false;
  }

  doEdit(): void {
    this.semesterService.putSemester(this.editSemester.id, this.editSemester).subscribe(x => {
      this.isEditModalVisible = false;
      this.ngOnInit();
    });
  }

  doSynchronize(): void {
    this.semesterService.synchronize().subscribe(x => {
      this.notifications.info('Semester synchronized successfully', 'All published semesters are being synchronized.');
    });
  }

}
