import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, ParamMap, convertToParamMap, provideRouter } from '@angular/router';
import { BehaviorSubject, NEVER, Subject, of, throwError } from 'rxjs';
import { RoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';
import {
  CertificacionResumen,
  EvidenceDetail,
  EvidenceItemV1,
  NotaResumen,
} from '../evidence.models';
import { EvidenceService } from '../evidence.service';
import { EvidencePage } from './evidence-page';

const temaId = '01a046d5-9bf3-7cec-aa05-10c93459fa17';
const temaDosId = '01a046d5-9bf3-7cec-aa05-10c93459fa18';
const proyectoId = '01a046d5-9bf3-7cec-aa05-10c93459fa19';
const laboratorioId = '01a046d5-9bf3-7cec-aa05-10c93459fa20';
const writeupId = '01a046d5-9bf3-7cec-aa05-10c93459fa21';
const artefactoId = '01a046d5-9bf3-7cec-aa05-10c93459fa22';
const certificacionObtenidaId = '01a046d5-9bf3-7cec-aa05-10c93459fa23';
const certificacionId = '01a046d5-9bf3-7cec-aa05-10c93459fa24';
const nuevaEvidenceId = '01a046d5-9bf3-7cec-aa05-10c93459fa25';

@Component({ template: '' })
class EmptyRouteComponent {}

const vista: RoadmapVistaV1 = {
  faseActualId: '01a046d5-9bf3-7cec-aa05-10c93459fa30',
  progresoGlobalPorcentaje: 0,
  totalTemas: 2,
  temasDominados: 0,
  fases: [
    {
      id: '01a046d5-9bf3-7cec-aa05-10c93459fa30',
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
          faseId: '01a046d5-9bf3-7cec-aa05-10c93459fa30',
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
          faseId: '01a046d5-9bf3-7cec-aa05-10c93459fa30',
          temaPadreId: null,
          nombre: 'Bash scripting',
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

const items: EvidenceItemV1[] = [
  crearItem(proyectoId, 'Proyecto', 'Proyecto SOC inicial', 'Documentado', [temaId], ['Wireshark']),
  crearItem(laboratorioId, 'Laboratorio', 'Laboratorio Nmap', 'Borrador', [temaId], ['Nmap']),
  crearItem(writeupId, 'Writeup', 'Writeup HTB', 'Publicado', [temaDosId], []),
  crearItem(artefactoId, 'ArtefactoTecnico', 'Script hardening', 'ListoPortafolio', [temaId], ['PowerShell']),
  crearItem(certificacionObtenidaId, 'CertificacionObtenida', 'Security+', 'Documentado', [temaId], []),
];

const certificaciones: CertificacionResumen[] = [
  {
    id: certificacionId,
    nombre: 'Security+',
    proveedor: 'CompTIA',
    tipoCosto: 'Pago',
    url: null,
  },
];

const notas: NotaResumen[] = [
  {
    id: '01a046d5-9bf3-7cec-aa05-10c93459fa40',
    usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
    temaId: null,
    proyectoId,
    laboratorioId: null,
    writeupId: null,
    artefactoTecnicoId: null,
    fecha: '2026-09-04T00:00:00Z',
    texto: 'Hallazgo del proyecto',
    tipo: 'Hallazgo',
  },
];

describe('EvidencePage', () => {
  let fixture: ComponentFixture<EvidencePage>;
  let queryParamMap: BehaviorSubject<ParamMap>;
  let evidence: {
    listar: ReturnType<typeof vi.fn>;
    obtenerDetalle: ReturnType<typeof vi.fn>;
    crear: ReturnType<typeof vi.fn>;
    actualizar: ReturnType<typeof vi.fn>;
    eliminar: ReturnType<typeof vi.fn>;
    vincularTema: ReturnType<typeof vi.fn>;
    listarNotas: ReturnType<typeof vi.fn>;
    agregarNota: ReturnType<typeof vi.fn>;
    listarCertificaciones: ReturnType<typeof vi.fn>;
  };
  let roadmap: {
    obtenerVista: ReturnType<typeof vi.fn>;
    refrescarVista: ReturnType<typeof vi.fn>;
  };

  type QueryParams = string | null | Record<string, string>;

  async function configure(query: QueryParams = null, lista: EvidenceItemV1[] = items): Promise<void> {
    TestBed.resetTestingModule();
    queryParamMap = new BehaviorSubject(convertToParamMap(parametros(query)));

    evidence = {
      listar: vi.fn(() => of({ total: lista.length, items: lista })),
      obtenerDetalle: vi.fn((item: EvidenceItemV1) => of(detallePara(item))),
      crear: vi.fn(() => of({ id: nuevaEvidenceId })),
      actualizar: vi.fn(() => of(undefined)),
      eliminar: vi.fn(() => of(undefined)),
      vincularTema: vi.fn(() => of(undefined)),
      listarNotas: vi.fn(() => of(notas)),
      agregarNota: vi.fn(() => of(undefined)),
      listarCertificaciones: vi.fn(() => of(certificaciones)),
    };
    roadmap = {
      obtenerVista: vi.fn(() => of(vista)),
      refrescarVista: vi.fn(() => of(vista)),
    };

    await TestBed.configureTestingModule({
      imports: [EvidencePage],
      providers: [
        provideRouter([
          { path: 'roadmap/tema/:temaId', component: EmptyRouteComponent },
          { path: 'portfolio', component: EmptyRouteComponent },
        ]),
        {
          provide: ActivatedRoute,
          useValue: {
            queryParamMap: queryParamMap.asObservable(),
          },
        },
        { provide: EvidenceService, useValue: evidence },
        { provide: RoadmapService, useValue: roadmap },
      ],
    }).compileComponents();
  }

  function parametros(query: QueryParams): Record<string, string> {
    if (query === null) return {};
    return typeof query === 'string' ? { temaId: query } : query;
  }

  it('muestra loading estructural', async () => {
    await configure();
    evidence.listar.mockReturnValue(NEVER);

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    expect(text()).toContain('Cargando Evidence');
  });

  it('mantiene estado de lista controlado ante error del unified read', async () => {
    await configure();
    evidence.listar.mockReturnValue(throwError(() => new Error('No pudimos cargar Evidence.')));

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    expect(text()).toContain('No pudimos cargar Evidence.');
    expect(text()).not.toContain('Error:');
  });

  it('muestra Evidence global con lista compacta y relaciones factuales', async () => {
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    expect(text()).toContain('Evidence global');
    expect(text()).toContain('Proyecto SOC inicial');
    expect(text()).toContain('Listo para Portfolio');
    expect(text()).toContain('Modelo OSI / TCP-IP');
    expect(text()).toContain('Wireshark');
    expect(evidence.listar).toHaveBeenCalledWith({
      temaId: null,
      tipoEvidence: '',
      estadoMadurez: '',
    });
  });

  it('usa modo contextual con temaId y link de vuelta al Workspace', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    expect(evidence.listar).toHaveBeenCalledWith({
      temaId,
      tipoEvidence: '',
      estadoMadurez: '',
    });
    expect(text()).toContain('Evidence de Modelo OSI / TCP-IP');
    expect(fixture.nativeElement.querySelector(`a[href="/roadmap/tema/${temaId}"]`)).not.toBeNull();
  });

  it('ignora temaId malformado', async () => {
    await configure('tema-malformado');

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    expect(text()).toContain('Contexto no disponible.');
    expect(evidence.listar).toHaveBeenCalledWith({
      temaId: null,
      tipoEvidence: '',
      estadoMadurez: '',
    });
  });

  it('compone filtros server-side sin perder temaId', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    cambiarSelect(0, 'Laboratorio');
    cambiarSelect(1, 'Documentado');

    expect(evidence.listar).toHaveBeenLastCalledWith({
      temaId,
      tipoEvidence: 'Laboratorio',
      estadoMadurez: 'Documentado',
    });
  });

  it('busca localmente sin nuevas requests', async () => {
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    cambiarInput('input[type="search"]', 'hardening');

    expect(listaText()).toContain('Script hardening');
    expect(listaText()).not.toContain('Proyecto SOC inicial');
    expect(evidence.listar).toHaveBeenCalledTimes(1);
  });

  it('diferencia empty global, contextual y filtrado', async () => {
    await configure(null, []);

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();
    expect(text()).toContain('Aun no has registrado evidencias.');

    await configure(temaId, []);
    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();
    expect(text()).toContain('Aun no hay evidencias vinculadas a este tema.');

    await configure();
    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();
    cambiarInput('input[type="search"]', 'no existe');
    expect(text()).toContain('No encontramos evidencias con estos filtros.');
  });

  it.each([
    ['Proyecto', 'Proyecto', 'Estado'],
    ['Laboratorio', 'Laboratorio', 'Hallazgos'],
    ['Writeup', 'Writeup', 'Plataforma'],
    ['ArtefactoTecnico', 'Artefacto tecnico', 'Tipo artefacto'],
    ['CertificacionObtenida', 'Certificacion obtenida', 'Fecha obtencion'],
  ] as const)('abre detail por tipo %s', async (tipo, titulo, textoEsperado) => {
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem(tipo);

    expect(evidence.obtenerDetalle).toHaveBeenCalledWith(items.find((item) => item.tipoEvidence === tipo));
    expect(text()).toContain(titulo);
    expect(text()).toContain(textoEsperado);
  });

  it('abre detail desde evidenceId + tipoEvidence factual sin filtro stale', async () => {
    await configure({ evidenceId: laboratorioId, tipoEvidence: 'Laboratorio' });

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    expect(evidence.listar).toHaveBeenCalledWith({
      temaId: null,
      tipoEvidence: '',
      estadoMadurez: '',
    });
    expect(evidence.obtenerDetalle).toHaveBeenCalledWith(items.find((item) => item.id === laboratorioId));
    expect(text()).toContain('Laboratorio Nmap');
    expect(text()).toContain('Hallazgos');
  });

  it('mantiene evidenceId + tipoEvidence al reintentar la lista', async () => {
    await configure({ evidenceId: laboratorioId, tipoEvidence: 'Laboratorio' });
    evidence.listar
      .mockReturnValueOnce(throwError(() => new Error('No pudimos cargar Evidence.')))
      .mockReturnValueOnce(of({ total: items.length, items }));

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();
    expect(text()).toContain('No pudimos cargar Evidence.');

    clickPorTexto('Reintentar');

    expect(evidence.listar).toHaveBeenCalledTimes(2);
    expect(evidence.obtenerDetalle).toHaveBeenCalledWith(items.find((item) => item.id === laboratorioId));
  });

  it('cancela detail anterior cuando cambia evidenceId en la misma ruta', async () => {
    await configure();
    const detallePendiente = new Subject<EvidenceDetail>();
    evidence.obtenerDetalle.mockImplementation((item: EvidenceItemV1) =>
      item.tipoEvidence === 'Proyecto' ? detallePendiente.asObservable() : of(detallePara(item)),
    );

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();
    abrirItem('Proyecto');

    queryParamMap.next(convertToParamMap({ evidenceId: laboratorioId, tipoEvidence: 'Laboratorio' }));
    fixture.detectChanges();
    detallePendiente.next(detallePara(items[0]));
    detallePendiente.complete();
    fixture.detectChanges();

    expect(evidence.obtenerDetalle).toHaveBeenLastCalledWith(items.find((item) => item.id === laboratorioId));
    expect(text()).toContain('Laboratorio Nmap');
    expect(text()).not.toContain('Proyecto defensivo');
  });

  it('rechaza deep links invalidos o con tipo incorrecto sin destruir lista', async () => {
    await configure({ evidenceId: laboratorioId, tipoEvidence: 'Proyecto' });

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    expect(evidence.obtenerDetalle).not.toHaveBeenCalled();
    expect(text()).toContain('La Evidence del enlace ya no esta disponible en esta lista.');
    expect(listaText()).toContain('Laboratorio Nmap');

    queryParamMap.next(convertToParamMap({ evidenceId: 'id-invalido', tipoEvidence: 'Laboratorio' }));
    fixture.detectChanges();

    expect(text()).toContain('El enlace de Evidence no es valido.');
    expect(listaText()).toContain('Laboratorio Nmap');
  });

  it('carga y agrega notas append-only para Proyecto', async () => {
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('Proyecto');
    expect(evidence.listarNotas).toHaveBeenCalled();
    expect(text()).toContain('Hallazgo del proyecto');

    cambiarTextareaPorLabel('Agregar nota', 'Nueva observacion factual');
    clickPorTexto('Agregar nota');

    expect(evidence.agregarNota).toHaveBeenCalledWith('Proyecto', proyectoId, {
      texto: 'Nueva observacion factual',
      tipo: 'Nota',
    });
    expect(text()).toContain('Nota agregada.');
  });

  it('conserva historial visible si falla append de Nota', async () => {
    await configure();
    evidence.agregarNota.mockReturnValueOnce(throwError(() => new Error('No pudimos agregar la nota.')));

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('Proyecto');
    cambiarTextareaPorLabel('Agregar nota', 'Nueva observacion factual');
    clickPorTexto('Agregar nota');

    expect(text()).toContain('Hallazgo del proyecto');
    expect(text()).toContain('No pudimos agregar la nota.');
    expect(text()).not.toContain('Editar nota');
    expect(text()).not.toContain('Eliminar nota');
  });

  it('no ofrece notas para CertificacionObtenida', async () => {
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('CertificacionObtenida');

    expect(evidence.listarNotas).not.toHaveBeenCalled();
    expect(text()).not.toContain('append-only');
  });

  it('valida create y crea Proyecto contextual con vinculo factual', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    clickPorTexto('+ Nueva evidencia');
    enviarFormulario();

    expect(evidence.crear).not.toHaveBeenCalled();
    expect(text()).toContain('Ingresa un nombre.');

    cambiarInputPorLabel('Nombre', 'Prueba integracion Evidence Frontend V1');
    enviarFormulario();

    expect(evidence.crear).toHaveBeenCalledWith({
      tipoEvidence: 'Proyecto',
      payload: { nombre: 'Prueba integracion Evidence Frontend V1' },
    });
    expect(evidence.vincularTema).toHaveBeenCalledWith('Proyecto', nuevaEvidenceId, temaId);
  });

  it('mantiene lista actual si falla create', async () => {
    await configure();
    evidence.crear.mockReturnValueOnce(throwError(() => new Error('No pudimos crear la evidencia.')));

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    clickPorTexto('+ Nueva evidencia');
    cambiarInputPorLabel('Nombre', 'Proyecto no persistido');
    enviarFormulario();

    expect(text()).toContain('No pudimos crear la evidencia.');
    expect(listaText()).toContain('Proyecto SOC inicial');
    expect(evidence.vincularTema).not.toHaveBeenCalled();
  });

  it('crea CertificacionObtenida sin inventar vinculo a Tema', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    clickPorTexto('+ Nueva evidencia');
    cambiarSelect(2, 'CertificacionObtenida');
    enviarFormulario();

    expect(evidence.crear).toHaveBeenCalledWith({
      tipoEvidence: 'CertificacionObtenida',
      payload: {
        certificacionId,
        fechaObtencion: expect.stringMatching(/^\d{4}-\d{2}-\d{2}$/),
      },
    });
    expect(evidence.vincularTema).not.toHaveBeenCalled();
  });

  it('edita Laboratorio con payload especifico', async () => {
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('Laboratorio');
    clickPorTexto('Editar');

    cambiarInputPorLabel('Nombre', 'Laboratorio Nmap editado');
    cambiarTextareaPorLabel('Hallazgos', 'Hallazgos editados');
    cambiarInputPorLabel('Minutos', '90');
    cambiarSelectPorLabel('Estado madurez', 'ListoPortafolio');
    enviarFormulario();

    expect(evidence.actualizar).toHaveBeenCalledWith({
      tipoEvidence: 'Laboratorio',
      id: laboratorioId,
      payload: {
        nombre: 'Laboratorio Nmap editado',
        objetivo: 'Enumerar red local',
        entornoVms: 'Kali',
        hallazgos: 'Hallazgos editados',
        tiempoInvertidoMinutos: 90,
        fecha: '2026-09-03',
        estadoMadurez: 'ListoPortafolio',
      },
    });
  });

  it('reabre detail desde el unified read refrescado despues de update', async () => {
    const laboratorioActualizado = crearItem(
      laboratorioId,
      'Laboratorio',
      'Laboratorio Nmap actualizado',
      'ListoPortafolio',
      [temaId],
      ['Nmap'],
    );
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('Laboratorio');
    clickPorTexto('Editar');
    evidence.listar.mockReturnValueOnce(of({ total: items.length, items: [items[0], laboratorioActualizado, ...items.slice(2)] }));
    cambiarInputPorLabel('Nombre', 'Laboratorio Nmap actualizado');
    cambiarSelectPorLabel('Estado madurez', 'ListoPortafolio');
    enviarFormulario();

    expect(evidence.obtenerDetalle).toHaveBeenLastCalledWith(laboratorioActualizado);
    expect(text()).toContain('Laboratorio Nmap actualizado');
    expect(text()).toContain('Listo para Portfolio');
  });

  it('mantiene detail y lista si falla update', async () => {
    await configure();
    evidence.actualizar.mockReturnValueOnce(throwError(() => new Error('No pudimos guardar cambios.')));

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('Laboratorio');
    clickPorTexto('Editar');
    cambiarInputPorLabel('Nombre', 'Laboratorio no persistido');
    enviarFormulario();

    expect(text()).toContain('No pudimos guardar cambios.');
    expect(listaText()).toContain('Laboratorio Nmap');
    expect(evidence.listar).toHaveBeenCalledTimes(1);
  });

  it('confirma delete y refresca lista sin cerrar con error global', async () => {
    await configure();

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('Proyecto');
    clickPorTexto('Eliminar');
    expect(text()).toContain('Eliminar esta evidencia?');
    expect(text()).toContain('Dejara de aparecer en Evidence y, si correspondia, en Portfolio.');

    clickUltimoPorTexto('Eliminar');

    expect(evidence.eliminar).toHaveBeenCalledWith('Proyecto', proyectoId);
    expect(evidence.listar).toHaveBeenCalledTimes(2);
    expect(roadmap.refrescarVista).toHaveBeenCalled();
  });

  it('muestra errores de detail y delete de forma separada', async () => {
    await configure();
    evidence.obtenerDetalle.mockReturnValueOnce(throwError(() => new Error('No pudimos cargar el proyecto.')));

    fixture = TestBed.createComponent(EvidencePage);
    fixture.detectChanges();

    abrirItem('Proyecto');
    expect(text()).toContain('No pudimos abrir la evidencia.');
    expect(listaText()).toContain('Proyecto SOC inicial');

    evidence.obtenerDetalle.mockReturnValueOnce(of(detallePara(items[0])));
    evidence.eliminar.mockReturnValueOnce(throwError(() => new Error('No pudimos eliminar la evidencia.')));
    abrirItem('Proyecto');
    clickPorTexto('Eliminar');
    clickUltimoPorTexto('Eliminar');

    expect(text()).toContain('No pudimos eliminar la evidencia.');
  });

  function text(): string {
    return fixture.nativeElement.textContent;
  }

  function listaText(): string {
    return fixture.nativeElement.querySelector('.list-panel').textContent;
  }

  function abrirItem(tipo: EvidenceItemV1['tipoEvidence']): void {
    const index = items.findIndex((item) => item.tipoEvidence === tipo);
    const buttons = fixture.nativeElement.querySelectorAll('.evidence-open') as NodeListOf<HTMLButtonElement>;
    buttons[index].click();
    fixture.detectChanges();
  }

  function enviarFormulario(): void {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

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

  function cambiarInputPorLabel(label: string, value: string): void {
    const labelElement = labelPorTexto(label);
    const input = labelElement.querySelector('input') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function cambiarTextareaPorLabel(label: string, value: string): void {
    const labelElement = labelPorTexto(label);
    const textarea = labelElement.querySelector('textarea') as HTMLTextAreaElement;
    textarea.value = value;
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function cambiarSelectPorLabel(label: string, value: string): void {
    const labelElement = labelPorTexto(label);
    const select = labelElement.querySelector('select') as HTMLSelectElement;
    select.value = value;
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }

  function labelPorTexto(label: string): HTMLLabelElement {
    return Array.from(fixture.nativeElement.querySelectorAll('label') as NodeListOf<HTMLLabelElement>).find(
      (candidate) => candidate.textContent?.includes(label),
    )!;
  }

  function clickPorTexto(label: string): void {
    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>).find(
      (candidate) => candidate.textContent?.includes(label),
    );
    button?.click();
    fixture.detectChanges();
  }

  function clickUltimoPorTexto(label: string): void {
    const buttons = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).filter((candidate) => candidate.textContent?.includes(label));
    buttons[buttons.length - 1].click();
    fixture.detectChanges();
  }
});

function crearItem(
  id: string,
  tipoEvidence: EvidenceItemV1['tipoEvidence'],
  titulo: string,
  estadoMadurez: EvidenceItemV1['estadoMadurez'],
  temas: string[],
  herramientas: string[],
): EvidenceItemV1 {
  return {
    id,
    tipoEvidence,
    titulo,
    estadoMadurez,
    fechaCreacionUtc: '2026-09-01T00:00:00Z',
    fechaModificacionUtc: '2026-09-02T00:00:00Z',
    fechaActividadUtc: '2026-09-02T00:00:00Z',
    fechaReferencia: '2026-09-01',
    temas: temas.map((tema) => ({
      id: tema,
      nombre: tema === temaId ? 'Modelo OSI / TCP-IP' : 'Bash scripting',
    })),
    herramientas: herramientas.map((nombre, index) => ({
      id: `01a046d5-9bf3-7cec-aa05-10c93459fa5${index}`,
      nombre,
    })),
  };
}

function detallePara(item: EvidenceItemV1): EvidenceDetail {
  switch (item.tipoEvidence) {
    case 'Proyecto':
      return {
        tipoEvidence: item.tipoEvidence,
        item,
        datos: {
          id: item.id,
          usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
          nombre: item.titulo,
          descripcion: 'Proyecto defensivo',
          estado: 'Desarrollo',
          estadoMadurez: item.estadoMadurez,
          repositorioUrl: 'https://github.com/example/soc',
          fechaInicio: '2026-09-01',
          fechaFin: null,
        },
      };
    case 'Laboratorio':
      return {
        tipoEvidence: item.tipoEvidence,
        item,
        datos: {
          id: item.id,
          usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
          nombre: item.titulo,
          objetivo: 'Enumerar red local',
          entornoVms: 'Kali',
          hallazgos: 'Puertos abiertos documentados',
          tiempoInvertidoMinutos: 45,
          estadoMadurez: item.estadoMadurez,
          fecha: '2026-09-03',
        },
      };
    case 'Writeup':
      return {
        tipoEvidence: item.tipoEvidence,
        item,
        datos: {
          id: item.id,
          usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
          titulo: item.titulo,
          plataformaOrigen: 'Hack The Box',
          url: 'https://example.com/writeup',
          estadoMadurez: item.estadoMadurez,
          fecha: '2026-09-02',
        },
      };
    case 'ArtefactoTecnico':
      return {
        tipoEvidence: item.tipoEvidence,
        item,
        datos: {
          id: item.id,
          usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
          tipoArtefacto: 'Script',
          nombre: item.titulo,
          contenidoOUrl: 'https://example.com/script',
          lenguajeTecnologia: 'PowerShell',
          estadoMadurez: item.estadoMadurez,
        },
      };
    case 'CertificacionObtenida':
      return {
        tipoEvidence: item.tipoEvidence,
        item,
        datos: {
          id: item.id,
          usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
          certificacionId,
          fechaObtencion: '2026-09-01',
          evidenciaUrl: 'https://example.com/certificado',
          estadoMadurez: item.estadoMadurez,
        },
      };
  }
}
