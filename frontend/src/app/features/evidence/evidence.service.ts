import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { toApiError } from '../../core/api-error';
import {
  CertificacionResumen,
  ArtefactoTecnicoDetalle,
  CrearNotaRequest,
  CreateEvidenceRequest,
  CreateEvidenceResponse,
  CertificacionObtenidaDetalle,
  EvidenceDetail,
  EvidenceItemV1,
  EvidenceListaV1,
  EstadoMadurez,
  LaboratorioDetalle,
  NotaResumen,
  ProyectoDetalle,
  TipoEvidenceV1,
  UpdateEvidenceRequest,
  WriteupDetalle,
} from './evidence.models';

@Injectable({ providedIn: 'root' })
export class EvidenceService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  public listar(filtros: {
    temaId?: string | null;
    tipoEvidence?: TipoEvidenceV1 | '' | null;
    estadoMadurez?: EstadoMadurez | '' | null;
  }): Observable<EvidenceListaV1> {
    let params = new HttpParams();

    if (filtros.temaId) {
      params = params.set('temaId', filtros.temaId);
    }

    if (filtros.tipoEvidence) {
      params = params.set('tipoEvidence', filtros.tipoEvidence);
    }

    if (filtros.estadoMadurez) {
      params = params.set('estadoMadurez', filtros.estadoMadurez);
    }

    return this.http.get<EvidenceListaV1>(`${this.apiBaseUrl}/evidence`, { params }).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar las evidencias.')),
      ),
    );
  }

  public obtenerDetalle(item: EvidenceItemV1): Observable<EvidenceDetail> {
    switch (item.tipoEvidence) {
      case 'Proyecto':
        return this.http
          .get<ProyectoDetalle>(`${this.apiBaseUrl}/proyectos/${item.id}`)
          .pipe(
            map((datos) => ({ tipoEvidence: 'Proyecto' as const, item, datos })),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos cargar el proyecto.')),
            ),
          );
      case 'Laboratorio':
        return this.http
          .get<LaboratorioDetalle>(`${this.apiBaseUrl}/laboratorios/${item.id}`)
          .pipe(
            map((datos) => ({ tipoEvidence: 'Laboratorio' as const, item, datos })),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos cargar el laboratorio.')),
            ),
          );
      case 'Writeup':
        return this.http
          .get<WriteupDetalle>(`${this.apiBaseUrl}/writeups/${item.id}`)
          .pipe(
            map((datos) => ({ tipoEvidence: 'Writeup' as const, item, datos })),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos cargar el writeup.')),
            ),
          );
      case 'ArtefactoTecnico':
        return this.http
          .get<ArtefactoTecnicoDetalle>(`${this.apiBaseUrl}/artefactos-tecnicos/${item.id}`)
          .pipe(
            map((datos) => ({ tipoEvidence: 'ArtefactoTecnico' as const, item, datos })),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos cargar el artefacto tecnico.')),
            ),
          );
      case 'CertificacionObtenida':
        return this.http
          .get<CertificacionObtenidaDetalle>(`${this.apiBaseUrl}/certificaciones-obtenidas/${item.id}`)
          .pipe(
            map((datos) => ({ tipoEvidence: 'CertificacionObtenida' as const, item, datos })),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos cargar la certificacion obtenida.')),
            ),
          );
    }
  }

  public crear(request: CreateEvidenceRequest): Observable<CreateEvidenceResponse> {
    switch (request.tipoEvidence) {
      case 'Proyecto':
        return this.http.post<CreateEvidenceResponse>(`${this.apiBaseUrl}/proyectos`, request.payload).pipe(
          catchError((error: unknown) =>
            throwError(() => toApiError(error, 'No pudimos crear el proyecto.')),
          ),
        );
      case 'Laboratorio':
        return this.http.post<CreateEvidenceResponse>(`${this.apiBaseUrl}/laboratorios`, request.payload).pipe(
          catchError((error: unknown) =>
            throwError(() => toApiError(error, 'No pudimos crear el laboratorio.')),
          ),
        );
      case 'Writeup':
        return this.http.post<CreateEvidenceResponse>(`${this.apiBaseUrl}/writeups`, request.payload).pipe(
          catchError((error: unknown) =>
            throwError(() => toApiError(error, 'No pudimos crear el writeup.')),
          ),
        );
      case 'ArtefactoTecnico':
        return this.http.post<CreateEvidenceResponse>(
          `${this.apiBaseUrl}/artefactos-tecnicos`,
          request.payload,
        ).pipe(
          catchError((error: unknown) =>
            throwError(() => toApiError(error, 'No pudimos crear el artefacto tecnico.')),
          ),
        );
      case 'CertificacionObtenida':
        return this.http.post<CreateEvidenceResponse>(
          `${this.apiBaseUrl}/certificaciones-obtenidas`,
          request.payload,
        ).pipe(
          catchError((error: unknown) =>
            throwError(() => toApiError(error, 'No pudimos crear la certificacion obtenida.')),
          ),
        );
    }
  }

  public actualizar(request: UpdateEvidenceRequest): Observable<void> {
    switch (request.tipoEvidence) {
      case 'Proyecto':
        return this.http.put<void>(`${this.apiBaseUrl}/proyectos/${request.id}`, request.payload).pipe(
          map(() => undefined),
          catchError((error: unknown) =>
            throwError(() => toApiError(error, 'No pudimos actualizar el proyecto.')),
          ),
        );
      case 'Laboratorio':
        return this.http
          .put<void>(`${this.apiBaseUrl}/laboratorios/${request.id}`, request.payload)
          .pipe(
            map(() => undefined),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos actualizar el laboratorio.')),
            ),
          );
      case 'Writeup':
        return this.http.put<void>(`${this.apiBaseUrl}/writeups/${request.id}`, request.payload).pipe(
          map(() => undefined),
          catchError((error: unknown) =>
            throwError(() => toApiError(error, 'No pudimos actualizar el writeup.')),
          ),
        );
      case 'ArtefactoTecnico':
        return this.http
          .put<void>(`${this.apiBaseUrl}/artefactos-tecnicos/${request.id}`, request.payload)
          .pipe(
            map(() => undefined),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos actualizar el artefacto tecnico.')),
            ),
          );
      case 'CertificacionObtenida':
        return this.http
          .put<void>(`${this.apiBaseUrl}/certificaciones-obtenidas/${request.id}`, request.payload)
          .pipe(
            map(() => undefined),
            catchError((error: unknown) =>
              throwError(() => toApiError(error, 'No pudimos actualizar la certificacion obtenida.')),
            ),
          );
    }
  }

  public eliminar(tipoEvidence: TipoEvidenceV1, id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/${this.segmento(tipoEvidence)}/${id}`).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos eliminar la evidencia.')),
      ),
    );
  }

  public vincularTema(tipoEvidence: TipoEvidenceV1, id: string, temaId: string): Observable<void> {
    return this.http.put<void>(`${this.apiBaseUrl}/${this.segmento(tipoEvidence)}/${id}/temas/${temaId}`, {}).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos vincular la evidencia al tema.')),
      ),
    );
  }

  public listarNotas(): Observable<NotaResumen[]> {
    return this.http.get<NotaResumen[]>(`${this.apiBaseUrl}/notas`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar las notas.')),
      ),
    );
  }

  public agregarNota(tipoEvidence: TipoEvidenceV1, id: string, request: CrearNotaRequest): Observable<void> {
    return this.http.post(`${this.apiBaseUrl}/${this.segmento(tipoEvidence)}/${id}/notas`, request).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos agregar la nota.')),
      ),
    );
  }

  public listarCertificaciones(): Observable<CertificacionResumen[]> {
    return this.http.get<CertificacionResumen[]>(`${this.apiBaseUrl}/certificaciones`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar las certificaciones.')),
      ),
    );
  }

  private segmento(tipoEvidence: TipoEvidenceV1): string {
    const segmentos: Record<TipoEvidenceV1, string> = {
      Proyecto: 'proyectos',
      Laboratorio: 'laboratorios',
      Writeup: 'writeups',
      ArtefactoTecnico: 'artefactos-tecnicos',
      CertificacionObtenida: 'certificaciones-obtenidas',
    };

    return segmentos[tipoEvidence];
  }
}
