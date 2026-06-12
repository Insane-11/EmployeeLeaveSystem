import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { LoadingService } from './loading.service';

describe('LoadingService', () => {
  let service: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [LoadingService] });
    service = TestBed.inject(LoadingService);
  });

  it('starts with loading false', () => {
    expect(service.loading).toBe(false);
  });

  it('show sets loading to true', () => {
    service.show();
    expect(service.loading).toBe(true);
  });

  it('hide sets loading to false', () => {
    service.show();
    service.hide();
    expect(service.loading).toBe(false);
  });

  it('multiple shows require same number of hides', () => {
    service.show();
    service.show();
    service.show();
    expect(service.loading).toBe(true);
    service.hide();
    expect(service.loading).toBe(true);
    service.hide();
    expect(service.loading).toBe(true);
    service.hide();
    expect(service.loading).toBe(false);
  });

  it('hide does not go negative', () => {
    service.hide();
    service.hide();
    expect(service.loading).toBe(false);
    service.show();
    expect(service.loading).toBe(true);
    service.hide();
    expect(service.loading).toBe(false);
  });

  it('loading$ observable emits values', async () => {
    const values: boolean[] = [];
    const loading$ = service.loading$;
    loading$.subscribe(v => values.push(v));
    service.show();
    service.hide();
    await new Promise(r => setTimeout(r, 10));
    expect(values).toEqual([false, true, false]);
  });
});
