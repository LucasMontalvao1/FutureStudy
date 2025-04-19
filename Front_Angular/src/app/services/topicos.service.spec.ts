import { TestBed } from '@angular/core/testing';

import { TopicosService } from './topicos.service';

describe('TopicosService', () => {
  let service: TopicosService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TopicosService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
