import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrganizacijeComponent } from './organizacije.component';

describe('OrganizacijeComponent', () => {
  let component: OrganizacijeComponent;
  let fixture: ComponentFixture<OrganizacijeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrganizacijeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OrganizacijeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
