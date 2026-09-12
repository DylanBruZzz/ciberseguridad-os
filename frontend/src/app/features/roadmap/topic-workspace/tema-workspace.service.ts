import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../../core/api.config';
import { toApiError } from '../../../core/api-error';
import { HerramientaCatalogo, TemaWorkspaceV1 } from './tema-workspace.models';

@Injectable({ providedIn: 'root' })
export class TemaWorkspaceService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  public obtenerWorkspace(temaId: string): Observable<TemaWorkspaceV1> {
    return this.http.get<TemaWorkspaceV1>(`${this.apiBaseUrl}/temas/${temaId}/workspace`).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          return throwError(() => new Error('Este tema no esta disponible.'));
        }

        return throwError(() => toApiError(error, 'No pudimos cargar este tema.'));
      }),
    );
  }

  public guardarApuntes(temaId: string, contenido: string): Observable<void> {
    return this.http
      .put<void>(`${this.apiBaseUrl}/temas/${temaId}/apuntes`, { contenido })
      .pipe(
        map(() => undefined),
        catchError((error: unknown) =>
          throwError(() => toApiError(error, 'No pudimos guardar los apuntes.')),
        ),
      );
  }

  public listarHerramientas(): Observable<HerramientaCatalogo[]> {
    return this.http.get<HerramientaCatalogo[]>(`${this.apiBaseUrl}/herramientas`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar las herramientas.')),
      ),
    );
  }

  public vincularHerramienta(temaId: string, herramientaId: string): Observable<void> {
    return this.http
      .put<void>(`${this.apiBaseUrl}/temas/${temaId}/herramientas/${herramientaId}`, null)
      .pipe(
        map(() => undefined),
        catchError((error: unknown) =>
          throwError(() => toApiError(error, 'No pudimos vincular la herramienta.')),
        ),
      );
  }

  public desvincularHerramienta(temaId: string, herramientaId: string): Observable<void> {
    return this.http
      .delete<void>(`${this.apiBaseUrl}/temas/${temaId}/herramientas/${herramientaId}`)
      .pipe(
        map(() => undefined),
        catchError((error: unknown) =>
          throwError(() => toApiError(error, 'No pudimos quitar la herramienta.')),
        ),
      );
  }
}
