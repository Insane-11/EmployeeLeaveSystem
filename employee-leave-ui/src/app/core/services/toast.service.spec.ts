import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [ToastService] });
    service = TestBed.inject(ToastService);
  });

  it('success adds a success toast', () => {
    service.success('Works!');
    expect(service.messages.length).toBe(1);
    expect(service.messages[0].text).toBe('Works!');
    expect(service.messages[0].type).toBe('success');
  });

  it('error adds an error toast', () => {
    service.error('Failed!');
    const msg = service.messages[0];
    expect(msg.text).toBe('Failed!');
    expect(msg.type).toBe('error');
  });

  it('warning adds a warning toast', () => {
    service.warning('Caution!');
    expect(service.messages[0].type).toBe('warning');
  });

  it('info adds an info toast', () => {
    service.info('FYI');
    expect(service.messages[0].type).toBe('info');
  });

  it('remove deletes a toast', () => {
    service.success('Toast');
    const id = service.messages[0].id;
    service.remove(id);
    expect(service.messages.length).toBe(0);
  });
});
