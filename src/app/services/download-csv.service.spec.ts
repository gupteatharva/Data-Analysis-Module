import { TestBed } from '@angular/core/testing';

import { DownloadCSVService } from './download-csv.service';

describe('DownloadCSVService', () => {
  let service: DownloadCSVService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DownloadCSVService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
