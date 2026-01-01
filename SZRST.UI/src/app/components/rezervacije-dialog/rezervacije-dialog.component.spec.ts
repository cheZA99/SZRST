import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentDialogComponent } from './rezervacije-dialog.component';

describe('RezervacijeDialogComponent', () => {
  let component: AppointmentDialogComponent;
  let fixture: ComponentFixture<AppointmentDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppointmentDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
