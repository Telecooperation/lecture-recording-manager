import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RecordingListComponent } from './recording-list.component';

describe('RecordingListComponent', () => {
  let component: RecordingListComponent;
  let fixture: ComponentFixture<RecordingListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RecordingListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RecordingListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
