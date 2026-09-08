import { EvidenceItemV1 } from '../../evidence/evidence.models';
import { RecursoResumen } from '../../resources/resources.models';
import { FaseRoadmapVistaV1, RoadmapVistaV1, TemaRoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { SesionEstudioResumen } from '../../study/study.models';

export const sesion = (cambios: Partial<SesionEstudioResumen> = {}): SesionEstudioResumen => ({
  id: 'sesion-1', usuarioId: 'usuario-1', temaId: 'tema-1', fecha: '2026-09-08', duracionMinutos: 30, tipo: 'Teoria', notas: null, ...cambios,
});
export const tema = (cambios: Partial<TemaRoadmapVistaV1> = {}): TemaRoadmapVistaV1 => ({
  id: 'tema-1', faseId: 'fase-1', temaPadreId: null, nombre: 'Redes', descripcion: null, tipoConocimiento: 'Teorico', dificultadPercibida: null,
  confianza: null, intervaloRepasoDias: 14, estado: 'NoIniciado', criteriosTotal: 0, criteriosCumplidos: 0, progresoPorcentaje: 0,
  ultimaSesion: null, proximaFechaRepaso: null, repasoRecomendado: false, ...cambios,
});
export const fase = (cambios: Partial<FaseRoadmapVistaV1> = {}): FaseRoadmapVistaV1 => ({
  id: 'fase-1', orden: 1, nombre: 'Fundamentos', color: null, descripcion: null, objetivos: [], criteriosAvance: [], mesInicioRecomendado: null,
  mesFinRecomendado: null, cargaSemanalRecomendada: null, totalTemas: 1, temasDominados: 0, progresoPorcentaje: 37,
  estaCompletada: false, esFaseActual: true, temas: [tema()], ...cambios,
});
export const vista = (fases = [fase()]): RoadmapVistaV1 => ({
  faseActualId: fases[0]?.id ?? null, progresoGlobalPorcentaje: 37, totalTemas: fases.reduce((n, f) => n + f.temas.length, 0), temasDominados: 0, fases,
});
export const evidencia = (cambios: Partial<EvidenceItemV1> = {}): EvidenceItemV1 => ({
  id: 'evidence-1', tipoEvidence: 'Proyecto', titulo: 'Registro de laboratorio', estadoMadurez: 'Borrador', fechaCreacionUtc: '2026-09-08T01:00:00Z',
  fechaModificacionUtc: null, fechaActividadUtc: '2026-09-08T01:00:00Z', fechaReferencia: null, temas: [], herramientas: [], ...cambios,
});
export const recurso = (cambios: Partial<RecursoResumen> = {}): RecursoResumen => ({
  id: 'recurso-1', usuarioId: 'usuario-1', titulo: 'Documentación', tipo: 'Documentacion', estado: 'Referencia', url: null, temas: [], ...cambios,
});
