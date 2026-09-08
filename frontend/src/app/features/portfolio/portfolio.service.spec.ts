import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/api.config';
import { PortfolioService } from './portfolio.service';

describe('PortfolioService', () => {
  let service: PortfolioService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(PortfolioService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('lista Portafolio read-side con filtros reales y sin usuarioId', () => {
    service
      .listar({ tipoEvidence: 'Writeup', estadoMadurez: 'Publicado' })
      .subscribe((resultado) => {
        expect(resultado.resumen.total).toBe(1);
        expect(resultado.writeups[0].titulo).toBe('Writeup HTB');
      });

    const req = http.expectOne(
      (request) =>
        request.url === '/api/portafolio' &&
        request.params.get('tipoEvidence') === 'Writeup' &&
        request.params.get('estadoMadurez') === 'Publicado',
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush({
      usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
      resumen: {
        total: 1,
        proyectos: 0,
        laboratorios: 0,
        writeups: 1,
        artefactosTecnicos: 0,
        certificacionesObtenidas: 0,
        listosPortafolio: 0,
        publicados: 1,
      },
      proyectos: [],
      laboratorios: [],
      writeups: [
        {
          writeupId: '01a046d5-9bf3-7cec-aa05-10c93459fa17',
          titulo: 'Writeup HTB',
          plataformaOrigen: 'Hack The Box',
          url: 'https://example.com/writeup',
          fecha: '2026-09-02',
          estadoMadurez: 'Publicado',
          temas: [],
        },
      ],
      artefactosTecnicos: [],
      certificacionesObtenidas: [],
    });
  });

  it('no expone metodos de escritura', () => {
    const api = service as unknown as {
      crear?: unknown;
      actualizar?: unknown;
      eliminar?: unknown;
      appendNota?: unknown;
      publish?: unknown;
    };

    expect(api.crear).toBeUndefined();
    expect(api.actualizar).toBeUndefined();
    expect(api.eliminar).toBeUndefined();
    expect(api.appendNota).toBeUndefined();
    expect(api.publish).toBeUndefined();
  });
});
