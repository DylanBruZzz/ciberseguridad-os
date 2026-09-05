export type EstadoRecurso = 'PorClasificar' | 'PorRevisar' | 'EnUso' | 'Consultado' | 'Referencia';

export type TipoRecurso =
  | 'Documentacion'
  | 'Libro'
  | 'Curso'
  | 'Video'
  | 'Laboratorio'
  | 'Writeup'
  | 'Cheatsheet'
  | 'Script'
  | 'RepositorioGitHub'
  | 'NotebookIA'
  | 'Otro';

export const ESTADOS_RECURSO: readonly EstadoRecurso[] = [
  'PorClasificar',
  'PorRevisar',
  'EnUso',
  'Consultado',
  'Referencia',
];

export const TIPOS_RECURSO: readonly TipoRecurso[] = [
  'Documentacion',
  'Libro',
  'Curso',
  'Video',
  'Laboratorio',
  'Writeup',
  'Cheatsheet',
  'Script',
  'RepositorioGitHub',
  'NotebookIA',
  'Otro',
];

export type RatingRecurso = 1 | 2 | 3 | 4 | 5;

export interface RecursoTemaResumen {
  id: string;
  nombre: string;
}

export interface RecursoResumen {
  id: string;
  usuarioId: string;
  tipo: TipoRecurso;
  titulo: string;
  url: string | null;
  estado: EstadoRecurso;
  temas: RecursoTemaResumen[];
}

export interface RecursoDetalle extends RecursoResumen {
  rating: RatingRecurso | null;
  notas: string | null;
  herramientaIA: string | null;
  promptsUtilizados: string | null;
}

export interface CreateRecursoRequest {
  tipo: TipoRecurso;
  titulo: string;
  url: string | null;
}

export interface CreateRecursoResponse {
  id: string;
  usuarioId: string;
  tipo: TipoRecurso;
  titulo: string;
  url: string | null;
  estado: EstadoRecurso;
}

export interface UpdateRecursoRequest {
  titulo: string;
  url: string | null;
  estado: EstadoRecurso;
  rating: RatingRecurso | null;
  notas: string | null;
  herramientaIA: string | null;
  promptsUtilizados: string | null;
}
