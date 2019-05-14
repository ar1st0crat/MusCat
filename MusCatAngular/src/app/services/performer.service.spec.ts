import { TestBed } from '@angular/core/testing';

import { PerformerService } from './performer.service';

describe('PerformerService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: PerformerService = TestBed.get(PerformerService);
    expect(service).toBeTruthy();
  });
});
