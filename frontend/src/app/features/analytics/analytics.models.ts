export type RangoEstudio = 'Todo' | 30 | 90;
export interface DatoGrafico {
  readonly clave: string;
  readonly etiqueta: string;
  readonly valor: number;
}
export interface StudyAnalytics {
  readonly totalSesiones: number;
  readonly totalMinutos: number;
  readonly mediaMinutos: number;
  readonly porTipo: readonly DatoGrafico[];
  readonly porFecha: readonly DatoGrafico[];
  readonly porMes: readonly DatoGrafico[];
  readonly porTema: readonly DatoGrafico[];
  readonly porFase: readonly DatoGrafico[];
}
export interface RoadmapAnalytics {
  readonly totalTemas: number;
  readonly repasoRecomendado: number;
  readonly porFase: readonly DatoGrafico[];
  readonly porEstado: readonly DatoGrafico[];
}
export interface EvidenceAnalytics {
  readonly total: number;
  readonly porTipo: readonly DatoGrafico[];
  readonly porMadurez: readonly DatoGrafico[];
}
export interface ResourceAnalytics {
  readonly total: number;
  readonly porTipo: readonly DatoGrafico[];
  readonly porEstado: readonly DatoGrafico[];
}
