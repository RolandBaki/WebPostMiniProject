import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BlogpostForm } from './blogpost-form';

describe('BlogpostForm', () => {
  let component: BlogpostForm;
  let fixture: ComponentFixture<BlogpostForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BlogpostForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BlogpostForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
