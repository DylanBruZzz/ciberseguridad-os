import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { NEVER, of, throwError } from 'rxjs';
import { TemaWorkspaceV1 } from './tema-workspace.models';
import { TemaWorkspaceService } from './tema-workspace.service';
import { TopicWorkspacePage } from './topic-workspace-page';

const temaId = '01a046d5-9bf3-7cec-aa05-10c93459fa16';
const faseId = '01a046d5-9bf3-7cec-aa05-10c93459fa17';

@Component({ template: '' })
class EmptyRouteComponent {}

const workspaceBase: TemaWorkspaceV1 = {
  tema: {
    id: temaId,
    temaPadreId: null,
    nombre: 'Modelo OSI / TCP-IP',
    descripcion: 'Base para diagnosticar redes.',
    tipoConocimiento: 'Conceptual',
    estado: 'EnRepaso',
    dificultadPercibida: 4,
    confianza: 3,
    intervaloRepasoDias: 14,
    criteriosTotal: 2,
    criteriosCumplidos: 2,
    progresoPorcentaje: 100,
    objetivos: ['Comprender encapsulacion', 'Diferenciar TCP y UDP'],
    criterios: [
      {
        id: '01a046d5-9bf3-7cec-aa05-10c93459fa18',
        tipo: 'Teoria',
        cumplido: true,
        fechaCumplidoUtc: '2026-09-01T00:00:00Z',
      },
      {
        id: '01a046d5-9bf3-7cec-aa05-10c93459fa19',
        tipo: 'Practica',
        cumplido: false,
        fechaCumplidoUtc: null,
      },
    ],
  },
  fase: {
    id: faseId,
    orden: 1,
    nombre: 'Fundamentos',
  },
  apuntes: {
    contenido: 'Capas, encapsulacion y troubleshooting.',
    fechaModificacionUtc: '2026-09-02T00:00:00Z',
  },
  ultimaSesion: {
    id: '01a046d5-9bf3-7cec-aa05-10c93459fa20',
    fecha: '2026-08-30',
    duracionMinutos: 45,
    tipo: 'Repaso',
  },
  repaso: {
    proximaFechaRepaso: '2026-09-13',
    repasoRecomendado: true,
  },
  resourcesResumen: { total: 3 },
  sesionesResumen: { total: 2, totalMinutos: 75 },
  evidenceResumen: {
    total: 5,
    proyectos: 1,
    laboratorios: 1,
    writeups: 1,
    artefactosTecnicos: 1,
    certificacionesObtenidas: 1,
  },
};

describe('TopicWorkspacePage', () => {
  let fixture: ComponentFixture<TopicWorkspacePage>;

  async function configure(
    workspace: Partial<TemaWorkspaceService>,
    routeTemaId = temaId,
  ): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [TopicWorkspacePage],
      providers: [
        provideRouter([
          { path: 'roadmap', component: EmptyRouteComponent },
          { path: 'study', component: EmptyRouteComponent },
          { path: 'resources', component: EmptyRouteComponent },
          { path: 'evidence', component: EmptyRouteComponent },
        ]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ temaId: routeTemaId }),
            },
          },
        },
        { provide: TemaWorkspaceService, useValue: workspace },
      ],
    }).compileComponents();
  }

  it('muestra loading estructural', async () => {
    await configure({ obtenerWorkspace: () => NEVER });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Cargando workspace del tema');
  });

  it('muestra header, objetivos, criterios, progreso y estado exactos del backend', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Roadmap');
    expect(text).toContain('Fundamentos');
    expect(text).toContain('Modelo OSI / TCP-IP');
    expect(text).toContain('EnRepaso');
    expect(text).toContain('100%');
    expect(text).toContain('2 / 2');
    expect(text).toContain('Comprender encapsulacion');
    expect(text).toContain('Diferenciar TCP y UDP');
    expect(text).toContain('Teoria');
    expect(text).toContain('Practica');
    expect(text).toContain('Cumplido');
    expect(text).toContain('Pendiente');
  });

  it('muestra EnRepaso con progreso 100 sin tratarlo como reinicio', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('EnRepaso');
    expect(text).toContain('100%');
    expect(text).toContain('Conviene repasar este tema.');
  });

  it('muestra percepcion, ultima sesion, proximo repaso y CTA de continuidad', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Dificultad 4/5');
    expect(text).toContain('Confianza 3/5');
    expect(text).toContain('45 min');
    expect(text).toContain('Repaso');
    expect(text).toContain('Continuar estudiando');
  });

  it('muestra estado sin sesion y CTA para comenzar', async () => {
    await configure({
      obtenerWorkspace: () =>
        of({
          ...workspaceBase,
          ultimaSesion: null,
          repaso: { proximaFechaRepaso: null, repasoRecomendado: false },
          sesionesResumen: { total: 0, totalMinutos: 0 },
        }),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Nunca registraste una sesion para este tema.');
    expect(text).toContain('Sin proxima fecha de repaso.');
    expect(text).toContain('Comenzar a estudiar');
  });

  it('soporta objetivos, criterios, apuntes y resumenes vacios', async () => {
    await configure({
      obtenerWorkspace: () =>
        of({
          ...workspaceBase,
          tema: {
            ...workspaceBase.tema,
            objetivos: [],
            criterios: [],
            criteriosTotal: 0,
            criteriosCumplidos: 0,
            progresoPorcentaje: 0,
            dificultadPercibida: null,
            confianza: null,
          },
          apuntes: { contenido: '', fechaModificacionUtc: null },
          resourcesResumen: { total: 0 },
          evidenceResumen: {
            total: 0,
            proyectos: 0,
            laboratorios: 0,
            writeups: 0,
            artefactosTecnicos: 0,
            certificacionesObtenidas: 0,
          },
        }),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Este tema todavia no tiene objetivos definidos.');
    expect(text).toContain('Este tema todavia no tiene criterios definidos.');
    expect(text).toContain('Dificultad no definida');
    expect(text).toContain('Confianza no definida');
    expect(text).toContain('0 recursos vinculados');
    expect(text).toContain('Sin evidence vinculada');
  });

  it('muestra apuntes existentes y estado dirty al editar', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    expect(textarea.value).toBe('Capas, encapsulacion y troubleshooting.');

    textarea.value = 'Nuevo apunte';
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Cambios sin guardar');
  });

  it('guarda apuntes con PUT sin usuarioId y actualiza estado local', async () => {
    const guardarApuntes = vi.fn(() => of(undefined));
    await configure({
      obtenerWorkspace: () => of(workspaceBase),
      guardarApuntes,
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    textarea.value = 'Nuevo apunte';
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.notes-actions button').click();
    fixture.detectChanges();

    expect(guardarApuntes).toHaveBeenCalledWith(temaId, 'Nuevo apunte');
    expect(fixture.nativeElement.textContent).toContain('Guardado');
  });

  it('muestra error de guardado de apuntes', async () => {
    await configure({
      obtenerWorkspace: () => of(workspaceBase),
      guardarApuntes: () => throwError(() => new Error('No pudimos guardar los apuntes.')),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    textarea.value = 'Nuevo apunte';
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.notes-actions button').click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos guardar los apuntes.');
  });

  it('muestra enlaces contextuales con temaId', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector(`a[href="/study?temaId=${temaId}"]`)).not.toBeNull();
    expect(fixture.nativeElement.querySelector(`a[href="/resources?temaId=${temaId}"]`)).not.toBeNull();
    expect(fixture.nativeElement.querySelector(`a[href="/evidence?temaId=${temaId}"]`)).not.toBeNull();
  });

  it('muestra error 404 especifico', async () => {
    await configure({
      obtenerWorkspace: () => throwError(() => new Error('Este tema no esta disponible.')),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Este tema no esta disponible.');
  });

  it('muestra error generico y permite reintentar', async () => {
    const refrescar = vi
      .fn()
      .mockReturnValueOnce(throwError(() => new Error('No pudimos cargar este tema.')))
      .mockReturnValueOnce(of(workspaceBase));
    await configure({ obtenerWorkspace: refrescar });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos cargar este tema.');

    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();

    expect(refrescar).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('Modelo OSI / TCP-IP');
  });

  it('trata temaId malformado como no disponible sin llamar la API', async () => {
    const obtenerWorkspace = vi.fn(() => of(workspaceBase));
    await configure({ obtenerWorkspace }, 'tema-malformado');

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    expect(obtenerWorkspace).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Este tema no esta disponible.');
  });

  it('muestra los resumenes de Resources, Sesiones y Evidence', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('3 recursos vinculados');
    expect(text).toContain('2 sesiones');
    expect(text).toContain('75 min');
    expect(text).toContain('5 elementos vinculados');
    expect(text).toContain('1 proyecto');
    expect(text).toContain('1 laboratorio');
    expect(text).toContain('1 writeup');
    expect(text).toContain('1 artefacto');
    expect(text).toContain('1 certificacion');
  });
});
