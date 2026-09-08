import { EvidenceItemV1, ESTADOS_MADUREZ, TIPOS_EVIDENCE } from '../evidence/evidence.models';
import { ESTADOS_RECURSO, RecursoResumen, TIPOS_RECURSO } from '../resources/resources.models';
import { RoadmapVistaV1 } from '../roadmap/roadmap.models';
import { SesionEstudioResumen, TIPOS_SESION } from '../study/study.models';
import { DatoGrafico, EvidenceAnalytics, RangoEstudio, ResourceAnalytics, RoadmapAnalytics, StudyAnalytics } from './analytics.models';

const etiquetas: Readonly<Record<string, string>> = {
  Teoria: 'Teoría', Practica: 'Práctica', NoIniciado: 'No iniciado', EnPractica: 'En práctica', EnRepaso: 'En repaso',
  ArtefactoTecnico: 'Artefacto técnico', CertificacionObtenida: 'Certificación obtenida', ListoPortafolio: 'Listo para Portfolio',
  PorClasificar: 'Por clasificar', PorRevisar: 'Por revisar', EnUso: 'En uso', Documentacion: 'Documentación',
  RepositorioGitHub: 'Repositorio GitHub', NotebookIA: 'Notebook IA',
};

function distribuir(valores: readonly string[], orden: readonly string[]): DatoGrafico[] {
  const conteos = new Map(orden.map((clave) => [clave, 0]));
  for (const clave of valores) conteos.set(clave, (conteos.get(clave) ?? 0) + 1);
  return [...conteos].map(([clave, valor]) => ({ clave, etiqueta: etiquetas[clave] ?? clave, valor }));
}

// Fecha de Study es DateOnly. Nunca se parsea como instante UTC.
export function fechaLocal(fecha: Date): string {
  return `${fecha.getFullYear().toString().padStart(4, '0')}-${(fecha.getMonth() + 1).toString().padStart(2, '0')}-${fecha.getDate().toString().padStart(2, '0')}`;
}

export function filtrarSesiones(sesiones: readonly SesionEstudioResumen[], rango: RangoEstudio, hoy: string): readonly SesionEstudioResumen[] {
  if (rango === 'Todo') return sesiones;
  const [year, month, day] = hoy.split('-').map(Number);
  const inicio = new Date(year, month - 1, day, 12);
  inicio.setDate(inicio.getDate() - rango + 1);
  const desde = fechaLocal(inicio);
  return sesiones.filter((sesion) => sesion.fecha >= desde && sesion.fecha <= hoy);
}

function sumarPor(sesiones: readonly SesionEstudioResumen[], claveDe: (sesion: SesionEstudioResumen) => string): Map<string, number> {
  const sumas = new Map<string, number>();
  for (const sesion of sesiones) {
    const clave = claveDe(sesion);
    sumas.set(clave, (sumas.get(clave) ?? 0) + sesion.duracionMinutos);
  }
  return sumas;
}

export function agregarEstudio(sesiones: readonly SesionEstudioResumen[], vista: RoadmapVistaV1 | null): StudyAnalytics {
  const temas = new Map(vista?.fases.flatMap((fase) => fase.temas.map((tema) => [tema.id, { tema, fase }] as const)) ?? []);
  const fases = new Map(vista?.fases.map((fase) => [fase.id, fase]) ?? []);
  const totalMinutos = sesiones.reduce((total, sesion) => total + sesion.duracionMinutos, 0);
  const porFecha = [...sumarPor(sesiones, (s) => s.fecha)].sort(([a], [b]) => a.localeCompare(b))
    .map(([clave, valor]) => ({ clave, etiqueta: clave.split('-').reverse().join('/'), valor }));
  const porMes = [...sumarPor(sesiones, (s) => s.fecha.slice(0, 7))].sort(([a], [b]) => a.localeCompare(b))
    .map(([clave, valor]) => {
      const [year, month] = clave.split('-').map(Number);
      return { clave, etiqueta: new Intl.DateTimeFormat('es-PE', { month: 'short', year: 'numeric' }).format(new Date(year, month - 1, 1)), valor };
    });
  const porTiempo = (a: DatoGrafico, b: DatoGrafico) => b.valor - a.valor || a.etiqueta.localeCompare(b.etiqueta);
  return {
    totalSesiones: sesiones.length, totalMinutos, mediaMinutos: sesiones.length ? totalMinutos / sesiones.length : 0,
    porTipo: distribuir(sesiones.map((s) => s.tipo), TIPOS_SESION), porFecha, porMes,
    porTema: [...sumarPor(sesiones, (s) => s.temaId)].map(([clave, valor]) => ({
      clave, etiqueta: temas.get(clave)?.tema.nombre ?? 'Tema no disponible en Roadmap', valor,
    })).sort(porTiempo),
    porFase: [...sumarPor(sesiones, (s) => temas.get(s.temaId)?.fase.id ?? 'sin-fase-resuelta')].map(([clave, valor]) => ({
      clave, etiqueta: fases.get(clave)?.nombre ?? 'Sin fase resuelta en Roadmap', valor,
    })).sort(porTiempo),
  };
}

export function agregarRoadmap(vista: RoadmapVistaV1): RoadmapAnalytics {
  const temas = vista.fases.flatMap((fase) => fase.temas);
  return {
    totalTemas: temas.length,
    repasoRecomendado: temas.filter((tema) => tema.repasoRecomendado).length,
    porFase: [...vista.fases].sort((a, b) => a.orden - b.orden).map((fase) => ({ clave: fase.id, etiqueta: fase.nombre, valor: fase.progresoPorcentaje })),
    porEstado: distribuir(temas.map((tema) => tema.estado), ['NoIniciado', 'EnPractica', 'Dominado', 'EnRepaso']),
  };
}

export function agregarEvidence(items: readonly EvidenceItemV1[]): EvidenceAnalytics {
  return { total: items.length, porTipo: distribuir(items.map((item) => item.tipoEvidence), TIPOS_EVIDENCE), porMadurez: distribuir(items.map((item) => item.estadoMadurez), ESTADOS_MADUREZ) };
}

export function agregarRecursos(items: readonly RecursoResumen[]): ResourceAnalytics {
  return { total: items.length, porTipo: distribuir(items.map((item) => item.tipo), TIPOS_RECURSO).filter((dato) => dato.valor > 0), porEstado: distribuir(items.map((item) => item.estado), ESTADOS_RECURSO) };
}
