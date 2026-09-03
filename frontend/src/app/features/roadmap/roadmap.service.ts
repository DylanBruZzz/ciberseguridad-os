import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, shareReplay, throwError } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { toApiError } from '../../core/api-error';
import { RoadmapVistaV1 } from './roadmap.models';

@Injectable({ providedIn: 'root' })
export class RoadmapService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private vistaCache$: Observable<RoadmapVistaV1> | null = null;

  public obtenerVista(): Observable<RoadmapVistaV1> {
    this.vistaCache$ ??= this.crearSolicitudVista();

    return this.vistaCache$;
  }

  public refrescarVista(): Observable<RoadmapVistaV1> {
    this.vistaCache$ = this.crearSolicitudVista();

    return this.vistaCache$;
  }

  private crearSolicitudVista(): Observable<RoadmapVistaV1> {
    return this.http.get<RoadmapVistaV1>(`${this.apiBaseUrl}/roadmap/vista`).pipe(
      catchError((error: unknown) => {
        this.vistaCache$ = null;

        return throwError(() => toApiError(error, 'No se pudo cargar la vista del roadmap.'));
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
  }
}
