import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UserService } from './user.service';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserService]
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getAll GETs /api/users', () => {
    service.getAll().subscribe();
    const req = httpMock.expectOne('/api/users');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('getById GETs /api/users/{id}', () => {
    service.getById(7).subscribe();
    const req = httpMock.expectOne('/api/users/7');
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('update PUTs /api/users/{id}', () => {
    const update = { firstName: 'Updated' };
    service.update(7, update).subscribe();
    const req = httpMock.expectOne('/api/users/7');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(update);
    req.flush({});
  });

  it('delete DELETEs /api/users/{id}', () => {
    service.delete(7).subscribe();
    const req = httpMock.expectOne('/api/users/7');
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('getTeam GETs /api/users/team', () => {
    service.getTeam().subscribe();
    const req = httpMock.expectOne('/api/users/team');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
