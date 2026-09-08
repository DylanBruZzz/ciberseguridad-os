import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NEVER, of, throwError } from 'rxjs';
import { provideRouter } from '@angular/router';
import { EvidenceDetail, EvidenceItemV1, NotaResumen } from '../../evidence/evidence.models';
import { EvidenceService } from '../../evidence/evidence.service';
import { PortafolioDto } from '../portfolio.models';
import { PortfolioService } from '../portfolio.service';
import { PortfolioPage } from './portfolio-page';

const usuarioId = '01a046d5-9bf3-7cec-aa05-10c93459fa16';
const temaId = '01a046d5-9bf3-7cec-aa05-10c93459fa17';
const temaDosId = '01a046d5-9bf3-7cec-aa05-10c93459fa18';
const proyectoId = '01a046d5-9bf3-7cec-aa05-10c93459fa19';
const laboratorioId = '01a046d5-9bf3-7cec-aa05-10c93459fa20';
const writeupId = '01a046d5-9bf3-7cec-aa05-10c93459fa21';
const artefactoId = '01a046d5-9bf3-7cec-aa05-10c93459fa22';
const certificacionObtenidaId = '01a046d5-9bf3-7cec-aa05-10c93459fa23';
const certificacionId = '01a046d5-9bf3-7cec-aa05-10c93459fa24';

@Component({ template: '' })
class EmptyRouteComponent {}

const portafolio: PortafolioDto = {
  usuarioId,
  resumen: {
    total: 5,
    proyectos: 1,
    laboratorios: 1,
    writeups: 1,
    artefactosTecnicos: 1,
    certificacionesObtenidas: 1,
    listosPortafolio: 3,
    publicados: 2,
  },
  proyectos: [
    {
      proyectoId,
      nombre: 'Proyecto SOC inicial',
      descripcion: 'Correlacion defensiva documentada',
      estado: 'Documentado',
      estadoMadurez: 'ListoPortafolio',
      repositorioUrl: 'https://github.com/example/soc',
      fechaInicio: '2026-09-01',
      fechaFin: null,
      temas: [{ temaId, nombre: 'Modelo OSI / TCP-IP' }],
      herramientas: [{ herramientaId: '01a046d5-9bf3-7cec-aa05-10c93459fa31', nombre: 'Wireshark', categoria: 'Red' }],
    },
  ],
  laboratorios: [
    {
      laboratorioId,
      nombre: 'Laboratorio Nmap',
      objetivo: 'Enumerar servicios',
      entornoVms: 'Kali',
      hallazgos: 'Puertos abiertos documentados',
      tiempoInvertidoMinutos: 45,
      fecha: '2026-09-03',
      estadoMadurez: 'Publicado',
      temas: [{ temaId, nombre: 'Modelo OSI / TCP-IP' }],
      herramientas: [{ herramientaId: '01a046d5-9bf3-7cec-aa05-10c93459fa32', nombre: 'Nmap', categoria: null }],
    },
  ],
  writeups: [
    {
      writeupId,
      titulo: 'Writeup HTB',
      plataformaOrigen: 'Hack The Box',
      url: 'https://example.com/writeup',
      fecha: '2026-09-02',
      estadoMadurez: 'Publicado',
      temas: [{ temaId: temaDosId, nombre: 'Bash scripting' }],
    },
  ],
  artefactosTecnicos: [
    {
      artefactoTecnicoId: artefactoId,
      tipoArtefacto: 'Script',
      nombre: 'Script hardening',
      contenidoOUrl: 'https://example.com/script',
      lenguajeTecnologia: 'PowerShell',
      estadoMadurez: 'ListoPortafolio',
      temas: [{ temaId, nombre: 'Modelo OSI / TCP-IP' }],
      herramientas: [{ herramientaId: '01a046d5-9bf3-7cec-aa05-10c93459fa33', nombre: 'PowerShell', categoria: 'Scripting' }],
    },
  ],
  certificacionesObtenidas: [
    {
      certificacionObtenidaId,
      certificacionId,
      nombre: 'Security+',
      proveedor: 'CompTIA',
      tipoCosto: 'Pago',
      fechaObtencion: '2026-09-01',
      evidenciaUrl: 'https://example.com/certificado',
      estadoMadurez: 'ListoPortafolio',
    },
  ],
};

const portafolioConEstadosNoElegibles: PortafolioDto = {
  ...portafolio,
  resumen: {
    ...portafolio.resumen,
    total: 7,
    proyectos: 2,
    laboratorios: 2,
  },
  proyectos: [
    ...portafolio.proyectos,
    {
      ...portafolio.proyectos[0],
      proyectoId: '01a046d5-9bf3-7cec-aa05-10c93459fa40',
      nombre: 'Proyecto borrador no elegible',
      estadoMadurez: 'Borrador',
    },
  ],
  laboratorios: [
    ...portafolio.laboratorios,
    {
      ...portafolio.laboratorios[0],
      laboratorioId: '01a046d5-9bf3-7cec-aa05-10c93459fa41',
      nombre: 'Laboratorio documentado no elegible',
      estadoMadurez: 'Documentado',
    },
  ],
};

const notas: NotaResumen[] = [
  {
    id: '01a046d5-9bf3-7cec-aa05-10c93459fa50',
    usuarioId,
    temaId: null,
    proyectoId,
    laboratorioId: null,
    writeupId: null,
    artefactoTecnicoId: null,
    fecha: '2026-09-04T00:00:00Z',
    texto: 'Nota factual del proyecto',
    tipo: 'Hallazgo',
  },
];

describe('PortfolioPage', () => {
  let fixture: ComponentFixture<PortfolioPage>;
  let portfolioService: {
    listar: ReturnType<typeof vi.fn>;
  };
  let evidence: {
    obtenerDetalle: ReturnType<typeof vi.fn>;
    listarNotas: ReturnType<typeof vi.fn>;
  };

  async function configure(datos: PortafolioDto = portafolio): Promise<void> {
    TestBed.resetTestingModule();

    portfolioService = {
      listar: vi.fn(() => of(datos)),
    };
    evidence = {
      obtenerDetalle: vi.fn((item: EvidenceItemV1) => of(detallePara(item))),
      listarNotas: vi.fn(() => of(notas)),
    };

    await TestBed.configureTestingModule({
      imports: [PortfolioPage],
      providers: [
        provideRouter([
          { path: 'evidence', component: EmptyRouteComponent },
          { path: 'roadmap/tema/:temaId', component: EmptyRouteComponent },
        ]),
        { provide: PortfolioService, useValue: portfolioService },
        { provide: EvidenceService, useValue: evidence },
      ],
    }).compileComponents();
  }

  it('muestra loading estructural', async () => {
    await configure();
    portfolioService.listar.mockReturnValue(NEVER);

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    expect(text()).toContain('Cargando Portfolio');
  });

  it('muestra empty Personal sin crear Portfolio', async () => {
    await configure(portafolioVacio());

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    expect(text()).toContain('Aun no tienes evidencias listas para Portfolio.');
    expect(text()).toContain("Cuando una evidencia alcance 'Listo para Portfolio' o 'Publicado'");
    expect(fixture.nativeElement.querySelector('a[href="/evidence"]')).not.toBeNull();
  });

  it('muestra lista curada separando Listo para Portfolio y Publicado', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    expect(text()).toContain('Read-side curado');
    expect(text()).toContain('Listo para Portfolio');
    expect(text()).toContain('Publicado');
    expect(listaText()).toContain('Proyecto SOC inicial');
    expect(listaText()).toContain('Laboratorio Nmap');
    expect(listaText()).toContain('Security+');
    expect(listaText()).toContain('Wireshark');
    expect(portfolioService.listar).toHaveBeenCalledWith({ tipoEvidence: '', estadoMadurez: '' });
  });

  it('muestra solo ListoPortafolio y Publicado si un mock trae estados no elegibles', async () => {
    await configure(portafolioConEstadosNoElegibles);

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    expect(listaText()).toContain('Proyecto SOC inicial');
    expect(listaText()).toContain('Laboratorio Nmap');
    expect(listaText()).not.toContain('Proyecto borrador no elegible');
    expect(listaText()).not.toContain('Laboratorio documentado no elegible');
    expect(listaText()).not.toContain('Borrador');
    expect(listaText()).not.toContain('Documentado');
  });

  it('compone filtros server-side de tipo y estado', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    cambiarSelect(0, 'ArtefactoTecnico');
    cambiarSelect(1, 'Publicado');

    expect(portfolioService.listar).toHaveBeenLastCalledWith({
      tipoEvidence: 'ArtefactoTecnico',
      estadoMadurez: 'Publicado',
    });
  });

  it('busca localmente por titulo tema y herramienta sin nuevas requests', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    cambiarInput('input[type="search"]', 'powershell');
    expect(listaText()).toContain('Script hardening');
    expect(listaText()).not.toContain('Proyecto SOC inicial');

    cambiarInput('input[type="search"]', 'bash');
    expect(listaText()).toContain('Writeup HTB');
    expect(listaText()).not.toContain('Script hardening');

    expect(portfolioService.listar).toHaveBeenCalledTimes(1);
  });

  it('muestra empty filtrado y limpia filtros', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    cambiarInput('input[type="search"]', 'no existe');
    expect(text()).toContain('No encontramos elementos con estos filtros.');

    clickPorTexto('Limpiar filtros');
    expect(listaText()).toContain('Proyecto SOC inicial');
    expect(portfolioService.listar).toHaveBeenCalledTimes(2);
  });

  it.each([
    ['Proyecto', 'Proyecto', 'Estado proyecto'],
    ['Laboratorio', 'Laboratorio', 'Hallazgos'],
    ['Writeup', 'Writeup', 'Plataforma'],
    ['ArtefactoTecnico', 'Artefacto tecnico', 'Tipo artefacto'],
    ['CertificacionObtenida', 'Certificacion obtenida', 'Fecha obtencion'],
  ] as const)('abre detail factual por AR %s', async (tipo, titulo, textoEsperado) => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    abrirItem(tipo);

    expect(evidence.obtenerDetalle).toHaveBeenCalledWith(
      expect.objectContaining({ tipoEvidence: tipo }),
    );
    expect(text()).toContain(titulo);
    expect(text()).toContain(textoEsperado);
    expect(text()).toContain('Detalle read-only');
  });

  it('mantiene detail read-only sin acciones de escritura', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    abrirItem('Proyecto');

    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(text()).not.toContain('Editar');
    expect(text()).not.toContain('Eliminar');
    expect(text()).not.toContain('Agregar nota');
    expect(text()).not.toContain('Publicar');
  });

  it('muestra notas read-only cuando el AR las soporta', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    abrirItem('Proyecto');

    expect(evidence.listarNotas).toHaveBeenCalled();
    expect(text()).toContain('Nota factual del proyecto');
    expect(text()).toContain('read-only');
  });

  it('no carga notas para CertificacionObtenida', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    abrirItem('CertificacionObtenida');

    expect(evidence.listarNotas).not.toHaveBeenCalled();
    expect(text()).not.toContain(certificacionId);
    expect(text()).not.toContain('Notas');
  });

  it('expone navegacion a Evidence y Tema sin deep-link nuevo', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    abrirItem('Proyecto');

    expect(fixture.nativeElement.querySelector('a[href="/evidence"]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector(`a[href="/roadmap/tema/${temaId}"]`)).not.toBeNull();
  });

  it('mantiene lista visible si falla detail y permite retry', async () => {
    await configure();
    evidence.obtenerDetalle.mockReturnValueOnce(throwError(() => new Error('No pudimos cargar el proyecto.')));

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    abrirItem('Proyecto');

    expect(text()).toContain('No pudimos abrir el detalle.');
    expect(listaText()).toContain('Proyecto SOC inicial');

    evidence.obtenerDetalle.mockReturnValueOnce(of(detallePara(evidence.obtenerDetalle.mock.calls[0][0])));
    clickPorTexto('Reintentar detalle');
    expect(text()).toContain('Proyecto defensivo');
  });

  it('muestra error de lista sin stacktrace', async () => {
    await configure();
    portfolioService.listar.mockReturnValue(throwError(() => new Error('No pudimos cargar Portfolio.')));

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    expect(text()).toContain('No pudimos cargar Portfolio.');
    expect(text()).not.toContain('Error:');
  });

  it('incluye labels accesibles de search y filtros', async () => {
    await configure();

    fixture = TestBed.createComponent(PortfolioPage);
    fixture.detectChanges();

    expect(toolbarText()).toContain('Buscar en Portfolio');
    expect(toolbarText()).toContain('Tipo');
    expect(toolbarText()).toContain('Estado');
    expect(fixture.nativeElement.querySelector('main[aria-label="Lista de Portfolio"]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('aside[aria-label="Detalle de Portfolio"]')).not.toBeNull();
  });

  function text(): string {
    return fixture.nativeElement.textContent;
  }

  function toolbarText(): string {
    return fixture.nativeElement.querySelector('.toolbar').textContent;
  }

  function listaText(): string {
    return fixture.nativeElement.querySelector('.list-panel').textContent;
  }

  function abrirItem(tipo: EvidenceItemV1['tipoEvidence']): void {
    const buttons = fixture.nativeElement.querySelectorAll('.portfolio-open') as NodeListOf<HTMLButtonElement>;
    const button = Array.from(buttons).find((candidate) => candidate.textContent?.includes(labelPorTipo(tipo)));
    button?.click();
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

  function clickPorTexto(label: string): void {
    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>).find(
      (candidate) => candidate.textContent?.includes(label),
    );
    button?.click();
    fixture.detectChanges();
  }
});

function portafolioVacio(): PortafolioDto {
  return {
    usuarioId,
    resumen: {
      total: 0,
      proyectos: 0,
      laboratorios: 0,
      writeups: 0,
      artefactosTecnicos: 0,
      certificacionesObtenidas: 0,
      listosPortafolio: 0,
      publicados: 0,
    },
    proyectos: [],
    laboratorios: [],
    writeups: [],
    artefactosTecnicos: [],
    certificacionesObtenidas: [],
  };
}

function labelPorTipo(tipo: EvidenceItemV1['tipoEvidence']): string {
  const labels: Record<EvidenceItemV1['tipoEvidence'], string> = {
    Proyecto: 'Proyecto SOC inicial',
    Laboratorio: 'Laboratorio Nmap',
    Writeup: 'Writeup HTB',
    ArtefactoTecnico: 'Script hardening',
    CertificacionObtenida: 'Security+',
  };

  return labels[tipo];
}

function detallePara(item: EvidenceItemV1): EvidenceDetail {
  switch (item.tipoEvidence) {
    case 'Proyecto':
      return {
        tipoEvidence: item.tipoEvidence,
        item,
        datos: {
          id: item.id,
          usuarioId,
          nombre: item.titulo,
          descripcion: 'Proyecto defensivo',
          estado: 'Documentado',
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
          usuarioId,
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
          usuarioId,
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
          usuarioId,
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
          usuarioId,
          certificacionId,
          fechaObtencion: '2026-09-01',
          evidenciaUrl: 'https://example.com/certificado',
          estadoMadurez: item.estadoMadurez,
        },
      };
  }
}
