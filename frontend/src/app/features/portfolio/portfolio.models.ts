import {
  EstadoMadurez,
  EstadoProyecto,
  TipoArtefacto,
  TipoEvidenceV1,
} from '../evidence/evidence.models';

export type EstadoMadurezPortfolio = Extract<EstadoMadurez, 'ListoPortafolio' | 'Publicado'>;
export type TipoEvidencePortfolio = TipoEvidenceV1;

export const TIPOS_PORTFOLIO: readonly TipoEvidencePortfolio[] = [
  'Proyecto',
  'Laboratorio',
  'Writeup',
  'ArtefactoTecnico',
  'CertificacionObtenida',
];

export const ESTADOS_PORTFOLIO: readonly EstadoMadurezPortfolio[] = [
  'ListoPortafolio',
  'Publicado',
];

export interface PortafolioTemaDto {
  temaId: string;
  nombre: string;
}

export interface PortafolioHerramientaDto {
  herramientaId: string;
  nombre: string;
  categoria: string | null;
}

export interface PortafolioProyectoDto {
  proyectoId: string;
  nombre: string;
  descripcion: string | null;
  estado: EstadoProyecto;
  estadoMadurez: EstadoMadurez;
  repositorioUrl: string | null;
  fechaInicio: string | null;
  fechaFin: string | null;
  temas: PortafolioTemaDto[];
  herramientas: PortafolioHerramientaDto[];
}

export interface PortafolioLaboratorioDto {
  laboratorioId: string;
  nombre: string;
  objetivo: string | null;
  entornoVms: string | null;
  hallazgos: string | null;
  tiempoInvertidoMinutos: number | null;
  fecha: string | null;
  estadoMadurez: EstadoMadurez;
  temas: PortafolioTemaDto[];
  herramientas: PortafolioHerramientaDto[];
}

export interface PortafolioWriteupDto {
  writeupId: string;
  titulo: string;
  plataformaOrigen: string | null;
  url: string | null;
  fecha: string | null;
  estadoMadurez: EstadoMadurez;
  temas: PortafolioTemaDto[];
}

export interface PortafolioArtefactoTecnicoDto {
  artefactoTecnicoId: string;
  tipoArtefacto: TipoArtefacto;
  nombre: string;
  contenidoOUrl: string | null;
  lenguajeTecnologia: string | null;
  estadoMadurez: EstadoMadurez;
  temas: PortafolioTemaDto[];
  herramientas: PortafolioHerramientaDto[];
}

export interface PortafolioCertificacionObtenidaDto {
  certificacionObtenidaId: string;
  certificacionId: string;
  nombre: string;
  proveedor: string | null;
  tipoCosto: string;
  fechaObtencion: string;
  evidenciaUrl: string | null;
  estadoMadurez: EstadoMadurez;
}

export interface PortafolioResumenDto {
  total: number;
  proyectos: number;
  laboratorios: number;
  writeups: number;
  artefactosTecnicos: number;
  certificacionesObtenidas: number;
  listosPortafolio: number;
  publicados: number;
}

export interface PortafolioDto {
  usuarioId: string;
  resumen: PortafolioResumenDto;
  proyectos: PortafolioProyectoDto[];
  laboratorios: PortafolioLaboratorioDto[];
  writeups: PortafolioWriteupDto[];
  artefactosTecnicos: PortafolioArtefactoTecnicoDto[];
  certificacionesObtenidas: PortafolioCertificacionObtenidaDto[];
}

export interface PortfolioTema {
  id: string;
  nombre: string;
}

export interface PortfolioHerramienta {
  id: string;
  nombre: string;
  categoria: string | null;
}

export interface PortfolioItem {
  id: string;
  tipoEvidence: TipoEvidencePortfolio;
  titulo: string;
  estadoMadurez: EstadoMadurez;
  fechaPrincipal: string | null;
  fechaEtiqueta: string;
  resumen: string | null;
  temas: PortfolioTema[];
  herramientas: PortfolioHerramienta[];
}
