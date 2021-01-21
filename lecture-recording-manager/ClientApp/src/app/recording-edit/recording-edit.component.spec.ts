import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { RecordingEditComponent } from './recording-edit.component';

describe('RecordingEditComponent', () => {
  let component: RecordingEditComponent;
  let fixture: ComponentFixture<RecordingEditComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RecordingEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RecordingEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
