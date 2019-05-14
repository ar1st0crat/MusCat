import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PerformersPanelComponent } from './performers-panel.component';

describe('PerformersPanelComponent', () => {
  let component: PerformersPanelComponent;
  let fixture: ComponentFixture<PerformersPanelComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PerformersPanelComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PerformersPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
