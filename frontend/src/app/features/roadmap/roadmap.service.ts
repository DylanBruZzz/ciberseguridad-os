import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { toApiError } from '../../core/api-error';
import { FaseResumen, TemaResumen } from './roadmap.models';

@Injectable({ providedIn: 'root' })
export class RoadmapService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  public listarFases(): Observable<FaseResumen[]> {
    return this.http.get<FaseResumen[]>(`${this.apiBaseUrl}/fases`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No se pudieron cargar las fases del roadmap.')),
      ),
    );
  }

  public listarTemas(): Observable<TemaResumen[]> {
    return this.http.get<TemaResumen[]>(`${this.apiBaseUrl}/temas`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No se pudieron cargar los temas del roadmap.')),
      ),
    );
  }
}
