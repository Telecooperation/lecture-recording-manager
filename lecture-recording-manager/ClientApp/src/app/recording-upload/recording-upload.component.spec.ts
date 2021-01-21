import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { RecordingUploadComponent } from './recording-upload.component';

describe('RecordingUploadComponent', () => {
  let component: RecordingUploadComponent;
  let fixture: ComponentFixture<RecordingUploadComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RecordingUploadComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RecordingUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
