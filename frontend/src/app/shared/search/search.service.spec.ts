import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { RoadmapVistaV1 } from '../../features/roadmap/roadmap.models';
import { RoadmapService } from '../../features/roadmap/roadmap.service';
import { RecursoResumen } from '../../features/resources/resources.models';
import { EvidenceItemV1, TIPOS_EVIDENCE } from '../../features/evidence/evidence.models';
import { SearchService } from './search.service';

const vista: RoadmapVistaV1 = {
  faseActualId: 'fase-1', progresoGlobalPorcentaje: 25, totalTemas: 1, temasDominados: 0,
  fases: [{ id: 'fase-1', orden: 1, nombre: 'Fundamentos', color: null, descripcion: null,
    objetivos: [], criteriosAvance: [], mesInicioRecomendado: null, mesFinRecomendado: null,
    cargaSemanalRecomendada: null, totalTemas: 1, temasDominados: 0, progresoPorcentaje: 25,
    estaCompletada: false, esFaseActual: true, temas: [{ id: 'tema-1', faseId: 'fase-1', temaPadreId: null,
      nombre: 'Bash scripting', descripcion: null, tipoConocimiento: 'Practico', dificultadPercibida: null,
      confianza: null, intervaloRepasoDias: 14, estado: 'EnProgreso', criteriosTotal: 4, criteriosCumplidos: 1,
      progresoPorcentaje: 25, ultimaSesion: null, proximaFechaRepaso: null, repasoRecomendado: false }],
  }],
};
const recursos: RecursoResumen[] = [{ id: 'recurso-1', usuarioId: 'no-indexar', titulo: 'Guia de redes',
  tipo: 'Documentacion', estado: 'EnUso', url: 'https://github.com/redes', temas: [{ id: 'tema-1', nombre: 'Modelo OSI' }] }];
const evidence: EvidenceItemV1[] = [{ id: 'evidence-1', titulo: 'Analisis de trafico', tipoEvidence: 'Laboratorio',
  estadoMadurez: 'Documentado', fechaActividadUtc: '', fechaCreacionUtc: '', fechaModificacionUtc: null,
  fechaReferencia: null, temas: [{ id: 'tema-1', nombre: 'Modelo OSI' }], herramientas: [{ id: 'tool-1', nombre: 'Wireshark' }] }];

describe('SearchService', () => {
  let search: SearchService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [SearchService, provideHttpClient(), provideHttpClientTesting()] });
    search = TestBed.inject(SearchService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  function cargar(rs = recursos, es = evidence): void {
    search.cargar();
    http.expectOne('/api/roadmap/vista').flush(vista);
    http.expectOne('/api/recursos').flush(rs);
    http.expectOne('/api/evidence').flush({ total: es.length, items: es });
  }

  it('carga tres read sides globales sin usuarioId ni detail', () => {
    expect(search.indice()).toEqual([]);
    cargar();
    expect(search.indice()).toHaveLength(4);
    expect(search.cargando()).toBe(false);
    expect(search.errores()).toEqual([]);
    expect(JSON.stringify(search.indice())).not.toContain('no-indexar');
  });

  it('mapea Tema y Fase con contexto y navegacion factual', () => {
    cargar();
    expect(search.buscar('bash')[0].items[0]).toMatchObject({ tipo: 'Tema', id: 'tema-1', ruta: '/roadmap/tema/tema-1', contexto: 'Tema · Fundamentos · En progreso' });
    expect(search.buscar('fundamentos').find((g) => g.tipo === 'Fase')?.items[0]).toMatchObject({ ruta: '/roadmap', queryParams: { fase: 'fase-1' }, contexto: 'Fase del Roadmap · 25%' });
  });

  it('mapea Resource solo con datos LIST, URL y temas', () => {
    cargar();
    const item = search.buscar('github')[0].items[0];
    expect(item).toMatchObject({ tipo: 'Recurso', titulo: 'Guia de redes', contexto: 'Resource · Documentacion · En uso', queryParams: { recursoId: 'recurso-1' } });
    expect(search.buscar('osi').map((g) => g.tipo)).toEqual(['Recurso', 'Evidence']);
  });

  it.each(TIPOS_EVIDENCE)('mapea Evidence %s sin duplicar Portfolio ni cargar AR details', (tipoEvidence) => {
    cargar([], [{ ...evidence[0], tipoEvidence, estadoMadurez: 'ListoPortafolio' }]);
    expect(search.buscar('wireshark')[0].items[0]).toMatchObject({ tipo: 'Evidence', titulo: evidence[0].titulo, ruta: '/evidence', queryParams: { evidenceId: 'evidence-1', tipoEvidence } });
    expect(search.buscar('portfolio')[0].items).toHaveLength(1);
  });

  it.each(['BASH', 'bAsH', '  bash  ', 'scripting bash', 'bas scr'])('matching local por tokens: %s', (query) => {
    cargar();
    expect(search.buscar(query)[0].items[0].id).toBe('tema-1');
  });

  it('normaliza tildes y espacios sin fuzzy matching', () => {
    cargar([{ ...recursos[0], titulo: 'Análisis práctico' }]);
    expect(search.buscar(' ANALISIS   practico ')[0].items[0].id).toBe('recurso-1');
    expect(search.buscar('analizis')).toEqual([]);
  });

  it('ordena prefijo, titulo contenido y metadata, con desempate estable', () => {
    cargar([
      { ...recursos[0], id: 'meta', titulo: 'A guia', url: 'https://bash.org' },
      { ...recursos[0], id: 'contains', titulo: 'Curso de Bash' },
      { ...recursos[0], id: 'prefix', titulo: 'Bash basico' },
    ]);
    expect(search.buscar('bash').find((g) => g.tipo === 'Recurso')?.items.map((i) => i.id)).toEqual(['prefix', 'contains', 'meta']);
  });

  it('limita cinco por categoria y conserva total; query vacia no muestra indice', () => {
    cargar(Array.from({ length: 30 }, (_, i) => ({ ...recursos[0], id: `r-${i}`, titulo: `Guia ${i}` })));
    expect(search.buscar('guia')[0].items).toHaveLength(5);
    expect(search.buscar('guia')[0].total).toBe(30);
    expect(search.buscar(' ')).toEqual([]);
    expect(search.buscar('g')[0].items.length).toBeGreaterThan(0);
  });

  it('no realiza requests por tecla ni por cargar repetidamente durante una apertura', () => {
    cargar();
    for (const query of ['b', 'ba', 'bas', 'bash', 'github', '']) search.buscar(query);
    search.cargar();
    http.expectNone(() => true);
  });

  it('permite resultados mientras otra fuente carga y reintenta solo la fallida', () => {
    search.cargar();
    http.expectOne('/api/roadmap/vista').flush(vista);
    expect(search.cargando()).toBe(true);
    expect(search.buscar('bash')).toHaveLength(1);
    http.expectOne('/api/recursos').flush({}, { status: 500, statusText: 'Internal Server Error' });
    http.expectOne('/api/evidence').flush({ total: 1, items: evidence });
    expect(search.errores()).toEqual(['Resources']);
    expect(search.buscar('wireshark')).toHaveLength(1);
    search.reintentar();
    http.expectOne('/api/recursos').flush(recursos);
    expect(search.errores()).toEqual([]);
    expect(search.buscar('github')).toHaveLength(1);
  });

  it('reapertura renueva Resources/Evidence y reutiliza la cache Roadmap vigente', () => {
    cargar();
    const siguiente = TestBed.runInInjectionContext(() => new SearchService());
    siguiente.cargar();
    http.expectNone('/api/roadmap/vista');
    http.expectOne('/api/recursos').flush([]);
    http.expectOne('/api/evidence').flush({ total: 0, items: [] });
    expect(siguiente.buscar('github')).toEqual([]);
    expect(siguiente.buscar('wireshark')).toEqual([]);
    expect(siguiente.buscar('bash')).toHaveLength(1);
  });

  it('reapertura respeta invalidacion Roadmap existente de Study', () => {
    cargar();
    TestBed.inject(RoadmapService).refrescarVista();
    const siguiente = TestBed.runInInjectionContext(() => new SearchService());
    siguiente.cargar();
    http.expectOne('/api/roadmap/vista').flush({ ...vista, fases: [] });
    http.expectOne('/api/recursos').flush([]);
    http.expectOne('/api/evidence').flush({ total: 0, items: [] });
    expect(siguiente.buscar('bash')).toEqual([]);
  });
});
