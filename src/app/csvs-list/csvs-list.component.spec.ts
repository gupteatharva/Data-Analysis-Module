import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CsvsListComponent } from './csvs-list.component';

describe('CsvsListComponent', () => {
  let component: CsvsListComponent;
  let fixture: ComponentFixture<CsvsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CsvsListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CsvsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
