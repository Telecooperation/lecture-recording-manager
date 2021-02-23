import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RecordingAddComponent } from './recording-add.component';

describe('RecordingAddComponent', () => {
  let component: RecordingAddComponent;
  let fixture: ComponentFixture<RecordingAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RecordingAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RecordingAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
