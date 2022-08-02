import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ExternalLinksComponent } from './external-links.component';

describe('ExternalLinksComponent', () => {
  let component: ExternalLinksComponent;
  let fixture: ComponentFixture<ExternalLinksComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ExternalLinksComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExternalLinksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
