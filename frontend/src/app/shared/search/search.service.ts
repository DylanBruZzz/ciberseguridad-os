import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, map } from 'rxjs';
import { EvidenceService } from '../../features/evidence/evidence.service';
import { ResourcesService } from '../../features/resources/resources.service';
import { RoadmapService } from '../../features/roadmap/roadmap.service';
import { SearchItem, buscarEnIndice } from './search.models';

type Fuente = 'Roadmap' | 'Resources' | 'Evidence';
interface EstadoFuente {
  readonly items: readonly SearchItem[];
  readonly cargando: boolean;
  readonly error: boolean;
}

// Una instancia por apertura: cache local durante la palette, listas frescas al reabrir.
@Injectable()
export class SearchService {
  private readonly roadmap = inject(RoadmapService);
  private readonly resources = inject(ResourcesService);
  private readonly evidence = inject(EvidenceService);
  private readonly destroyRef = inject(DestroyRef);
  private iniciada = false;
  private readonly fuentes = signal<Record<Fuente, EstadoFuente>>({
    Roadmap: { items: [], cargando: false, error: false },
    Resources: { items: [], cargando: false, error: false },
    Evidence: { items: [], cargando: false, error: false },
  });
  public readonly indice = computed(() => Object.values(this.fuentes()).flatMap((fuente) => fuente.items));
  public readonly cargando = computed(() => Object.values(this.fuentes()).some((fuente) => fuente.cargando));
  public readonly errores = computed(() => (Object.keys(this.fuentes()) as Fuente[]).filter((nombre) => this.fuentes()[nombre].error));

  public cargar(): void {
    if (this.iniciada) return;
    this.iniciada = true;
    for (const fuente of ['Roadmap', 'Resources', 'Evidence'] as const) this.cargarFuente(fuente);
  }

  public reintentar(): void {
    for (const fuente of this.errores()) this.cargarFuente(fuente);
  }

  public buscar(texto: string) {
    return buscarEnIndice(this.indice(), texto);
  }

  private cargarFuente(nombre: Fuente): void {
    this.actualizarFuente(nombre, { items: [], cargando: true, error: false });
    this.solicitud(nombre).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (items) => this.actualizarFuente(nombre, { items, cargando: false, error: false }),
      error: () => this.actualizarFuente(nombre, { items: [], cargando: false, error: true }),
    });
  }

  private actualizarFuente(nombre: Fuente, estado: EstadoFuente): void {
    this.fuentes.update((fuentes) => ({ ...fuentes, [nombre]: estado }));
  }

  private solicitud(fuente: Fuente): Observable<SearchItem[]> {
    switch (fuente) {
      case 'Roadmap':
        return this.roadmap.obtenerVista().pipe(map((vista) => vista.fases.flatMap((fase): SearchItem[] => [
          { tipo: 'Fase', id: fase.id, titulo: fase.nombre, contexto: `Fase del Roadmap · ${fase.progresoPorcentaje}%`, terminos: `Fase ${fase.orden}`, ruta: '/roadmap', queryParams: { fase: fase.id } },
          ...fase.temas.map((tema): SearchItem => ({ tipo: 'Tema', id: tema.id, titulo: tema.nombre, contexto: `Tema · ${fase.nombre} · ${this.etiqueta(tema.estado)}`, terminos: tema.estado, ruta: `/roadmap/tema/${tema.id}` })),
        ])));
      case 'Resources':
        return this.resources.listar().pipe(map((recursos) => recursos.map((recurso): SearchItem => ({
          tipo: 'Recurso', id: recurso.id, titulo: recurso.titulo,
          contexto: `Resource · ${this.etiqueta(recurso.tipo)} · ${this.etiqueta(recurso.estado)}`,
          terminos: [recurso.url, recurso.tipo, recurso.estado, ...recurso.temas.map((tema) => tema.nombre)].join(' '),
          ruta: '/resources', queryParams: { recursoId: recurso.id },
        }))));
      case 'Evidence':
        return this.evidence.listar({}).pipe(map((lista) => lista.items.map((item): SearchItem => ({
          tipo: 'Evidence', id: item.id, titulo: item.titulo,
          contexto: `Evidence · ${this.etiqueta(item.tipoEvidence)} · ${this.etiqueta(item.estadoMadurez)}`,
          terminos: [item.tipoEvidence, item.estadoMadurez, ...item.temas.map((tema) => tema.nombre), ...item.herramientas.map((herramienta) => herramienta.nombre)].join(' '),
          ruta: '/evidence', queryParams: { evidenceId: item.id, tipoEvidence: item.tipoEvidence },
        }))));
    }
  }

  private etiqueta(valor: string): string {
    const labels: Readonly<Record<string, string>> = {
      NoIniciado: 'No iniciado', EnProgreso: 'En progreso', EnRepaso: 'En repaso',
      PorClasificar: 'Por clasificar', PorRevisar: 'Por revisar', EnUso: 'En uso',
      RepositorioGitHub: 'Repositorio GitHub', NotebookIA: 'Notebook IA',
      ArtefactoTecnico: 'Artefacto tecnico', CertificacionObtenida: 'Certificacion obtenida',
      ListoPortafolio: 'Listo para Portfolio',
    };
    return labels[valor] ?? valor;
  }
}
