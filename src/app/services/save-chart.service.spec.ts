import { TestBed } from '@angular/core/testing';

import { SaveChartService } from './save-chart.service';

describe('SaveChartService', () => {
  let service: SaveChartService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SaveChartService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
