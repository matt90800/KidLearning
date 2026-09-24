import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LetterGameComponent } from './letter-game.component';

describe('LetterGameComponent', () => {
  let component: LetterGameComponent;
  let fixture: ComponentFixture<LetterGameComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [LetterGameComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LetterGameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
