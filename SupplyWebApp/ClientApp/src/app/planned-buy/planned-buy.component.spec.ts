import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlannedBuyComponent } from './planned-buy.component';

describe('PlannedBuyComponent', () => {
  let component: PlannedBuyComponent;
  let fixture: ComponentFixture<PlannedBuyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlannedBuyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlannedBuyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
