import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiasEstudosComponent } from '../DiasEstudos/DiasEstudos.component';

describe('DiasEstudosComponent', () => {
  let component: DiasEstudosComponent;
  let fixture: ComponentFixture<DiasEstudosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DiasEstudosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DiasEstudosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
