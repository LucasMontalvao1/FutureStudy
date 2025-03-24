import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TempoEstudoComponent } from './tempo-estudo.component';

describe('TempoEstudoComponent', () => {
  let component: TempoEstudoComponent;
  let fixture: ComponentFixture<TempoEstudoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TempoEstudoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TempoEstudoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
