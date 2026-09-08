import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { toApiError } from '../../core/api-error';
import {
  EstadoMadurezPortfolio,
  PortafolioDto,
  TipoEvidencePortfolio,
} from './portfolio.models';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  public listar(filtros: {
    tipoEvidence?: TipoEvidencePortfolio | '' | null;
    estadoMadurez?: EstadoMadurezPortfolio | '' | null;
  }): Observable<PortafolioDto> {
    let params = new HttpParams();

    if (filtros.tipoEvidence) {
      params = params.set('tipoEvidence', filtros.tipoEvidence);
    }

    if (filtros.estadoMadurez) {
      params = params.set('estadoMadurez', filtros.estadoMadurez);
    }

    return this.http.get<PortafolioDto>(`${this.apiBaseUrl}/portafolio`, { params }).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No pudimos cargar Portfolio.')),
      ),
    );
  }
}
