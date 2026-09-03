export interface RoadmapVistaV1 {
  faseActualId: string | null;
  progresoGlobalPorcentaje: number;
  totalTemas: number;
  temasDominados: number;
  fases: FaseRoadmapVistaV1[];
}

export interface FaseRoadmapVistaV1 {
  id: string;
  orden: number;
  nombre: string;
  color: string | null;
  descripcion: string | null;
  objetivos: string[];
  criteriosAvance: string[];
  mesInicioRecomendado: number | null;
  mesFinRecomendado: number | null;
  cargaSemanalRecomendada: string | null;
  totalTemas: number;
  temasDominados: number;
  progresoPorcentaje: number;
  estaCompletada: boolean;
  esFaseActual: boolean;
  temas: TemaRoadmapVistaV1[];
}

export interface TemaRoadmapVistaV1 {
  id: string;
  faseId: string | null;
  temaPadreId: string | null;
  nombre: string;
  descripcion: string | null;
  tipoConocimiento: string;
  dificultadPercibida: number | null;
  confianza: number | null;
  intervaloRepasoDias: number;
  estado: string;
  criteriosTotal: number;
  criteriosCumplidos: number;
  progresoPorcentaje: number;
  ultimaSesion: string | null;
  proximaFechaRepaso: string | null;
  repasoRecomendado: boolean;
}
