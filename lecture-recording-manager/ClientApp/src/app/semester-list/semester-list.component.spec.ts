import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SemesterListComponent } from './semester-list.component';

describe('SemesterListComponent', () => {
  let component: SemesterListComponent;
  let fixture: ComponentFixture<SemesterListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SemesterListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SemesterListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
