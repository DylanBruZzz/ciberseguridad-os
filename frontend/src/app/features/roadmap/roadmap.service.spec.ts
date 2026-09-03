import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/api.config';
import { RoadmapVistaV1 } from './roadmap.models';
import { RoadmapService } from './roadmap.service';

const vista: RoadmapVistaV1 = {
  faseActualId: 'fase-1',
  progresoGlobalPorcentaje: 37,
  totalTemas: 2,
  temasDominados: 1,
  fases: [
    {
      id: 'fase-1',
      orden: 1,
      nombre: 'Fundamentos',
      color: null,
      descripcion: null,
      objetivos: [],
      criteriosAvance: [],
      mesInicioRecomendado: 1,
      mesFinRecomendado: 2,
      cargaSemanalRecomendada: '10 hrs/semana',
      totalTemas: 2,
      temasDominados: 1,
      progresoPorcentaje: 37,
      estaCompletada: false,
      esFaseActual: true,
      temas: [],
    },
  ],
};

describe('RoadmapService', () => {
  let service: RoadmapService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(RoadmapService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('carga RoadmapVistaV1 sin enviar usuarioId', () => {
    service.obtenerVista().subscribe((resultado) => {
      expect(resultado.progresoGlobalPorcentaje).toBe(37);
      expect(resultado.fases[0].progresoPorcentaje).toBe(37);
    });

    const req = http.expectOne('/api/roadmap/vista');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush(vista);
  });

  it('reutiliza la vista en memoria hasta pedir refresco', () => {
    const resultados: RoadmapVistaV1[] = [];

    service.obtenerVista().subscribe((resultado) => resultados.push(resultado));
    service.obtenerVista().subscribe((resultado) => resultados.push(resultado));

    const req = http.expectOne('/api/roadmap/vista');
    req.flush(vista);

    expect(resultados.length).toBe(2);

    service.obtenerVista().subscribe((resultado) => resultados.push(resultado));
    http.expectNone('/api/roadmap/vista');
    expect(resultados.length).toBe(3);

    service.refrescarVista().subscribe((resultado) => resultados.push(resultado));
    http.expectOne('/api/roadmap/vista').flush({ ...vista, progresoGlobalPorcentaje: 44 });
    expect(resultados[3].progresoGlobalPorcentaje).toBe(44);
  });

  it('permite reintentar despues de error', () => {
    const errores: string[] = [];

    service.obtenerVista().subscribe({ error: (error: Error) => errores.push(error.message) });
    http.expectOne('/api/roadmap/vista').flush('Error', { status: 500, statusText: 'Error' });

    expect(errores[0]).toContain('No se pudo cargar la vista del roadmap.');

    service.obtenerVista().subscribe((resultado) => {
      expect(resultado.fases.length).toBe(1);
    });
    http.expectOne('/api/roadmap/vista').flush(vista);
  });
});
