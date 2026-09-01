import { HttpErrorResponse } from '@angular/common/http';

export function toApiError(error: unknown, fallback: string): Error {
  if (!(error instanceof HttpErrorResponse)) {
    return new Error(fallback);
  }

  if (error.status === 0) {
    return new Error('La API local no esta disponible. Inicia el runtime Personal y vuelve a intentar.');
  }

  const codigo = obtenerCodigo(error.error);

  if (codigo === 'SIN_USUARIO_LOCAL') {
    return new Error('La base personal local no tiene un usuario visible. Completa el bootstrap personal.');
  }

  if (codigo === 'MULTIPLES_USUARIOS_LOCALES') {
    return new Error('La base personal local tiene mas de un usuario visible. Revisa la configuracion local.');
  }

  return new Error(fallback);
}

function obtenerCodigo(error: unknown): string | null {
  if (!error || typeof error !== 'object') {
    return null;
  }

  const maybeError = error as { codigo?: unknown; error?: unknown; code?: unknown };

  if (typeof maybeError.codigo === 'string') {
    return maybeError.codigo;
  }

  if (typeof maybeError.error === 'string') {
    return maybeError.error;
  }

  if (typeof maybeError.code === 'string') {
    return maybeError.code;
  }

  return null;
}
