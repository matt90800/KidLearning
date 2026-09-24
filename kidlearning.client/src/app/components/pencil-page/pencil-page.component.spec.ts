import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PencilPageComponent } from './pencil-page.component';

describe('PencilPageComponent', () => {
  let component: PencilPageComponent;
  let fixture: ComponentFixture<PencilPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PencilPageComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PencilPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
