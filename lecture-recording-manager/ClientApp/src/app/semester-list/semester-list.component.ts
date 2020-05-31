import { Component, OnInit } from '@angular/core';
import { SemesterService } from '../semester.service';
import { Semester } from '../shared/semester';

@Component({
  selector: 'app-semester-list',
  templateUrl: './semester-list.component.html',
  styleUrls: ['./semester-list.component.scss']
})
export class SemesterListComponent implements OnInit {
  public semesters: Semester[] = [];

  constructor(private semesterService: SemesterService) { }

  ngOnInit(): void {
    this.semesterService.getSemesters().subscribe(x => this.semesters = x);
  }

}
