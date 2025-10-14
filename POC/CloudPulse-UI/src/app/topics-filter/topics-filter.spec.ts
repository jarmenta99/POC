import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopicsFilter } from './topics-filter';

describe('TopicsFilter', () => {
  let component: TopicsFilter;
  let fixture: ComponentFixture<TopicsFilter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopicsFilter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopicsFilter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
