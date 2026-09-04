import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { toApiError } from '../../core/api-error';
import {
  ActualizarSesionEstudioRequest,
  RegistrarSesionEstudioRequest,
  RegistrarSesionEstudioResponse,
  SesionEstudioResumen,
} from './study.models';

@Injectable({ providedIn: 'root' })
export class StudyService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  public listarSesiones(): Observable<SesionEstudioResumen[]> {
    return this.http.get<SesionEstudioResumen[]>(`${this.apiBaseUrl}/sesiones-estudio`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar las sesiones de estudio.')),
      ),
    );
  }

  public registrarSesion(
    request: RegistrarSesionEstudioRequest,
  ): Observable<RegistrarSesionEstudioResponse> {
    return this.http.post<RegistrarSesionEstudioResponse>(
      `${this.apiBaseUrl}/sesiones-estudio`,
      request,
    ).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos registrar la sesion de estudio.')),
      ),
    );
  }

  public actualizarSesion(id: string, request: ActualizarSesionEstudioRequest): Observable<void> {
    return this.http.put<void>(`${this.apiBaseUrl}/sesiones-estudio/${id}`, request).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos actualizar la sesion de estudio.')),
      ),
    );
  }

  public eliminarSesion(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/sesiones-estudio/${id}`).pipe(
      map(() => undefined),
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos eliminar la sesion de estudio.')),
      ),
    );
  }
}
