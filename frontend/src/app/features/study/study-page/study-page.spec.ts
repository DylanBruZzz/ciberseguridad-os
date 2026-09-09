import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { BehaviorSubject, NEVER, of, throwError } from 'rxjs';
import { RoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';
import { SesionEstudioResumen } from '../study.models';
import { StudyService } from '../study.service';
import { StudyPage } from './study-page';

const temaId = '01a046d5-9bf3-7cec-aa05-10c93459fa17';
const temaDosId = '01a046d5-9bf3-7cec-aa05-10c93459fa18';
const sesionId = '01a046d5-9bf3-7cec-aa05-10c93459fa19';

@Component({ template: '' })
class EmptyRouteComponent {}

const vista: RoadmapVistaV1 = {
  faseActualId: '01a046d5-9bf3-7cec-aa05-10c93459fa20',
  progresoGlobalPorcentaje: 0,
  totalTemas: 2,
  temasDominados: 0,
  fases: [
    {
      id: '01a046d5-9bf3-7cec-aa05-10c93459fa20',
      orden: 1,
      nombre: 'Fundamentos',
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
          id: temaId,
          faseId: '01a046d5-9bf3-7cec-aa05-10c93459fa20',
          temaPadreId: null,
          nombre: 'Modelo OSI / TCP-IP',
          descripcion: null,
          tipoConocimiento: 'Conceptual',
          dificultadPercibida: null,
          confianza: null,
          intervaloRepasoDias: 14,
          estado: 'NoIniciado',
          criteriosTotal: 0,
          criteriosCumplidos: 0,
          progresoPorcentaje: 0,
          ultimaSesion: null,
          proximaFechaRepaso: null,
          repasoRecomendado: false,
        },
        {
          id: temaDosId,
          faseId: '01a046d5-9bf3-7cec-aa05-10c93459fa20',
          temaPadreId: null,
          nombre: 'Linux CLI',
          descripcion: null,
          tipoConocimiento: 'Practico',
          dificultadPercibida: null,
          confianza: null,
          intervaloRepasoDias: 14,
          estado: 'NoIniciado',
          criteriosTotal: 0,
          criteriosCumplidos: 0,
          progresoPorcentaje: 0,
          ultimaSesion: null,
          proximaFechaRepaso: null,
          repasoRecomendado: false,
        },
      ],
    },
  ],
};

const sesiones: SesionEstudioResumen[] = [
  {
    id: sesionId,
    usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
    temaId,
    fecha: '2026-09-04',
    duracionMinutos: 90,
    tipo: 'Practica',
    notas: 'Prueba Study V1',
  },
  {
    id: '01a046d5-9bf3-7cec-aa05-10c93459fa21',
    usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
    temaId: temaDosId,
    fecha: '2026-09-03',
    duracionMinutos: 30,
    tipo: 'Teoria',
    notas: null,
  },
];

describe('StudyPage', () => {
  let fixture: ComponentFixture<StudyPage>;
  let parametros: BehaviorSubject<ReturnType<typeof convertToParamMap>>;
  let study: {
    listarSesiones: ReturnType<typeof vi.fn>;
    registrarSesion: ReturnType<typeof vi.fn>;
    actualizarSesion: ReturnType<typeof vi.fn>;
    eliminarSesion: ReturnType<typeof vi.fn>;
  };
  let roadmap: {
    obtenerVista: ReturnType<typeof vi.fn>;
    refrescarVista: ReturnType<typeof vi.fn>;
  };

  async function configure(queryTemaId: string | null = null): Promise<void> {
    TestBed.resetTestingModule();
    parametros = new BehaviorSubject(convertToParamMap(queryTemaId ? { temaId: queryTemaId } : {}));

    study = {
      listarSesiones: vi.fn(() => of(sesiones)),
      registrarSesion: vi.fn(() =>
        of({
          estado: 'Creada',
          id: sesionId,
          usuarioId: sesiones[0].usuarioId,
          temaId,
          fecha: '2026-09-04',
          duracionMinutos: 45,
          tipo: 'Teoria',
          notas: 'Nueva sesion',
        }),
      ),
      actualizarSesion: vi.fn(() => of(undefined)),
      eliminarSesion: vi.fn(() => of(undefined)),
    };
    roadmap = {
      obtenerVista: vi.fn(() => of(vista)),
      refrescarVista: vi.fn(() => of(vista)),
    };

    await TestBed.configureTestingModule({
      imports: [StudyPage],
      providers: [
        provideRouter([
          { path: 'roadmap/tema/:temaId', component: EmptyRouteComponent },
          { path: 'resources', component: EmptyRouteComponent },
          { path: 'evidence', component: EmptyRouteComponent },
        ]),
        {
          provide: ActivatedRoute,
          useValue: {
            queryParamMap: parametros,
          },
        },
        { provide: StudyService, useValue: study },
        { provide: RoadmapService, useValue: roadmap },
      ],
    }).compileComponents();
  }

  it('muestra loading estructural', async () => {
    await configure();
    study.listarSesiones.mockReturnValue(NEVER);
    roadmap.obtenerVista.mockReturnValue(NEVER);

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Cargando Study');
  });

  it('cambiar el contexto no arrastra una sesión en edición ni su confirmación de eliminación', async () => {
    await configure(temaId);
    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();
    const component = fixture.componentInstance as any;
    component.editarSesion(sesiones[0]);
    component.pedirEliminar(sesiones[0]);
    parametros.next(convertToParamMap({}));
    expect(component.sesionEditando()).toBeNull();
    expect(component.sesionAEliminar()).toBeNull();
    expect(component.formNotas()).toBe('');
    expect(component.formTemaId()).toBe('');
    expect(study.listarSesiones).toHaveBeenCalledTimes(1);
    expect(study.actualizarSesion).not.toHaveBeenCalled();
  });

  it('muestra modo global, resumen e historial reciente', async () => {
    await configure();

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Study global');
    expect(text).toContain('Registro manual');
    expect(text).toContain('Sesiones recientes');
    expect(text).toContain('Modelo OSI / TCP-IP');
    expect(text).toContain('Linux CLI');
    expect(text).toContain('2');
    expect(text).toContain('2 h');
    expect(text).toContain('1 h 30 min');
  });

  it('muestra modo contextual, preselecciona tema y filtra historial', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;

    expect(text).toContain('Estudiando: Modelo OSI / TCP-IP');
    expect(text).toContain('Sesiones del tema');
    expect(text).toContain('Volver al tema');
    expect(select.value).toBe(temaId);
    expect(text).toContain('Prueba Study V1');
    expect(fixture.nativeElement.querySelector('.session-list').textContent).not.toContain('Linux CLI');
    expect(fixture.nativeElement.querySelector(`a[href="/roadmap/tema/${temaId}"]`)).not.toBeNull();
  });

  it('muestra contexto invalido sin enviar Guid malformado al formulario', async () => {
    await configure('tema-malformado');

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    expect(fixture.nativeElement.textContent).toContain('No encontramos el tema solicitado.');
    expect(select.value).toBe('');
  });

  it('valida campos obligatorios antes de crear', async () => {
    await configure();

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(study.registrarSesion).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Selecciona un tema disponible.');
  });

  it('registra sesion, refresca historial e invalida roadmap', async () => {
    await configure();

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    cambiarSelect(0, temaId);
    cambiarInput('input[type="date"]', '2026-09-04');
    cambiarInput('input[type="number"]', '45');
    cambiarSelect(1, 'Repaso');
    cambiarTextarea('Nueva sesion');

    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(study.registrarSesion).toHaveBeenCalledWith({
      temaId,
      fecha: '2026-09-04',
      duracionMinutos: 45,
      tipo: 'Repaso',
      notas: 'Nueva sesion',
    });
    expect(roadmap.refrescarVista).toHaveBeenCalled();
    expect(study.listarSesiones).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('Sesion registrada.');
  });

  it('muestra error de creacion sin tumbar historial', async () => {
    await configure();
    study.registrarSesion.mockReturnValue(throwError(() => new Error('No pudimos registrar la sesion de estudio.')));

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    cambiarSelect(0, temaId);
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos registrar la sesion de estudio.');
    expect(fixture.nativeElement.textContent).toContain('Prueba Study V1');
  });

  it('entra en modo edicion y actualiza sesion', async () => {
    await configure();

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.session-actions button').click();
    fixture.detectChanges();

    cambiarInput('input[type="number"]', '75');
    cambiarTextarea('Notas corregidas');

    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(study.actualizarSesion).toHaveBeenCalledWith(sesionId, {
      temaId,
      fecha: '2026-09-04',
      duracionMinutos: 75,
      tipo: 'Practica',
      notas: 'Notas corregidas',
    });
    expect(roadmap.refrescarVista).toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Cambios guardados.');
  });

  it('cancela edicion y restaura formulario contextual', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.session-actions button').click();
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.ghost-button').click();
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    expect(select.value).toBe(temaId);
    expect(fixture.nativeElement.textContent).toContain('Registrar sesion');
  });

  it('confirma eliminacion y ejecuta soft delete por API', async () => {
    await configure();

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    const deleteButton = fixture.nativeElement.querySelector('.session-actions .danger-button') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Eliminar esta sesion?');

    const confirmButton = fixture.nativeElement.querySelector('.delete-confirm .danger-button') as HTMLButtonElement;
    confirmButton.click();
    fixture.detectChanges();

    expect(study.eliminarSesion).toHaveBeenCalledWith(sesionId);
    expect(roadmap.refrescarVista).toHaveBeenCalled();
  });

  it('muestra error de eliminacion separado', async () => {
    await configure();
    study.eliminarSesion.mockReturnValue(throwError(() => new Error('No pudimos eliminar la sesion de estudio.')));

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    const deleteButton = fixture.nativeElement.querySelector('.session-actions .danger-button') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    const confirmButton = fixture.nativeElement.querySelector('.delete-confirm .danger-button') as HTMLButtonElement;
    confirmButton.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos eliminar la sesion de estudio.');
  });

  it('muestra empty global y contextual', async () => {
    await configure();
    study.listarSesiones.mockReturnValue(of([]));

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Aún no has registrado sesiones de estudio.');

    TestBed.resetTestingModule();
    await configure(temaId);
    study.listarSesiones.mockReturnValue(of([]));

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Aún no registraste sesiones para este tema.');
  });

  it('muestra errores de carga con reintento', async () => {
    await configure();
    study.listarSesiones.mockReturnValueOnce(throwError(() => new Error('No pudimos cargar las sesiones de estudio.')));
    roadmap.obtenerVista.mockReturnValueOnce(throwError(() => new Error('No se pudo cargar la vista del roadmap.')));

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos cargar el historial.');
    expect(fixture.nativeElement.textContent).toContain('No pudimos cargar los temas.');

    fixture.nativeElement.querySelector('.history-panel .ghost-button').click();
    fixture.detectChanges();

    expect(roadmap.refrescarVista).toHaveBeenCalled();
    expect(study.listarSesiones).toHaveBeenCalledTimes(2);
  });

  it('usa enum real de tipo y no renderiza timer', async () => {
    await configure();

    fixture = TestBed.createComponent(StudyPage);
    fixture.detectChanges();

    const options = Array.from(
      fixture.nativeElement.querySelectorAll('select')[1].querySelectorAll('option') as NodeListOf<HTMLOptionElement>,
    ).map((option) => option.value);
    const text = fixture.nativeElement.textContent;

    expect(options).toEqual(['Teoria', 'Practica', 'Laboratorio', 'Repaso']);
    expect(text).not.toContain('Timer');
    expect(text).not.toContain('Pomodoro');
    expect(text).not.toContain('Pausa');
    expect(text).not.toContain('Reanudar');
  });

  function cambiarInput(selector: string, value: string): void {
    const input = fixture.nativeElement.querySelector(selector) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function cambiarSelect(index: number, value: string): void {
    const select = fixture.nativeElement.querySelectorAll('select')[index] as HTMLSelectElement;
    select.value = value;
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }

  function cambiarTextarea(value: string): void {
    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    textarea.value = value;
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }
});
