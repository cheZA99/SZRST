import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LokacijeComponent } from './lokacije.component';

describe('UposleniciComponent', () => {
  let component: LokacijeComponent;
  let fixture: ComponentFixture<LokacijeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LokacijeComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LokacijeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
