import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { toApiError } from './api-error';

export interface UsuarioActual {
  id: string;
  nombre: string;
}

@Injectable({ providedIn: 'root' })
export class UsuarioActualService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  public obtener(): Observable<UsuarioActual> {
    return this.http.get<UsuarioActual>(`${this.apiBaseUrl}/usuario-actual`).pipe(
      catchError((error: unknown) =>
        throwError(() => toApiError(error, 'No se pudo obtener el usuario actual.')),
      ),
    );
  }
}
