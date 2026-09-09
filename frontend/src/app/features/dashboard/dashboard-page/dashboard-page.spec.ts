import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { UsuarioActualService } from '../../../core/usuario-actual.service';
import { RoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';
import { DashboardPage } from './dashboard-page';

const vista: RoadmapVistaV1 = {
  faseActualId: 'fase-2',
  progresoGlobalPorcentaje: 42,
  totalTemas: 3,
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
          nombre: 'Modelo OSI',
          descripcion: null,
          tipoConocimiento: 'Teorico',
          dificultadPercibida: null,
          confianza: null,
          intervaloRepasoDias: 21,
          estado: 'Dominado',
          criteriosTotal: 2,
          criteriosCumplidos: 2,
          progresoPorcentaje: 100,
          ultimaSesion: '2026-08-01',
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
      descripcion: null,
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

describe('DashboardPage', () => {
  let fixture: ComponentFixture<DashboardPage>;

  function configure(
    roadmap: Partial<RoadmapService>,
    usuarioActual: Partial<UsuarioActualService> = { obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }) },
  ): Promise<void> {
    return TestBed.configureTestingModule({
      imports: [DashboardPage],
      providers: [
        provideRouter([
          { path: 'roadmap', component: DashboardPage },
          { path: 'study', component: DashboardPage },
        ]),
        { provide: RoadmapService, useValue: roadmap },
        { provide: UsuarioActualService, useValue: usuarioActual },
      ],
    }).compileComponents();
  }

  it('muestra saludo usuario y progreso global del DTO', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Hola, Dylan');
    expect(text).toContain('42%');
    expect(text).toContain('Progreso global');
  });

  it('muestra progreso conectado y distingue fases completada actual y pendiente', async () => {
    const vistaConPendiente = {
      ...vista,
      fases: [
        ...vista.fases,
        {
          ...vista.fases[1],
          id: 'fase-3',
          orden: 3,
          nombre: 'Web',
          esFaseActual: false,
        },
      ],
    };
    await configure({ obtenerVista: () => of(vistaConPendiente) });

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Completada');
    expect(text).toContain('Fase actual');
    expect(text).toContain('Pendiente');
  });

  it('muestra fase actual y enlace con query param hacia Roadmap', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Fase 2');
    expect(text).toContain('Linux y CLI');

    const link = fixture.nativeElement.querySelector('a[href="/roadmap?fase=fase-2"]');
    expect(link).not.toBeNull();
  });

  it('continua ultimo tema cuando existe ultimaSesion factual', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Continuar donde lo dejaste');
    expect(text).toContain('Permisos Linux');
    expect(text).toContain('Continuar estudiando');
  });

  it('usa fallback honesto a fase actual cuando no hay sesiones ni orden factual de tema', async () => {
    const sinSesiones = {
      ...vista,
      fases: vista.fases.map((fase) => ({
        ...fase,
        temas: fase.temas.map((tema) => ({ ...tema, ultimaSesion: null })),
      })),
    };
    await configure({ obtenerVista: () => of(sinSesiones) });

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Explorar fase actual');
    expect(text).toContain('Linux y CLI');
    expect(text).toContain('Ver fase');
    expect(text).not.toContain('Shell scripting');
  });

  it('muestra conteo de repasos y no muestra streak inventado', async () => {
    await configure({ obtenerVista: () => of(vista) });

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Repasos recomendados: 1');
    expect(text.toLowerCase()).not.toContain('streak');
    expect(text.toLowerCase()).not.toContain('racha');
  });

  it('mantiene dashboard usable con roadmap vacio', async () => {
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

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Tu roadmap todavia no contiene fases.');
  });

  it('muestra error de roadmap y permite reintentar', async () => {
    await configure({
      obtenerVista: () => throwError(() => new Error('API local no disponible')),
      refrescarVista: () => of(vista),
    });

    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('API local no disponible');

    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Linux y CLI');
  });

  it('retry tras API offline recupera también el saludo factual', async () => {
    const obtener = vi.fn().mockReturnValueOnce(throwError(() => new Error('Offline')))
      .mockReturnValue(of({ id: 'usuario-1', nombre: 'Dylan' }));
    await configure({ obtenerVista: () => throwError(() => new Error('Offline')), refrescarVista: () => of(vista) }, { obtener });
    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Dylan');
    expect(obtener).toHaveBeenCalledTimes(2);
  });
});
