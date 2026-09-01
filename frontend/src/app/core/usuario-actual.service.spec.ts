import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from './api.config';
import { UsuarioActualService } from './usuario-actual.service';

describe('UsuarioActualService', () => {
  let service: UsuarioActualService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(UsuarioActualService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('obtiene el usuario actual desde la API Personal', () => {
    service.obtener().subscribe((usuario) => {
      expect(usuario).toEqual({ id: 'usuario-1', nombre: 'Dylan' });
    });

    const req = http.expectOne('/api/usuario-actual');
    expect(req.request.method).toBe('GET');
    req.flush({ id: 'usuario-1', nombre: 'Dylan' });
  });

  it('traduce error de API no disponible', () => {
    service.obtener().subscribe({
      next: () => {
        throw new Error('No debe resolver exitosamente');
      },
      error: (error: Error) => {
        expect(error.message).toContain('API local no esta disponible');
      },
    });

    const req = http.expectOne('/api/usuario-actual');
    req.error(new ProgressEvent('error'));
  });
});
