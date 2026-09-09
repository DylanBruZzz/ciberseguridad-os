// Etiquetas de presentación; el estado siempre procede del read-side.
export function etiquetaEstadoTema(estado: string): string {
  const etiquetas: Record<string, string> = {
    NoIniciado: 'No iniciado',
    EnPractica: 'En práctica',
    Dominado: 'Dominado',
    EnRepaso: 'En repaso',
  };
  return etiquetas[estado] ?? estado;
}
