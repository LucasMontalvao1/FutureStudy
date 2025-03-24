import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MateriaPrincipalComponent } from './materia-principal.component';

describe('MateriaPrincipalComponent', () => {
  let component: MateriaPrincipalComponent;
  let fixture: ComponentFixture<MateriaPrincipalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MateriaPrincipalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MateriaPrincipalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
