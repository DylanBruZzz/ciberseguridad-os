import { TipoEvidenceV1 } from '../../features/evidence/evidence.models';

interface SearchItemBase {
  readonly id: string;
  readonly titulo: string;
  readonly contexto: string;
  readonly terminos: string;
}

export type SearchItem = SearchItemBase & (
  | { readonly tipo: 'Tema'; readonly ruta: string; readonly queryParams?: never }
  | { readonly tipo: 'Fase'; readonly ruta: '/roadmap'; readonly queryParams: { fase: string } }
  | { readonly tipo: 'Recurso'; readonly ruta: '/resources'; readonly queryParams: { recursoId: string } }
  | { readonly tipo: 'Evidence'; readonly ruta: '/evidence'; readonly queryParams: { evidenceId: string; tipoEvidence: TipoEvidenceV1 } }
);

export interface SearchGroup {
  readonly tipo: SearchItem['tipo'];
  readonly nombre: string;
  readonly items: readonly SearchItem[];
  readonly total: number;
}

export function normalizarBusqueda(texto: string): string {
  return texto.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim().replace(/\s+/g, ' ');
}

export function buscarEnIndice(indice: readonly SearchItem[], texto: string): SearchGroup[] {
  const query = normalizarBusqueda(texto);
  if (!query) return [];
  const tokens = query.split(' ');
  const coincidencias = indice.flatMap((item) => {
    const titulo = normalizarBusqueda(item.titulo);
    const terminos = normalizarBusqueda(`${item.titulo} ${item.contexto} ${item.terminos}`);
    if (!tokens.every((token) => terminos.includes(token))) return [];
    const prioridad = titulo.startsWith(query) ? 0 : titulo.includes(query) ? 1 : 2;
    return [{ item, prioridad }];
  }).sort((a, b) => a.prioridad - b.prioridad || a.item.titulo.localeCompare(b.item.titulo, 'es') || a.item.id.localeCompare(b.item.id));
  // Navegacion primaria primero; cada categoria conserva su ranking y hasta cinco resultados.
  return (['Tema', 'Fase', 'Recurso', 'Evidence'] as const).flatMap((tipo) => {
    const items = coincidencias.filter((resultado) => resultado.item.tipo === tipo).map(({ item }) => item);
    const nombres = { Tema: 'Temas', Fase: 'Fases', Recurso: 'Resources', Evidence: 'Evidence' };
    return items.length ? [{ tipo, nombre: nombres[tipo], items: items.slice(0, 5), total: items.length }] : [];
  });
}
