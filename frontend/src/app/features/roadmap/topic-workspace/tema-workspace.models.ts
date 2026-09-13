export interface TemaWorkspaceV1 {
  tema: TemaWorkspaceTema;
  fase: TemaWorkspaceFase | null;
  apuntes: TemaWorkspaceApuntes;
  herramientas: TemaWorkspaceHerramienta[];
  certificaciones: TemaWorkspaceCertificacion[];
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
  descripcion: string | null;
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

export interface TemaWorkspaceHerramienta {
  id: string;
  nombre: string;
}

export interface HerramientaCatalogo {
  id: string;
  nombre: string;
  categoria: string | null;
}

export interface TemaWorkspaceCertificacion {
  id: string;
  nombre: string;
  proveedor: string | null;
  tipoCosto: string;
}

export interface CertificacionCatalogo {
  id: string;
  nombre: string;
  proveedor: string | null;
  tipoCosto: string;
  url: string | null;
}

export const tiposCriterioTema = [
  'Teoria',
  'Practica',
  'Explicacion',
  'Ejercicios',
  'Laboratorio',
] as const;

export type TipoCriterioTema = (typeof tiposCriterioTema)[number];

export interface DefinicionCriterioTema {
  tipo: TipoCriterioTema;
  descripcion: string;
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
