import { TestBed } from '@angular/core/testing';

import { SessoesEstudoService } from './sessoes-estudo.service';

describe('SessoesEstudoService', () => {
  let service: SessoesEstudoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SessoesEstudoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
