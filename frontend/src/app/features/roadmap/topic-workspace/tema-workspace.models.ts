export interface TemaWorkspaceV1 {
  tema: TemaWorkspaceTema;
  fase: TemaWorkspaceFase | null;
  apuntes: TemaWorkspaceApuntes;
  ultimaSesion: TemaWorkspaceSesion | null;
  repaso: TemaWorkspaceRepaso;
  resourcesResumen: TemaWorkspaceResourcesResumen;
  sesionesResumen: TemaWorkspaceSesionesResumen;
  evidenceResumen: TemaWorkspaceEvidenceResumen;
}

export interface TemaWorkspaceTema {
  id: string;
  temaPadreId: string | null;
  nombre: string;
  descripcion: string | null;
  tipoConocimiento: string;
  estado: string;
  dificultadPercibida: number | null;
  confianza: number | null;
  intervaloRepasoDias: number;
  criteriosTotal: number;
  criteriosCumplidos: number;
  progresoPorcentaje: number;
  objetivos: string[];
  criterios: TemaWorkspaceCriterio[];
}

export interface TemaWorkspaceCriterio {
  id: string;
  tipo: string;
  cumplido: boolean;
  fechaCumplidoUtc: string | null;
}

export interface TemaWorkspaceFase {
  id: string;
  orden: number;
  nombre: string;
}

export interface TemaWorkspaceApuntes {
  contenido: string;
  fechaModificacionUtc: string | null;
}

export interface TemaWorkspaceSesion {
  id: string;
  fecha: string;
  duracionMinutos: number;
  tipo: string;
}

export interface TemaWorkspaceRepaso {
  proximaFechaRepaso: string | null;
  repasoRecomendado: boolean;
}

export interface TemaWorkspaceResourcesResumen {
  total: number;
}

export interface TemaWorkspaceSesionesResumen {
  total: number;
  totalMinutos: number;
}

export interface TemaWorkspaceEvidenceResumen {
  total: number;
  proyectos: number;
  laboratorios: number;
  writeups: number;
  artefactosTecnicos: number;
  certificacionesObtenidas: number;
}
