import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/api.config';
import { RoadmapService } from './roadmap.service';

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

  it('carga fases sin enviar usuarioId', () => {
    service.listarFases().subscribe((fases) => {
      expect(fases.length).toBe(1);
      expect(fases[0].nombre).toBe('Fundamentos');
    });

    const req = http.expectOne('/api/fases');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush([
      {
        id: 'fase-1',
        nombre: 'Fundamentos',
        orden: 1,
        color: null,
        descripcion: null,
        objetivos: [],
        criteriosAvance: [],
        mesInicioRecomendado: 1,
        mesFinRecomendado: 2,
        cargaSemanalRecomendada: '10 hrs/semana',
      },
    ]);
  });

  it('carga temas sin enviar usuarioId', () => {
    service.listarTemas().subscribe((temas) => {
      expect(temas.length).toBe(1);
      expect(temas[0].nombre).toBe('Modelo OSI / TCP-IP');
    });

    const req = http.expectOne('/api/temas');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush([
      {
        id: 'tema-1',
        faseId: 'fase-1',
        temaPadreId: null,
        nombre: 'Modelo OSI / TCP-IP',
        tipoConocimiento: 'Teorico',
        descripcion: null,
      },
    ]);
  });
});
