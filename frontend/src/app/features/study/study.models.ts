export type TipoSesion = 'Teoria' | 'Practica' | 'Laboratorio' | 'Repaso';

export const TIPOS_SESION: readonly TipoSesion[] = ['Teoria', 'Practica', 'Laboratorio', 'Repaso'];

export interface SesionEstudioResumen {
  id: string;
  usuarioId: string;
  temaId: string;
  fecha: string;
  duracionMinutos: number;
  tipo: TipoSesion;
  notas: string | null;
}

export interface RegistrarSesionEstudioRequest {
  temaId: string;
  fecha: string;
  duracionMinutos: number;
  tipo: TipoSesion;
  notas: string | null;
}

export type ActualizarSesionEstudioRequest = RegistrarSesionEstudioRequest;

export interface RegistrarSesionEstudioResponse {
  estado: 'Creada' | 'TemaNoEncontrado' | 'UsuarioNoCoincide';
  id: string | null;
  usuarioId: string | null;
  temaId: string | null;
  fecha: string | null;
  duracionMinutos: number | null;
  tipo: TipoSesion | null;
  notas: string | null;
}
