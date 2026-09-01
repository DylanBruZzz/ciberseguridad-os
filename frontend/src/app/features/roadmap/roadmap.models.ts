export interface FaseResumen {
  id: string;
  nombre: string;
  orden: number;
  color: string | null;
  descripcion: string | null;
  objetivos: string[];
  criteriosAvance: string[];
  mesInicioRecomendado: number | null;
  mesFinRecomendado: number | null;
  cargaSemanalRecomendada: string | null;
}

export interface TemaResumen {
  id: string;
  faseId: string | null;
  temaPadreId: string | null;
  nombre: string;
  tipoConocimiento: string;
  descripcion: string | null;
}
