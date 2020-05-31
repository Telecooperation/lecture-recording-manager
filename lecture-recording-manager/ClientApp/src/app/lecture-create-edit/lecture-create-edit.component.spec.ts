import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LectureCreateEditComponent } from './lecture-create-edit.component';

describe('LectureCreateEditComponent', () => {
  let component: LectureCreateEditComponent;
  let fixture: ComponentFixture<LectureCreateEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [LectureCreateEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LectureCreateEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
