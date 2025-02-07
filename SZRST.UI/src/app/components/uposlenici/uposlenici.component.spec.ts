import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UposleniciComponent } from './uposlenici.component';

describe('UposleniciComponent', () => {
  let component: UposleniciComponent;
  let fixture: ComponentFixture<UposleniciComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UposleniciComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UposleniciComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
