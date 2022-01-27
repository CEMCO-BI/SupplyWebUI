import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CRUPricingComponent } from './crupricing.component';

describe('CRUPricingComponent', () => {
  let component: CRUPricingComponent;
  let fixture: ComponentFixture<CRUPricingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CRUPricingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CRUPricingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
