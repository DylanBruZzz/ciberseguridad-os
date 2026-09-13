import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../../core/api.config';
import { TemaWorkspaceV1 } from './tema-workspace.models';
import { TemaWorkspaceService } from './tema-workspace.service';

const workspace: TemaWorkspaceV1 = {
  tema: {
    id: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
    temaPadreId: null,
    nombre: 'Modelo OSI',
    descripcion: 'Base de redes',
    tipoConocimiento: 'Conceptual',
    estado: 'EnRepaso',
    dificultadPercibida: 4,
    confianza: 3,
    intervaloRepasoDias: 14,
    criteriosTotal: 2,
    criteriosCumplidos: 2,
    progresoPorcentaje: 100,
    objetivos: ['Comprender capas'],
    criterios: [],
  },
  fase: {
    id: '01a046d5-9bf3-7cec-aa05-10c93459fa17',
    orden: 1,
    nombre: 'Fundamentos',
  },
  apuntes: {
    contenido: 'Capas y encapsulacion',
    fechaModificacionUtc: '2026-09-01T00:00:00Z',
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
    id: '01a046d5-9bf3-7cec-aa05-10c93459fa18',
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
    total: 1,
    proyectos: 1,
    laboratorios: 0,
    writeups: 0,
    artefactosTecnicos: 0,
    certificacionesObtenidas: 0,
  },
};

describe('TemaWorkspaceService', () => {
  let service: TemaWorkspaceService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(TemaWorkspaceService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('carga TemaWorkspaceV1 sin enviar usuarioId', () => {
    service.obtenerWorkspace(workspace.tema.id).subscribe((resultado) => {
      expect(resultado.tema.progresoPorcentaje).toBe(100);
      expect(resultado.tema.estado).toBe('EnRepaso');
    });

    const req = http.expectOne(`/api/temas/${workspace.tema.id}/workspace`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush(workspace);
  });

  it('guarda apuntes sin enviar usuarioId', () => {
    service.guardarApuntes(workspace.tema.id, 'Nuevo contenido').subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/temas/${workspace.tema.id}/apuntes`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    expect(req.request.body).toEqual({ contenido: 'Nuevo contenido' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('lista herramientas del catalogo existente', () => {
    service.listarHerramientas().subscribe((resultado) => {
      expect(resultado[0].nombre).toBe('Wireshark');
    });

    const req = http.expectOne('/api/herramientas');
    expect(req.request.method).toBe('GET');
    req.flush([{ id: 'herramienta-1', nombre: 'Wireshark', categoria: 'Redes' }]);
  });

  it('vincula herramienta a Tema sin usuarioId ni body redundante', () => {
    service.vincularHerramienta(workspace.tema.id, 'herramienta-1').subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/temas/${workspace.tema.id}/herramientas/herramienta-1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    expect(req.request.body).toBeNull();
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('define criterios con tipo y descripcion', () => {
    service.definirCriterios(workspace.tema.id, [
      { tipo: 'Teoria', descripcion: 'Explicar el modelo OSI.' },
      { tipo: 'Practica', descripcion: 'Diagnosticar conectividad.' },
    ]).subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/temas/${workspace.tema.id}/criterios`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    expect(req.request.body).toEqual({
      criterios: [
        { tipo: 'Teoria', descripcion: 'Explicar el modelo OSI.' },
        { tipo: 'Practica', descripcion: 'Diagnosticar conectividad.' },
      ],
    });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('marca y desmarca criterios sin enviar usuarioId', () => {
    service.marcarCriterio(workspace.tema.id, 'Teoria').subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const marcar = http.expectOne(`/api/temas/${workspace.tema.id}/criterios/Teoria/cumplido`);
    expect(marcar.request.method).toBe('PUT');
    expect(marcar.request.body).toBeNull();
    marcar.flush(null, { status: 204, statusText: 'No Content' });

    service.desmarcarCriterio(workspace.tema.id, 'Teoria').subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const desmarcar = http.expectOne(`/api/temas/${workspace.tema.id}/criterios/Teoria/cumplido`);
    expect(desmarcar.request.method).toBe('DELETE');
    expect(desmarcar.request.params.has('usuarioId')).toBeFalsy();
    desmarcar.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('desvincula herramienta de Tema sin usuarioId', () => {
    service.desvincularHerramienta(workspace.tema.id, 'herramienta-1').subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const req = http.expectOne(`/api/temas/${workspace.tema.id}/herramientas/herramienta-1`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('lista y vincula certificaciones existentes sin usuarioId', () => {
    service.listarCertificaciones().subscribe((resultado) => {
      expect(resultado[0].nombre).toBe('Security+');
    });

    const listar = http.expectOne('/api/certificaciones');
    expect(listar.request.method).toBe('GET');
    listar.flush([
      {
        id: 'certificacion-1',
        nombre: 'Security+',
        proveedor: 'CompTIA',
        tipoCosto: 'Pago',
        url: null,
      },
    ]);

    service.vincularCertificacion(workspace.tema.id, 'certificacion-1').subscribe((resultado) => {
      expect(resultado).toBeUndefined();
    });

    const vincular = http.expectOne(`/api/certificaciones/certificacion-1/temas/${workspace.tema.id}`);
    expect(vincular.request.method).toBe('PUT');
    expect(vincular.request.params.has('usuarioId')).toBeFalsy();
    expect(vincular.request.body).toBeNull();
    vincular.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('traduce 404 a tema no disponible', () => {
    const errores: string[] = [];

    service.obtenerWorkspace(workspace.tema.id).subscribe({
      error: (error: Error) => errores.push(error.message),
    });

    http.expectOne(`/api/temas/${workspace.tema.id}/workspace`).flush(null, {
      status: 404,
      statusText: 'Not Found',
    });

    expect(errores[0]).toBe('Este tema no esta disponible.');
  });
});
