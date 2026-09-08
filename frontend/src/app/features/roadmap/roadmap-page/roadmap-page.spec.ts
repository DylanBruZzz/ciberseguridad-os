import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, ParamMap, Router, convertToParamMap, provideRouter } from '@angular/router';
import { BehaviorSubject, NEVER, of, throwError } from 'rxjs';
import { RoadmapVistaV1 } from '../roadmap.models';
import { RoadmapService } from '../roadmap.service';
import { RoadmapPage } from './roadmap-page';

const vista: RoadmapVistaV1 = {
  faseActualId: 'fase-2',
  progresoGlobalPorcentaje: 23,
  totalTemas: 3,
  temasDominados: 1,
  fases: [
    {
      id: 'fase-1',
      orden: 1,
      nombre: 'Fundamentos de Informatica y Redes',
      color: null,
      descripcion: 'Base de redes',
      objetivos: ['Comprender redes'],
      criteriosAvance: ['Explicar OSI'],
      mesInicioRecomendado: 1,
      mesFinRecomendado: 2,
      cargaSemanalRecomendada: '10 hrs/semana',
      totalTemas: 1,
      temasDominados: 1,
      progresoPorcentaje: 100,
      estaCompletada: true,
      esFaseActual: false,
      temas: [
        {
          id: 'tema-1',
          faseId: 'fase-1',
          temaPadreId: null,
          nombre: 'Modelo OSI / TCP-IP',
          descripcion: null,
          tipoConocimiento: 'Teorico',
          dificultadPercibida: 3,
          confianza: 4,
          intervaloRepasoDias: 14,
          estado: 'Dominado',
          criteriosTotal: 2,
          criteriosCumplidos: 2,
          progresoPorcentaje: 100,
          ultimaSesion: null,
          proximaFechaRepaso: null,
          repasoRecomendado: false,
        },
      ],
    },
    {
      id: 'fase-2',
      orden: 2,
      nombre: 'Linux y CLI',
      color: null,
      descripcion: 'Trabajo de terminal',
      objetivos: [],
      criteriosAvance: [],
      mesInicioRecomendado: null,
      mesFinRecomendado: null,
      cargaSemanalRecomendada: null,
      totalTemas: 2,
      temasDominados: 0,
      progresoPorcentaje: 0,
      estaCompletada: false,
      esFaseActual: true,
      temas: [
        {
          id: 'tema-2',
          faseId: 'fase-2',
          temaPadreId: null,
          nombre: 'Shell scripting',
          descripcion: null,
          tipoConocimiento: 'Practico',
          dificultadPercibida: null,
          confianza: null,
          intervaloRepasoDias: 21,
          estado: 'NoIniciado',
          criteriosTotal: 0,
          criteriosCumplidos: 0,
          progresoPorcentaje: 0,
          ultimaSesion: null,
          proximaFechaRepaso: null,
          repasoRecomendado: false,
        },
        {
          id: 'tema-3',
          faseId: 'fase-2',
          temaPadreId: null,
          nombre: 'Permisos Linux',
          descripcion: null,
          tipoConocimiento: 'Practico',
          dificultadPercibida: null,
          confianza: null,
          intervaloRepasoDias: 21,
          estado: 'EnRepaso',
          criteriosTotal: 2,
          criteriosCumplidos: 2,
          progresoPorcentaje: 100,
          ultimaSesion: '2026-09-01',
          proximaFechaRepaso: '2026-09-22',
          repasoRecomendado: true,
        },
      ],
    },
  ],
};

describe('RoadmapPage', () => {
  let fixture: ComponentFixture<RoadmapPage>;

  function configure(roadmap: Partial<RoadmapService>): Promise<void> {
    TestBed.resetTestingModule();
    return TestBed.configureTestingModule({
      imports: [RoadmapPage],
      providers: [
        provideRouter([
          { path: 'roadmap/tema/:temaId', component: RoadmapPage },
          { path: 'roadmap', component: RoadmapPage },
        ]),
        { provide: RoadmapService, useValue: roadmap },
      ],
    }).compileComponents();
  }

  it('muestra estado loading', async () => {
    await configure({ obtenerVista: () => NEVER });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Cargando roadmap');
  });

  it('muestra fases ordenadas, metadata y progreso entregado por el DTO', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('2 fases');
    expect(text).toContain('3 temas');
    expect(text).toContain('Fundamentos de Informatica y Redes');
    expect(text).toContain('Linux y CLI');
    expect(text).toContain('0%');
    expect(text).toContain('Shell scripting');
  });

  it('selecciona faseActual inicialmente y diferencia seleccion de fase actual', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Linux y CLI');
    expect(fixture.nativeElement.textContent).toContain('Fase actual');

    const firstPhase = fixture.nativeElement.querySelector('button');
    firstPhase.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Fundamentos de Informatica y Redes');
    expect(fixture.nativeElement.textContent).toContain('Seleccionada para explorar');
  });

  it('muestra objetivos, criterios, estado de tema, progreso y repaso factual', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Shell scripting');
    expect(text).toContain('NoIniciado');
    expect(text).toContain('Permisos Linux');
    expect(text).toContain('EnRepaso');
    expect(text).toContain('Repaso');
    expect(text).toContain('0%');
  });

  it('muestra estado vacio', async () => {
    await configure({
      obtenerVista: () =>
        of({
          faseActualId: null,
          progresoGlobalPorcentaje: 0,
          totalTemas: 0,
          temasDominados: 0,
          fases: [],
        }),
    });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Tu roadmap todavia no contiene fases.');
  });

  it('muestra error de carga', async () => {
    await configure({
      obtenerVista: () => throwError(() => new Error('API local no disponible')),
      refrescarVista: () => of(vista),
    });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos cargar tu roadmap.');
    expect(fixture.nativeElement.textContent).toContain('API local no disponible');
  });

  it('reintenta luego de un error', async () => {
    await configure({
      obtenerVista: () => throwError(() => new Error('API local no disponible')),
      refrescarVista: () => of(vista),
    });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Linux y CLI');
  });

  it('prepara navegacion al workspace futuro del tema', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl');
    const topicLink = fixture.nativeElement.querySelector('a[href="/roadmap/tema/tema-2"]');

    expect(topicLink).not.toBeNull();
    topicLink.click();
    fixture.detectChanges();

    expect(navigateSpy).toHaveBeenCalled();
  });

  it('actualiza la fase seleccionada cuando cambia ?fase= en la misma ruta', async () => {
    TestBed.resetTestingModule();
    const params = new BehaviorSubject<ParamMap>(convertToParamMap({ fase: 'fase-1' }));
    const route = {
      queryParamMap: params.asObservable(),
      get snapshot() {
        return { queryParamMap: params.value };
      },
    };

    await TestBed.configureTestingModule({
      imports: [RoadmapPage],
      providers: [
        provideRouter([{ path: 'roadmap/tema/:temaId', component: RoadmapPage }]),
        { provide: ActivatedRoute, useValue: route },
        { provide: RoadmapService, useValue: { obtenerVista: () => of(vista) } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Fundamentos de Informatica y Redes');

    params.next(convertToParamMap({ fase: 'fase-2' }));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Linux y CLI');
  });
});
