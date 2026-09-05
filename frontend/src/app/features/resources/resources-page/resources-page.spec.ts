import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { NEVER, of, throwError } from 'rxjs';
import { RoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';
import { RecursoDetalle, RecursoResumen } from '../resources.models';
import { ResourcesService } from '../resources.service';
import { ResourcesPage } from './resources-page';

const temaId = '01a046d5-9bf3-7cec-aa05-10c93459fa17';
const temaDosId = '01a046d5-9bf3-7cec-aa05-10c93459fa18';
const recursoId = '01a046d5-9bf3-7cec-aa05-10c93459fa19';
const recursoDosId = '01a046d5-9bf3-7cec-aa05-10c93459fa20';
const recursoTresId = '01a046d5-9bf3-7cec-aa05-10c93459fa21';
const nuevoRecursoId = '01a046d5-9bf3-7cec-aa05-10c93459fa22';

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

const recursos: RecursoResumen[] = [
  {
    id: recursoId,
    usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
    tipo: 'Documentacion',
    titulo: 'Cisco OSI Guide',
    url: 'https://docs.cisco.com/osi',
    estado: 'EnUso',
    temas: [{ id: temaId, nombre: 'Modelo OSI / TCP-IP' }],
  },
  {
    id: recursoDosId,
    usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
    tipo: 'Curso',
    titulo: 'Linux CLI Lab',
    url: 'https://example.com/linux-lab',
    estado: 'Consultado',
    temas: [],
  },
  {
    id: recursoTresId,
    usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
    tipo: 'RepositorioGitHub',
    titulo: 'TCP/IP examples',
    url: 'https://github.com/example/tcpip',
    estado: 'Referencia',
    temas: [
      { id: temaId, nombre: 'Modelo OSI / TCP-IP' },
      { id: temaDosId, nombre: 'Bash scripting' },
    ],
  },
];

const detalle: RecursoDetalle = {
  ...recursos[0],
  rating: 4,
  notas: 'Usar para contrastar capas.',
  herramientaIA: 'ChatGPT',
  promptsUtilizados: 'Resume las diferencias entre capas.',
};

const detalleSinRating: RecursoDetalle = {
  ...recursos[1],
  rating: null,
  notas: null,
  herramientaIA: null,
  promptsUtilizados: null,
};

describe('ResourcesPage', () => {
  let fixture: ComponentFixture<ResourcesPage>;
  let resourcesService: {
    listar: ReturnType<typeof vi.fn>;
    obtenerDetalle: ReturnType<typeof vi.fn>;
    crear: ReturnType<typeof vi.fn>;
    actualizar: ReturnType<typeof vi.fn>;
    eliminar: ReturnType<typeof vi.fn>;
    vincularTema: ReturnType<typeof vi.fn>;
  };
  let roadmap: {
    obtenerVista: ReturnType<typeof vi.fn>;
    refrescarVista: ReturnType<typeof vi.fn>;
  };

  async function configure(queryTemaId: string | null = null, lista: RecursoResumen[] = recursos): Promise<void> {
    TestBed.resetTestingModule();

    resourcesService = {
      listar: vi.fn(() => of(lista)),
      obtenerDetalle: vi.fn((id: string) => of(id === recursoDosId ? detalleSinRating : detalle)),
      crear: vi.fn(() =>
        of({
          id: nuevoRecursoId,
          usuarioId: recursos[0].usuarioId,
          tipo: 'Documentacion',
          titulo: 'Prueba Resources',
          url: 'https://example.com/resources-v1',
          estado: 'PorClasificar',
        }),
      ),
      actualizar: vi.fn(() => of(undefined)),
      eliminar: vi.fn(() => of(undefined)),
      vincularTema: vi.fn(() => of(undefined)),
    };
    roadmap = {
      obtenerVista: vi.fn(() => of(vista)),
      refrescarVista: vi.fn(() => of(vista)),
    };

    await TestBed.configureTestingModule({
      imports: [ResourcesPage],
      providers: [
        provideRouter([{ path: 'roadmap/tema/:temaId', component: EmptyRouteComponent }]),
        {
          provide: ActivatedRoute,
          useValue: {
            queryParamMap: of(convertToParamMap(queryTemaId ? { temaId: queryTemaId } : {})),
          },
        },
        { provide: ResourcesService, useValue: resourcesService },
        { provide: RoadmapService, useValue: roadmap },
      ],
    }).compileComponents();
  }

  it('muestra loading estructural', async () => {
    await configure();
    resourcesService.listar.mockReturnValue(NEVER);

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    expect(text()).toContain('Cargando Resources');
  });

  it('muestra biblioteca global como lista enriquecida sin filtro rating', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    expect(text()).toContain('Resources');
    expect(text()).toContain('Biblioteca global');
    expect(text()).toContain('Cisco OSI Guide');
    expect(text()).toContain('docs.cisco.com');
    expect(text()).toContain('Modelo OSI / TCP-IP');
    expect(text()).toContain('Sin tema vinculado');
    expect(toolbarText()).not.toContain('Rating');
    expect(resourcesService.listar).toHaveBeenCalledWith(null);
  });

  it('muestra modo contextual y consulta backend con temaId', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    expect(resourcesService.listar).toHaveBeenCalledWith(temaId);
    expect(text()).toContain('Recursos de Modelo OSI / TCP-IP');
    expect(text()).toContain('Biblioteca contextual');
    expect(fixture.nativeElement.querySelector(`a[href="/roadmap/tema/${temaId}"]`)).not.toBeNull();
  });

  it('ignora temaId malformado y no lo envia al service', async () => {
    await configure('tema-malformado');

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    expect(resourcesService.listar).toHaveBeenCalledWith(null);
    expect(text()).toContain('Contexto no disponible.');
    expect(text()).toContain('biblioteca global');
  });

  it('diferencia empty global y contextual', async () => {
    await configure(null, []);

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    expect(text()).toContain('Aun no tienes recursos en tu biblioteca.');

    await configure(temaId, []);

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    expect(text()).toContain('Aun no hay recursos vinculados a este tema.');
  });

  it('compone search local con filtros de estado y tipo', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    cambiarInput('input[type="search"]', 'tcpip');
    cambiarSelect(0, 'Referencia');
    cambiarSelect(1, 'RepositorioGitHub');

    expect(listaText()).toContain('TCP/IP examples');
    expect(listaText()).not.toContain('Cisco OSI Guide');
    expect(listaText()).not.toContain('Linux CLI Lab');
    expect(resourcesService.listar).toHaveBeenCalledTimes(1);
  });

  it('filtra por titulo, URL, estado y tipo por separado', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    cambiarInput('input[type="search"]', 'cisco');
    expect(listaText()).toContain('Cisco OSI Guide');
    expect(listaText()).not.toContain('Linux CLI Lab');

    fixture.nativeElement.querySelector('.ghost-button').click();
    fixture.detectChanges();
    cambiarInput('input[type="search"]', 'linux-lab');
    expect(listaText()).toContain('Linux CLI Lab');

    cambiarInput('input[type="search"]', '');
    cambiarSelect(0, 'EnUso');
    expect(listaText()).toContain('Cisco OSI Guide');
    expect(listaText()).not.toContain('TCP/IP examples');

    cambiarSelect(0, '');
    cambiarSelect(1, 'Curso');
    expect(listaText()).toContain('Linux CLI Lab');
    expect(listaText()).not.toContain('Cisco OSI Guide');
  });

  it('muestra empty filtrado y permite limpiar filtros sin tocar temaId', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    cambiarInput('input[type="search"]', 'no existe');

    expect(text()).toContain('No encontramos recursos con estos filtros.');

    const limpiar = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>).find(
      (button) => button.textContent?.includes('Limpiar filtros'),
    )!;
    limpiar.click();
    fixture.detectChanges();

    expect(resourcesService.listar).toHaveBeenCalledWith(temaId);
    expect(listaText()).toContain('Cisco OSI Guide');
  });

  it('abre detail con loading aislado y muestra datos enriquecidos', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirPrimerRecurso();
    fixture.detectChanges();

    expect(resourcesService.obtenerDetalle).toHaveBeenCalledWith(recursoId);
    expect(text()).toContain('Usar para contrastar capas.');
    expect(text()).toContain('ChatGPT');
    expect(text()).toContain('Resume las diferencias entre capas.');
    expect(text()).toContain('4/5');
    expect(fixture.nativeElement.querySelector('a[target="_blank"][rel="noopener noreferrer"]')).not.toBeNull();
    expect(listaText()).toContain('Cisco OSI Guide');
  });

  it('muestra error de detail sin destruir la lista', async () => {
    await configure();
    resourcesService.obtenerDetalle.mockReturnValue(throwError(() => new Error('No pudimos cargar el recurso.')));

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirPrimerRecurso();
    fixture.detectChanges();

    expect(text()).toContain('No pudimos abrir el recurso.');
    expect(listaText()).toContain('Cisco OSI Guide');
  });

  it('valida create antes de enviar', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirCrear();
    enviarFormulario();

    expect(resourcesService.crear).not.toHaveBeenCalled();
    expect(text()).toContain('Ingresa un titulo.');
  });

  it('crea recurso global, refresca lista y abre detail cuando hay id', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirCrear();
    cambiarInput('input[type="text"]', 'Prueba Resources');
    cambiarInput('input[type="url"]', 'https://example.com/resources-v1');
    enviarFormulario();

    expect(resourcesService.crear).toHaveBeenCalledWith({
      tipo: 'Documentacion',
      titulo: 'Prueba Resources',
      url: 'https://example.com/resources-v1',
    });
    expect(resourcesService.vincularTema).not.toHaveBeenCalled();
    expect(resourcesService.listar).toHaveBeenCalledTimes(2);
    expect(resourcesService.obtenerDetalle).toHaveBeenCalledWith(nuevoRecursoId);
  });

  it('crea recurso contextual y vincula tema factual', async () => {
    await configure(temaId);

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirCrear();
    expect(text()).toContain('Se vinculara a Modelo OSI / TCP-IP.');
    cambiarInput('input[type="text"]', 'Prueba Resources');
    cambiarInput('input[type="url"]', 'https://example.com/resources-v1');
    enviarFormulario();

    expect(resourcesService.crear).toHaveBeenCalled();
    expect(resourcesService.vincularTema).toHaveBeenCalledWith(nuevoRecursoId, temaId);
    expect(resourcesService.listar).toHaveBeenCalledTimes(2);
  });

  it('muestra fallo parcial si create contextual no puede vincular', async () => {
    await configure(temaId);
    resourcesService.vincularTema.mockReturnValue(
      throwError(() => new Error('No pudimos vincular el recurso al tema.')),
    );

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirCrear();
    cambiarInput('input[type="text"]', 'Prueba Resources');
    enviarFormulario();
    fixture.detectChanges();

    expect(text()).toContain('El recurso fue creado, pero no se pudo vincular al tema.');
    expect(resourcesService.obtenerDetalle).toHaveBeenCalledWith(nuevoRecursoId);
  });

  it('edita solo campos soportados y mantiene tipo immutable', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirPrimerRecurso();
    clickPorTexto('Editar');
    fixture.detectChanges();

    expect(text()).toContain('El tipo se define al crear el recurso.');

    cambiarInput('input[type="text"]', 'Cisco OSI Guide editado');
    cambiarInput('input[type="url"]', 'https://docs.cisco.com/osi-editado');
    cambiarSelect(2, 'EnUso');
    cambiarSelect(3, '5');
    cambiarTextarea(0, 'Notas Resource editadas');
    cambiarTextInput(1, 'Claude');
    cambiarTextarea(1, 'Prompt editado');
    enviarFormulario();

    expect(resourcesService.actualizar).toHaveBeenCalledWith(recursoId, {
      titulo: 'Cisco OSI Guide editado',
      url: 'https://docs.cisco.com/osi-editado',
      estado: 'EnUso',
      rating: 5,
      notas: 'Notas Resource editadas',
      herramientaIA: 'Claude',
      promptsUtilizados: 'Prompt editado',
    });
  });

  it('permite rating null y opciones 1..5', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirRecurso(1);
    clickPorTexto('Editar');
    fixture.detectChanges();

    const ratingSelect = fixture.nativeElement.querySelectorAll('select')[3] as HTMLSelectElement;
    const options = Array.from(ratingSelect.querySelectorAll('option')).map((option) => option.value);
    expect(options).toEqual(['', '1', '2', '3', '4', '5']);

    enviarFormulario();
    expect(resourcesService.actualizar).toHaveBeenCalledWith(
      recursoDosId,
      expect.objectContaining({ rating: null }),
    );
  });

  it('muestra error de update y conserva detail', async () => {
    await configure();
    resourcesService.actualizar.mockReturnValue(throwError(() => new Error('No pudimos actualizar el recurso.')));

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirPrimerRecurso();
    clickPorTexto('Editar');
    enviarFormulario();

    expect(text()).toContain('No pudimos actualizar el recurso.');
    expect(text()).toContain('Cisco OSI Guide');
  });

  it('confirma eliminacion y ejecuta soft delete por API', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirPrimerRecurso();
    clickPorTexto('Eliminar');
    fixture.detectChanges();

    expect(text()).toContain('Eliminar este recurso?');
    expect(text()).toContain('Dejara de aparecer en tu biblioteca.');

    clickUltimoPorTexto('Eliminar');
    fixture.detectChanges();

    expect(resourcesService.eliminar).toHaveBeenCalledWith(recursoId);
    expect(resourcesService.listar).toHaveBeenCalledTimes(2);
  });

  it('muestra error de delete separado', async () => {
    await configure();
    resourcesService.eliminar.mockReturnValue(throwError(() => new Error('No pudimos eliminar el recurso.')));

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    abrirPrimerRecurso();
    clickPorTexto('Eliminar');
    clickUltimoPorTexto('Eliminar');
    fixture.detectChanges();

    expect(text()).toContain('No pudimos eliminar el recurso.');
  });

  it('preserva filtros locales despues de writes', async () => {
    await configure();

    fixture = TestBed.createComponent(ResourcesPage);
    fixture.detectChanges();

    cambiarInput('input[type="search"]', 'Cisco');
    abrirPrimerRecurso();
    clickPorTexto('Editar');
    enviarFormulario();
    fixture.detectChanges();

    const search = fixture.nativeElement.querySelector('input[type="search"]') as HTMLInputElement;
    expect(search.value).toBe('Cisco');
    expect(listaText()).toContain('Cisco OSI Guide');
    expect(listaText()).not.toContain('Linux CLI Lab');
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

  function abrirCrear(): void {
    clickPorTexto('+ Anadir recurso');
    fixture.detectChanges();
  }

  function abrirPrimerRecurso(): void {
    abrirRecurso(0);
  }

  function abrirRecurso(index: number): void {
    const buttons = fixture.nativeElement.querySelectorAll('.resource-open') as NodeListOf<HTMLButtonElement>;
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

  function cambiarTextInput(index: number, value: string): void {
    const input = fixture.nativeElement.querySelectorAll('input[type="text"]')[index] as HTMLInputElement;
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

  function cambiarTextarea(index: number, value: string): void {
    const textarea = fixture.nativeElement.querySelectorAll('textarea')[index] as HTMLTextAreaElement;
    textarea.value = value;
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();
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
