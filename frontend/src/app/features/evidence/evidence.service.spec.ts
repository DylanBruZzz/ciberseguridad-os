import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/api.config';
import {
  CreateEvidenceRequest,
  EvidenceItemV1,
  UpdateEvidenceRequest,
} from './evidence.models';
import { EvidenceService } from './evidence.service';

const temaId = '01a046d5-9bf3-7cec-aa05-10c93459fa17';
const itemProyecto: EvidenceItemV1 = {
  id: '01a046d5-9bf3-7cec-aa05-10c93459fa18',
  tipoEvidence: 'Proyecto',
  titulo: 'Proyecto SOC',
  estadoMadurez: 'Documentado',
  fechaCreacionUtc: '2026-09-01T00:00:00Z',
  fechaModificacionUtc: '2026-09-02T00:00:00Z',
  fechaActividadUtc: '2026-09-02T00:00:00Z',
  fechaReferencia: '2026-09-01',
  temas: [{ id: temaId, nombre: 'Modelo OSI' }],
  herramientas: [{ id: '01a046d5-9bf3-7cec-aa05-10c93459fa19', nombre: 'Wireshark' }],
};

describe('EvidenceService', () => {
  let service: EvidenceService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(EvidenceService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('lista Evidence con filtros reales y sin usuarioId', () => {
    service
      .listar({ temaId, tipoEvidence: 'Laboratorio', estadoMadurez: 'Documentado' })
      .subscribe((resultado) => {
        expect(resultado.total).toBe(1);
        expect(resultado.items[0].tipoEvidence).toBe('Proyecto');
      });

    const req = http.expectOne(
      (request) =>
        request.url === '/api/evidence' &&
        request.params.get('temaId') === temaId &&
        request.params.get('tipoEvidence') === 'Laboratorio' &&
        request.params.get('estadoMadurez') === 'Documentado',
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('usuarioId')).toBeFalsy();
    req.flush({ total: 1, items: [itemProyecto] });
  });

  it('despacha detail por tipo real', () => {
    service.obtenerDetalle(itemProyecto).subscribe((detalle) => {
      expect(detalle.tipoEvidence).toBe('Proyecto');
      if (detalle.tipoEvidence === 'Proyecto') {
        expect(detalle.datos.nombre).toBe('Proyecto SOC');
      }
    });

    const req = http.expectOne(`/api/proyectos/${itemProyecto.id}`);
    expect(req.request.method).toBe('GET');
    req.flush({
      id: itemProyecto.id,
      usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
      nombre: 'Proyecto SOC',
      descripcion: null,
      estado: 'Idea',
      estadoMadurez: 'Documentado',
      repositorioUrl: null,
      fechaInicio: null,
      fechaFin: null,
    });
  });

  it('crea y actualiza usando endpoints especificos', () => {
    const crear: CreateEvidenceRequest = {
      tipoEvidence: 'ArtefactoTecnico',
      payload: { tipoArtefacto: 'Script', nombre: 'Parser logs' },
    };
    const actualizar: UpdateEvidenceRequest = {
      tipoEvidence: 'ArtefactoTecnico',
      id: itemProyecto.id,
      payload: {
        tipoArtefacto: 'Script',
        nombre: 'Parser logs editado',
        contenidoOUrl: 'https://example.com/parser',
        lenguajeTecnologia: 'PowerShell',
        estadoMadurez: 'ListoPortafolio',
      },
    };

    service.crear(crear).subscribe((resultado) => expect(resultado.id).toBe(itemProyecto.id));
    http.expectOne('/api/artefactos-tecnicos').flush({ id: itemProyecto.id });

    service.actualizar(actualizar).subscribe((resultado) => expect(resultado).toBeUndefined());
    const req = http.expectOne(`/api/artefactos-tecnicos/${itemProyecto.id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(actualizar.payload);
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('elimina y vincula tema con endpoints especificos', () => {
    service.eliminar('Laboratorio', itemProyecto.id).subscribe((resultado) => expect(resultado).toBeUndefined());
    const deleteReq = http.expectOne(`/api/laboratorios/${itemProyecto.id}`);
    expect(deleteReq.request.method).toBe('DELETE');
    deleteReq.flush(null, { status: 204, statusText: 'No Content' });

    service.vincularTema('Writeup', itemProyecto.id, temaId).subscribe((resultado) => expect(resultado).toBeUndefined());
    const req = http.expectOne(`/api/writeups/${itemProyecto.id}/temas/${temaId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({});
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('lista y agrega notas append-only', () => {
    service.listarNotas().subscribe((notas) => expect(notas[0].proyectoId).toBe(itemProyecto.id));
    http.expectOne('/api/notas').flush([
      {
        id: '01a046d5-9bf3-7cec-aa05-10c93459fa21',
        usuarioId: '01a046d5-9bf3-7cec-aa05-10c93459fa16',
        temaId: null,
        proyectoId: itemProyecto.id,
        laboratorioId: null,
        writeupId: null,
        artefactoTecnicoId: null,
        fecha: '2026-09-04T00:00:00Z',
        texto: 'Hallazgo factual',
        tipo: 'Hallazgo',
      },
    ]);

    service
      .agregarNota('Proyecto', itemProyecto.id, { texto: 'Nueva nota', tipo: 'Nota' })
      .subscribe((resultado) => expect(resultado).toBeUndefined());
    const req = http.expectOne(`/api/proyectos/${itemProyecto.id}/notas`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ texto: 'Nueva nota', tipo: 'Nota' });
    req.flush({ id: '01a046d5-9bf3-7cec-aa05-10c93459fa22' }, { status: 201, statusText: 'Created' });
  });

  it('carga certificaciones globales para create de CertificacionObtenida', () => {
    service.listarCertificaciones().subscribe((certificaciones) => {
      expect(certificaciones[0].nombre).toBe('Security+');
    });

    const req = http.expectOne('/api/certificaciones');
    expect(req.request.method).toBe('GET');
    req.flush([
      {
        id: '01a046d5-9bf3-7cec-aa05-10c93459fa23',
        nombre: 'Security+',
        proveedor: 'CompTIA',
        tipoCosto: 'Pago',
        url: null,
      },
    ]);
  });
});
