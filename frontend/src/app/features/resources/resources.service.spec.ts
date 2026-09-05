import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/api.config';
import {
  CreateRecursoRequest,
  RecursoDetalle,
  RecursoResumen,
  UpdateRecursoRequest,
} from './resources.models';
import { ResourcesService } from './resources.service';

const temaId = '01a046d5-9bf3-7cec-aa05-10c93459fa17';
const recursoId = '01a046d5-9bf3-7cec-aa05-10c93459fa18';

const recurso: RecursoResumen = {
  id: recursoId,
  usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
  tipo: 'Documentacion',
  titulo: 'Documentacion modelo OSI',
  url: 'https://example.com/osi',
  estado: 'PorClasificar',
  temas: [{ id: temaId, nombre: 'Modelo OSI / TCP-IP' }],
};

const detalle: RecursoDetalle = {
  ...recurso,
  rating: 4,
  notas: 'Usar como referencia base.',
  herramientaIA: 'ChatGPT',
  promptsUtilizados: 'Resume el recurso.',
};

describe('ResourcesService', () => {
  let service: ResourcesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(ResourcesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('lista recursos globales sin enviar usuarioId', () => {
    service.listar().subscribe((resultado) => {
      expect(resultado).toEqual([recurso]);
    });

    const req = http.expectOne('/api/recursos');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    expect(req.request.params.has('temaId')).toBeFalsy();
    req.flush([recurso]);
  });

  it('lista recursos contextuales con temaId factual y sin usuarioId', () => {
    service.listar(temaId).subscribe((resultado) => {
      expect(resultado).toEqual([recurso]);
    });

    const req = http.expectOne((request) => request.url === '/api/recursos');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('temaId')).toBe(temaId);
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush([recurso]);
  });

  it('obtiene detalle', () => {
    service.obtenerDetalle(recursoId).subscribe((resultado) => {
      expect(resultado).toEqual(detalle);
    });

    const req = http.expectOne(`/api/recursos/${recursoId}`);
    expect(req.request.method).toBe('GET');
    req.flush(detalle);
  });

  it('crea recurso con payload exacto y sin usuarioId', () => {
    const payload: CreateRecursoRequest = {
      tipo: 'Documentacion',
      titulo: 'Prueba Resources',
      url: 'https://example.com/resources-v1',
    };

    service.crear(payload).subscribe((resultado) => {
      expect(resultado.id).toBe(recursoId);
      expect(resultado.estado).toBe('PorClasificar');
    });

    const req = http.expectOne('/api/recursos');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush({ ...recurso, temas: undefined }, { status: 201, statusText: 'Created' });
  });

  it('actualiza recurso con payload exacto y sin usuarioId', () => {
    const payload: UpdateRecursoRequest = {
      titulo: 'Documentacion editada',
      url: 'https://example.com/editado',
      estado: 'EnUso',
      rating: 5,
      notas: 'Notas editadas',
      herramientaIA: 'ChatGPT',
      promptsUtilizados: 'Prompt editado',
    };

    service.actualizar(recursoId, payload).subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/recursos/${recursoId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('elimina recurso sin enviar usuarioId', () => {
    service.eliminar(recursoId).subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/recursos/${recursoId}`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('vincula recurso a tema sin usuarioId', () => {
    service.vincularTema(recursoId, temaId).subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/recursos/${recursoId}/temas/${temaId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('traduce errores de listado', () => {
    const errores: string[] = [];

    service.listar().subscribe({ error: (error: Error) => errores.push(error.message) });

    http.expectOne('/api/recursos').flush('Error', { status: 500, statusText: 'Error' });

    expect(errores[0]).toContain('No pudimos cargar los recursos.');
  });
});
