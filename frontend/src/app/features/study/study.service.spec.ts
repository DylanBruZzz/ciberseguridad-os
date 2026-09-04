import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/api.config';
import {
  ActualizarSesionEstudioRequest,
  RegistrarSesionEstudioRequest,
  SesionEstudioResumen,
} from './study.models';
import { StudyService } from './study.service';

const sesion: SesionEstudioResumen = {
  id: '01a046d5-9bf3-7cec-aa05-10c93459fa18',
  usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
  temaId: '01a046d5-9bf3-7cec-aa05-10c93459fa17',
  fecha: '2026-09-04',
  duracionMinutos: 45,
  tipo: 'Teoria',
  notas: 'Prueba Study V1',
};

describe('StudyService', () => {
  let service: StudyService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(StudyService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('lista sesiones sin enviar usuarioId', () => {
    service.listarSesiones().subscribe((resultado) => {
      expect(resultado).toEqual([sesion]);
    });

    const req = http.expectOne('/api/sesiones-estudio');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush([sesion]);
  });

  it('registra sesion con payload exacto y sin usuarioId', () => {
    const payload: RegistrarSesionEstudioRequest = {
      temaId: sesion.temaId,
      fecha: sesion.fecha,
      duracionMinutos: sesion.duracionMinutos,
      tipo: 'Practica',
      notas: 'Practica guiada',
    };

    service.registrarSesion(payload).subscribe((resultado) => {
      expect(resultado.estado).toBe('Creada');
      expect(resultado.id).toBe(sesion.id);
    });

    const req = http.expectOne('/api/sesiones-estudio');
    expect(req.request.method).toBe('POST');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    expect(req.request.body).toEqual(payload);
    req.flush({ estado: 'Creada', ...sesion, tipo: 'Practica', notas: 'Practica guiada' });
  });

  it('actualiza sesion con payload exacto y sin usuarioId', () => {
    const payload: ActualizarSesionEstudioRequest = {
      temaId: sesion.temaId,
      fecha: '2026-09-03',
      duracionMinutos: 90,
      tipo: 'Laboratorio',
      notas: 'Laboratorio corregido',
    };

    service.actualizarSesion(sesion.id, payload).subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/sesiones-estudio/${sesion.id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    expect(req.request.body).toEqual(payload);
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('elimina sesion sin enviar usuarioId', () => {
    service.eliminarSesion(sesion.id).subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/sesiones-estudio/${sesion.id}`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('traduce errores de listado', () => {
    const errores: string[] = [];

    service.listarSesiones().subscribe({ error: (error: Error) => errores.push(error.message) });

    http.expectOne('/api/sesiones-estudio').flush('Error', { status: 500, statusText: 'Error' });

    expect(errores[0]).toContain('No pudimos cargar las sesiones de estudio.');
  });
});
