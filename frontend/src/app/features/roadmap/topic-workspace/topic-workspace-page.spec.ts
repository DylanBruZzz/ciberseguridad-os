import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { BehaviorSubject, NEVER, of, Subject, throwError } from 'rxjs';
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
        descripcion: 'Explicar las capas del modelo OSI y su relacion con TCP/IP.',
        cumplido: true,
        fechaCumplidoUtc: '2026-09-01T00:00:00Z',
      },
      {
        id: '01a046d5-9bf3-7cec-aa05-10c93459fa19',
        tipo: 'Practica',
        descripcion: 'Identificar protocolos relevantes en una captura de red.',
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
  herramientas: [
    {
      id: '01a046d5-9bf3-7cec-aa05-10c93459fa21',
      nombre: 'Wireshark',
    },
  ],
  certificaciones: [
    {
      id: '01a046d5-9bf3-7cec-aa05-10c93459fa22',
      nombre: 'Security+',
      proveedor: 'CompTIA',
      tipoCosto: 'Pago',
    },
  ],
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
  let parametros: BehaviorSubject<ReturnType<typeof convertToParamMap>>;

  async function configure(
    workspace: Partial<TemaWorkspaceService>,
    routeTemaId = temaId,
  ): Promise<void> {
    parametros = new BehaviorSubject(convertToParamMap({ temaId: routeTemaId }));
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
            paramMap: parametros,
            snapshot: {
              paramMap: convertToParamMap({ temaId: routeTemaId }),
            },
          },
        },
        {
          provide: TemaWorkspaceService,
          useValue: {
            guardarApuntes: () => of(undefined),
            listarHerramientas: () => of([]),
            vincularHerramienta: () => of(undefined),
            desvincularHerramienta: () => of(undefined),
            definirCriterios: () => of(undefined),
            marcarCriterio: () => of(undefined),
            desmarcarCriterio: () => of(undefined),
            listarCertificaciones: () => of([]),
            vincularCertificacion: () => of(undefined),
            ...workspace,
          },
        },
      ],
    }).compileComponents();
  }

  it('muestra loading estructural', async () => {
    await configure({ obtenerWorkspace: () => NEVER });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Cargando workspace del tema');
  });

  it('Cancelar recupera los apuntes guardados sin enviar writes y limpia dirty/error', async () => {
    const guardarApuntes = vi.fn();
    await configure({ obtenerWorkspace: () => of(workspaceBase), guardarApuntes, listarHerramientas: () => of([]) });
    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    const component = fixture.componentInstance as any;
    component.borradorApuntes.set('Cambios sin guardar');
    component.errorGuardado.set('Error anterior');
    component.cancelarApuntes();
    expect(component.borradorApuntes()).toBe(workspaceBase.apuntes.contenido);
    expect(component.hayCambiosApuntes()).toBe(false);
    expect(component.errorGuardado()).toBeNull();
    expect(guardarApuntes).not.toHaveBeenCalled();
  });

  it('cambia de Tema sin recrear la página, cancela la lectura anterior y limpia apuntes', async () => {
    const anterior = new Subject<TemaWorkspaceV1>();
    const nuevo = { ...workspaceBase, tema: { ...workspaceBase.tema, id: faseId, nombre: 'Bash' }, apuntes: { ...workspaceBase.apuntes, contenido: 'Apuntes Bash' } };
    const obtenerWorkspace = vi.fn().mockReturnValueOnce(anterior).mockReturnValue(of(nuevo));
    await configure({ obtenerWorkspace, listarHerramientas: () => of([]) });
    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    parametros.next(convertToParamMap({ temaId: faseId }));
    anterior.next(workspaceBase);
    fixture.detectChanges();
    expect(obtenerWorkspace).toHaveBeenLastCalledWith(faseId);
    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Bash');
    expect(fixture.nativeElement.querySelector('.notes-editor textarea').value).toBe('Apuntes Bash');
    expect(anterior.observed).toBe(false);
    parametros.next(convertToParamMap({ temaId: 'invalido' }));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('textarea')).toBeNull();
    expect(obtenerWorkspace).toHaveBeenCalledTimes(2);
  });

  it('un guardado pendiente de otro Tema no sobrescribe apuntes ni estado del Tema actual', async () => {
    const guardado = new Subject<void>();
    await configure({ obtenerWorkspace: () => of(workspaceBase), guardarApuntes: () => guardado, listarHerramientas: () => of([]) });
    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    const component = fixture.componentInstance as any;
    component.borradorApuntes.set('Borrador anterior');
    component.guardarApuntes();
    parametros.next(convertToParamMap({ temaId: faseId }));
    guardado.next();
    expect(component.borradorApuntes()).toBe(workspaceBase.apuntes.contenido);
    expect(component.apuntes().contenido).toBe(workspaceBase.apuntes.contenido);
    expect(component.estadoGuardado()).toBe('idle');
  });

  it('muestra header, objetivos, criterios, progreso y estado exactos del backend', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase), listarHerramientas: () => of([]) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Roadmap');
    expect(text).toContain('Fundamentos');
    expect(text).toContain('Modelo OSI / TCP-IP');
    expect(text).toContain('En repaso');
    expect(text).toContain('100%');
    expect(text).toContain('2 / 2');
    expect(text).toContain('Comprender encapsulacion');
    expect(text).toContain('Diferenciar TCP y UDP');
    expect(text).toContain('Teoria');
    expect(text).toContain('Practica');
    expect(text).toContain('Explicar las capas del modelo OSI');
    expect(text).toContain('Identificar protocolos relevantes');
    expect(text).toContain('Cumplido');
    expect(text).toContain('Pendiente');
  });

  it('muestra EnRepaso con progreso 100 sin tratarlo como reinicio', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase), listarHerramientas: () => of([]) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('En repaso');
    expect(text).toContain('100%');
    expect(text).toContain('Conviene repasar este tema.');
  });

  it('muestra percepcion, ultima sesion, proximo repaso y CTA de continuidad', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase), listarHerramientas: () => of([]) });

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
      listarHerramientas: () => of([]),
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
          herramientas: [],
          certificaciones: [],
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
      listarHerramientas: () => of([]),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Este tema todavia no tiene objetivos definidos.');
    expect(text).toContain('Aún no hay criterios definidos para este tema.');
    expect(text).toContain('Dificultad no definida');
    expect(text).toContain('Confianza no definida');
    expect(text).toContain('0 recursos vinculados');
    expect(text).toContain('Sin evidence vinculada');
    expect(text).toContain('Sin certificaciones relacionadas.');
  });

  it('muestra apuntes existentes y estado dirty al editar', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase), listarHerramientas: () => of([]) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('.notes-editor textarea') as HTMLTextAreaElement;
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
      listarHerramientas: () => of([]),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('.notes-editor textarea') as HTMLTextAreaElement;
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
      listarHerramientas: () => of([]),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('.notes-editor textarea') as HTMLTextAreaElement;
    textarea.value = 'Nuevo apunte';
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.notes-actions button').click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos guardar los apuntes.');
  });

  it('muestra enlaces contextuales con temaId', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase), listarHerramientas: () => of([]) });

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
    await configure({ obtenerWorkspace: refrescar, listarHerramientas: () => of([]) });

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
    await configure({ obtenerWorkspace, listarHerramientas: () => of([]) }, 'tema-malformado');

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    expect(obtenerWorkspace).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Este tema no esta disponible.');
  });

  it('muestra los resumenes de Resources, Sesiones y Evidence', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase), listarHerramientas: () => of([]) });

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

  it('muestra herramientas vinculadas', async () => {
    await configure({ obtenerWorkspace: () => of(workspaceBase), listarHerramientas: () => of([]) });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Herramientas');
    expect(text).toContain('Wireshark');
  });

  it('define criterios y refresca el progreso desde el backend', async () => {
    const actualizado = {
      ...workspaceBase,
      tema: {
        ...workspaceBase.tema,
        criteriosTotal: 2,
        criteriosCumplidos: 0,
        progresoPorcentaje: 0,
        criterios: [
          { id: 'criterio-1', tipo: 'Teoria', descripcion: 'Explicar OSI.', cumplido: false, fechaCumplidoUtc: null },
          { id: 'criterio-2', tipo: 'Practica', descripcion: 'Diagnosticar red.', cumplido: false, fechaCumplidoUtc: null },
        ],
      },
    };
    const obtenerWorkspace = vi.fn().mockReturnValueOnce(of({
      ...workspaceBase,
      tema: { ...workspaceBase.tema, criterios: [], criteriosTotal: 0, criteriosCumplidos: 0, progresoPorcentaje: 0 },
    })).mockReturnValueOnce(of(actualizado));
    const definirCriterios = vi.fn(() => of(undefined));
    await configure({ obtenerWorkspace, definirCriterios });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    const checks = fixture.nativeElement.querySelectorAll('.criteria-editor input') as NodeListOf<HTMLInputElement>;
    checks[0].click();
    checks[1].click();
    fixture.detectChanges();
    const descripciones = fixture.nativeElement.querySelectorAll('.criteria-editor textarea') as NodeListOf<HTMLTextAreaElement>;
    descripciones[0].value = '  Explicar OSI.  ';
    descripciones[0].dispatchEvent(new Event('input'));
    descripciones[1].value = 'Diagnosticar red.';
    descripciones[1].dispatchEvent(new Event('input'));
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.criteria-editor button').click();
    fixture.detectChanges();

    expect(definirCriterios).toHaveBeenCalledWith(temaId, [
      { tipo: 'Teoria', descripcion: 'Explicar OSI.' },
      { tipo: 'Practica', descripcion: 'Diagnosticar red.' },
    ]);
    expect(obtenerWorkspace).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('Teoria');
    expect(fixture.nativeElement.textContent).toContain('Explicar OSI.');
    expect(fixture.nativeElement.textContent).toContain('0%');
  });

  it('no permite definir criterios seleccionados sin descripcion', async () => {
    const definirCriterios = vi.fn(() => of(undefined));
    await configure({
      obtenerWorkspace: () => of({
        ...workspaceBase,
        tema: { ...workspaceBase.tema, criterios: [], criteriosTotal: 0, criteriosCumplidos: 0, progresoPorcentaje: 0 },
      }),
      definirCriterios,
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    const checks = fixture.nativeElement.querySelectorAll('.criteria-editor input') as NodeListOf<HTMLInputElement>;
    checks[0].click();
    checks[1].click();
    fixture.detectChanges();

    const boton = fixture.nativeElement.querySelector('.criteria-editor button') as HTMLButtonElement;
    expect(boton.disabled).toBe(true);
    boton.click();
    expect(definirCriterios).not.toHaveBeenCalled();
  });

  it('marca y desmarca criterios usando el backend y no recalcula progreso localmente', async () => {
    const marcado = {
      ...workspaceBase,
      tema: {
        ...workspaceBase.tema,
        criteriosCumplidos: 2,
        progresoPorcentaje: 100,
        criterios: workspaceBase.tema.criterios.map((criterio) =>
          criterio.tipo === 'Practica'
            ? { ...criterio, cumplido: true, fechaCumplidoUtc: '2026-09-03T00:00:00Z' }
            : criterio,
        ),
      },
    };
    const desmarcado = {
      ...workspaceBase,
      tema: {
        ...workspaceBase.tema,
        criteriosCumplidos: 1,
        progresoPorcentaje: 50,
        criterios: workspaceBase.tema.criterios.map((criterio) =>
          criterio.tipo === 'Teoria'
            ? { ...criterio, cumplido: false, fechaCumplidoUtc: null }
            : criterio,
        ),
      },
    };
    const obtenerWorkspace = vi.fn()
      .mockReturnValueOnce(of(workspaceBase))
      .mockReturnValueOnce(of(marcado))
      .mockReturnValueOnce(of(desmarcado));
    const marcarCriterio = vi.fn(() => of(undefined));
    const desmarcarCriterio = vi.fn(() => of(undefined));
    await configure({ obtenerWorkspace, marcarCriterio, desmarcarCriterio });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    const botones = fixture.nativeElement.querySelectorAll('.criteria-list button') as NodeListOf<HTMLButtonElement>;
    botones[1].click();
    fixture.detectChanges();

    expect(marcarCriterio).toHaveBeenCalledWith(temaId, 'Practica');
    expect(fixture.nativeElement.textContent).toContain('2 / 2');

    const botonDesmarcar = fixture.nativeElement.querySelector('.criteria-list button') as HTMLButtonElement;
    botonDesmarcar.click();
    fixture.detectChanges();

    expect(desmarcarCriterio).toHaveBeenCalledWith(temaId, 'Teoria');
    expect(fixture.nativeElement.textContent).toContain('50%');
  });

  it('error al definir criterios conserva el estado factual recibido', async () => {
    await configure({
      obtenerWorkspace: () => of({ ...workspaceBase, tema: { ...workspaceBase.tema, criterios: [], criteriosTotal: 0, criteriosCumplidos: 0, progresoPorcentaje: 0 } }),
      definirCriterios: () => throwError(() => new Error('No pudimos definir los criterios.')),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    const checks = fixture.nativeElement.querySelectorAll('.criteria-editor input') as NodeListOf<HTMLInputElement>;
    checks[0].click();
    checks[1].click();
    fixture.detectChanges();
    const descripciones = fixture.nativeElement.querySelectorAll('.criteria-editor textarea') as NodeListOf<HTMLTextAreaElement>;
    descripciones[0].value = 'Explicar teoria.';
    descripciones[0].dispatchEvent(new Event('input'));
    descripciones[1].value = 'Aplicar practica.';
    descripciones[1].dispatchEvent(new Event('input'));
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.criteria-editor button').click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No pudimos definir los criterios.');
    expect(fixture.nativeElement.textContent).toContain('Aún no hay criterios definidos para este tema.');
  });

  it('agrega herramienta y actualiza la lista al confirmar API', async () => {
    const vincularHerramienta = vi.fn(() => of(undefined));
    await configure({
      obtenerWorkspace: () => of({ ...workspaceBase, herramientas: [] }),
      listarHerramientas: () => of([
        { id: 'herramienta-1', nombre: 'PowerShell', categoria: 'Sistema' },
      ]),
      vincularHerramienta,
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('.tool-picker select') as HTMLSelectElement;
    select.value = 'herramienta-1';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.tool-picker button').click();
    fixture.detectChanges();

    expect(vincularHerramienta).toHaveBeenCalledWith(temaId, 'herramienta-1');
    expect(fixture.nativeElement.textContent).toContain('PowerShell');
  });

  it('quita herramienta y elimina solo el vinculo visual', async () => {
    const desvincularHerramienta = vi.fn(() => of(undefined));
    await configure({
      obtenerWorkspace: () => of(workspaceBase),
      listarHerramientas: () => of([]),
      desvincularHerramienta,
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.tool-list button').click();
    fixture.detectChanges();

    expect(desvincularHerramienta).toHaveBeenCalledWith(temaId, workspaceBase.herramientas[0].id);
    expect(fixture.nativeElement.textContent).not.toContain('Wireshark');
  });

  it('error al agregar herramienta no deja estado falso', async () => {
    await configure({
      obtenerWorkspace: () => of({ ...workspaceBase, herramientas: [] }),
      listarHerramientas: () => of([
        { id: 'herramienta-1', nombre: 'PowerShell', categoria: 'Sistema' },
      ]),
      vincularHerramienta: () => throwError(() => new Error('No pudimos vincular la herramienta.')),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('.tool-picker select') as HTMLSelectElement;
    select.value = 'herramienta-1';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.tool-picker button').click();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('No pudimos vincular la herramienta.');
    expect(fixture.nativeElement.querySelector('.tools-section .tool-list')).toBeNull();
  });

  it('muestra certificaciones relacionadas y permite vincular una certificacion existente', async () => {
    const vincularCertificacion = vi.fn(() => of(undefined));
    await configure({
      obtenerWorkspace: () => of({ ...workspaceBase, certificaciones: [] }),
      listarCertificaciones: () => of([
        { id: 'certificacion-2', nombre: 'PNPT', proveedor: 'TCM', tipoCosto: 'Pago', url: null },
      ]),
      vincularCertificacion,
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Sin certificaciones relacionadas.');

    const selects = fixture.nativeElement.querySelectorAll('.tool-picker select') as NodeListOf<HTMLSelectElement>;
    const selectCertificacion = selects[1];
    selectCertificacion.value = 'certificacion-2';
    selectCertificacion.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    const botones = fixture.nativeElement.querySelectorAll('.tool-picker button') as NodeListOf<HTMLButtonElement>;
    botones[1].click();
    fixture.detectChanges();

    expect(vincularCertificacion).toHaveBeenCalledWith(temaId, 'certificacion-2');
    expect(fixture.nativeElement.textContent).toContain('PNPT');
    expect(fixture.nativeElement.textContent).toContain('TCM');
  });

  it('error al vincular certificacion no deja relacion falsa', async () => {
    await configure({
      obtenerWorkspace: () => of({ ...workspaceBase, certificaciones: [] }),
      listarCertificaciones: () => of([
        { id: 'certificacion-2', nombre: 'PNPT', proveedor: 'TCM', tipoCosto: 'Pago', url: null },
      ]),
      vincularCertificacion: () => throwError(() => new Error('No pudimos vincular la certificacion.')),
    });

    fixture = TestBed.createComponent(TopicWorkspacePage);
    fixture.detectChanges();
    const selects = fixture.nativeElement.querySelectorAll('.tool-picker select') as NodeListOf<HTMLSelectElement>;
    selects[1].value = 'certificacion-2';
    selects[1].dispatchEvent(new Event('change'));
    fixture.detectChanges();
    const botones = fixture.nativeElement.querySelectorAll('.tool-picker button') as NodeListOf<HTMLButtonElement>;
    botones[1].click();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('No pudimos vincular la certificacion.');
    expect(text).toContain('Sin certificaciones relacionadas.');
  });
});
