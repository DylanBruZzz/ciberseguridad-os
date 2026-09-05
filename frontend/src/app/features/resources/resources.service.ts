import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { toApiError } from '../../core/api-error';
import {
  CreateRecursoRequest,
  CreateRecursoResponse,
  RecursoDetalle,
  RecursoResumen,
  UpdateRecursoRequest,
} from './resources.models';

@Injectable({ providedIn: 'root' })
export class ResourcesService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  public listar(temaId?: string | null): Observable<RecursoResumen[]> {
    const params = temaId ? new HttpParams().set('temaId', temaId) : undefined;

    return this.http.get<RecursoResumen[]>(`${this.apiBaseUrl}/recursos`, { params }).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar los recursos.')),
      ),
    );
  }

  public obtenerDetalle(id: string): Observable<RecursoDetalle> {
    return this.http.get<RecursoDetalle>(`${this.apiBaseUrl}/recursos/${id}`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar el recurso.')),
      ),
    );
  }

  public crear(request: CreateRecursoRequest): Observable<CreateRecursoResponse> {
    return this.http.post<CreateRecursoResponse>(`${this.apiBaseUrl}/recursos`, request).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos crear el recurso.')),
      ),
    );
  }

  public actualizar(id: string, request: UpdateRecursoRequest): Observable<void> {
    return this.http.put<void>(`${this.apiBaseUrl}/recursos/${id}`, request).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos actualizar el recurso.')),
      ),
    );
  }

  public eliminar(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/recursos/${id}`).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos eliminar el recurso.')),
      ),
    );
  }

  public vincularTema(recursoId: string, temaId: string): Observable<void> {
    return this.http.put<void>(`${this.apiBaseUrl}/recursos/${recursoId}/temas/${temaId}`, {}).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos vincular el recurso al tema.')),
      ),
    );
  }
}
