export type TipoEvidenceV1 =
  | 'Proyecto'
  | 'Laboratorio'
  | 'Writeup'
  | 'ArtefactoTecnico'
  | 'CertificacionObtenida';

export type EstadoMadurez = 'Borrador' | 'Documentado' | 'ListoPortafolio' | 'Publicado';

export type EstadoProyecto = 'Idea' | 'Desarrollo' | 'Documentado' | 'Publicado';

export type TipoArtefacto =
  | 'Script'
  | 'Herramienta'
  | 'Cheatsheet'
  | 'Dashboard'
  | 'Playbook'
  | 'ReglaDeteccion'
  | 'ConsultaSiem'
  | 'Automatizacion'
  | 'Plantilla'
  | 'Otro';

export type TipoNota = 'Nota' | 'Hallazgo' | 'Actualizacion' | 'Autoexplicacion';

export const TIPOS_EVIDENCE: readonly TipoEvidenceV1[] = [
  'Proyecto',
  'Laboratorio',
  'Writeup',
  'ArtefactoTecnico',
  'CertificacionObtenida',
];

export const ESTADOS_MADUREZ: readonly EstadoMadurez[] = [
  'Borrador',
  'Documentado',
  'ListoPortafolio',
  'Publicado',
];

export const ESTADOS_PROYECTO: readonly EstadoProyecto[] = [
  'Idea',
  'Desarrollo',
  'Documentado',
  'Publicado',
];

export const TIPOS_ARTEFACTO: readonly TipoArtefacto[] = [
  'Script',
  'Herramienta',
  'Cheatsheet',
  'Dashboard',
  'Playbook',
  'ReglaDeteccion',
  'ConsultaSiem',
  'Automatizacion',
  'Plantilla',
  'Otro',
];

export const TIPOS_NOTA: readonly TipoNota[] = ['Nota', 'Hallazgo', 'Actualizacion', 'Autoexplicacion'];

export interface EvidenceTemaV1 {
  id: string;
  nombre: string;
}

export interface EvidenceHerramientaV1 {
  id: string;
  nombre: string;
}

export interface EvidenceItemV1 {
  id: string;
  tipoEvidence: TipoEvidenceV1;
  titulo: string;
  estadoMadurez: EstadoMadurez;
  fechaCreacionUtc: string;
  fechaModificacionUtc: string | null;
  fechaActividadUtc: string;
  fechaReferencia: string | null;
  temas: EvidenceTemaV1[];
  herramientas: EvidenceHerramientaV1[];
}

export interface EvidenceListaV1 {
  total: number;
  items: EvidenceItemV1[];
}

export interface ProyectoDetalle {
  id: string;
  usuarioId: string;
  nombre: string;
  descripcion: string | null;
  estado: EstadoProyecto;
  estadoMadurez: EstadoMadurez;
  repositorioUrl: string | null;
  fechaInicio: string | null;
  fechaFin: string | null;
}

export interface LaboratorioDetalle {
  id: string;
  usuarioId: string;
  nombre: string;
  objetivo: string | null;
  entornoVms: string | null;
  hallazgos: string | null;
  tiempoInvertidoMinutos: number | null;
  estadoMadurez: EstadoMadurez;
  fecha: string | null;
}

export interface WriteupDetalle {
  id: string;
  usuarioId: string;
  titulo: string;
  plataformaOrigen: string | null;
  url: string | null;
  estadoMadurez: EstadoMadurez;
  fecha: string | null;
}

export interface ArtefactoTecnicoDetalle {
  id: string;
  usuarioId: string;
  tipoArtefacto: TipoArtefacto;
  nombre: string;
  contenidoOUrl: string | null;
  lenguajeTecnologia: string | null;
  estadoMadurez: EstadoMadurez;
}

export interface CertificacionObtenidaDetalle {
  id: string;
  usuarioId: string;
  certificacionId: string;
  fechaObtencion: string;
  evidenciaUrl: string | null;
  estadoMadurez: EstadoMadurez;
}

export type EvidenceDetail =
  | { tipoEvidence: 'Proyecto'; item: EvidenceItemV1; datos: ProyectoDetalle }
  | { tipoEvidence: 'Laboratorio'; item: EvidenceItemV1; datos: LaboratorioDetalle }
  | { tipoEvidence: 'Writeup'; item: EvidenceItemV1; datos: WriteupDetalle }
  | { tipoEvidence: 'ArtefactoTecnico'; item: EvidenceItemV1; datos: ArtefactoTecnicoDetalle }
  | {
      tipoEvidence: 'CertificacionObtenida';
      item: EvidenceItemV1;
      datos: CertificacionObtenidaDetalle;
    };

export interface CreateProyectoRequest {
  nombre: string;
}

export interface UpdateProyectoRequest {
  nombre: string;
  descripcion: string | null;
  estado: EstadoProyecto;
  estadoMadurez: EstadoMadurez;
  repositorioUrl: string | null;
  fechaInicio: string | null;
  fechaFin: string | null;
}

export interface CreateLaboratorioRequest {
  nombre: string;
  objetivo: string | null;
  entornoVms: string | null;
  hallazgos: string | null;
  tiempoInvertidoMinutos: number | null;
  fecha: string | null;
}

export interface UpdateLaboratorioRequest extends CreateLaboratorioRequest {
  estadoMadurez: EstadoMadurez;
}

export interface CreateWriteupRequest {
  titulo: string;
}

export interface UpdateWriteupRequest {
  titulo: string;
  plataformaOrigen: string | null;
  url: string | null;
  fecha: string | null;
  estadoMadurez: EstadoMadurez;
}

export interface CreateArtefactoTecnicoRequest {
  tipoArtefacto: TipoArtefacto;
  nombre: string;
}

export interface UpdateArtefactoTecnicoRequest extends CreateArtefactoTecnicoRequest {
  contenidoOUrl: string | null;
  lenguajeTecnologia: string | null;
  estadoMadurez: EstadoMadurez;
}

export interface CreateCertificacionObtenidaRequest {
  certificacionId: string;
  fechaObtencion: string;
}

export interface UpdateCertificacionObtenidaRequest {
  evidenciaUrl: string | null;
  estadoMadurez: EstadoMadurez;
}

export type CreateEvidenceRequest =
  | { tipoEvidence: 'Proyecto'; payload: CreateProyectoRequest }
  | { tipoEvidence: 'Laboratorio'; payload: CreateLaboratorioRequest }
  | { tipoEvidence: 'Writeup'; payload: CreateWriteupRequest }
  | { tipoEvidence: 'ArtefactoTecnico'; payload: CreateArtefactoTecnicoRequest }
  | { tipoEvidence: 'CertificacionObtenida'; payload: CreateCertificacionObtenidaRequest };

export type UpdateEvidenceRequest =
  | { tipoEvidence: 'Proyecto'; id: string; payload: UpdateProyectoRequest }
  | { tipoEvidence: 'Laboratorio'; id: string; payload: UpdateLaboratorioRequest }
  | { tipoEvidence: 'Writeup'; id: string; payload: UpdateWriteupRequest }
  | { tipoEvidence: 'ArtefactoTecnico'; id: string; payload: UpdateArtefactoTecnicoRequest }
  | {
      tipoEvidence: 'CertificacionObtenida';
      id: string;
      payload: UpdateCertificacionObtenidaRequest;
    };

export interface CreateEvidenceResponse {
  id: string | null;
}

export interface NotaResumen {
  id: string;
  usuarioId: string;
  temaId: string | null;
  proyectoId: string | null;
  laboratorioId: string | null;
  writeupId: string | null;
  artefactoTecnicoId: string | null;
  fecha: string;
  texto: string;
  tipo: TipoNota;
}

export interface CrearNotaRequest {
  texto: string;
  tipo: TipoNota;
}

export interface CertificacionResumen {
  id: string;
  nombre: string;
  proveedor: string | null;
  tipoCosto: string;
  url: string | null;
}
