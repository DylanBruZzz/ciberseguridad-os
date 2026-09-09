import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FaseRoadmapVistaV1, RoadmapVistaV1, TemaRoadmapVistaV1 } from '../roadmap.models';
import { RoadmapService } from '../roadmap.service';
import { etiquetaEstadoTema } from '../roadmap.labels';

@Component({
  selector: 'app-roadmap-page',
  imports: [RouterLink],
  templateUrl: './roadmap-page.html',
  styleUrl: './roadmap-page.css',
})
export class RoadmapPage implements OnInit {
  protected readonly etiquetaEstadoTema = etiquetaEstadoTema;
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly vista = signal<RoadmapVistaV1 | null>(null);
  protected readonly faseSeleccionadaId = signal<string | null>(null);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly fases = computed(() => this.vista()?.fases ?? []);
  protected readonly faseActualId = computed(() => this.vista()?.faseActualId ?? null);
  protected readonly totalTemas = computed(() => this.vista()?.totalTemas ?? 0);
  protected readonly temasDominados = computed(() => this.vista()?.temasDominados ?? 0);

  protected readonly faseSeleccionada = computed(() =>
    this.fases().find((fase) => fase.id === this.faseSeleccionadaId()) ??
    this.fases().find((fase) => fase.id === this.faseActualId()) ??
    this.fases()[0] ??
    null,
  );

  protected readonly temasRaizDeFase = computed(() => {
    const fase = this.faseSeleccionada();

    if (!fase) {
      return [];
    }

    return fase.temas.filter((tema) => !tema.temaPadreId);
  });

  protected readonly temasPorPadre = computed(() => {
    const temas = this.faseSeleccionada()?.temas ?? [];

    return temas.reduce<Record<string, TemaRoadmapVistaV1[]>>((grupos, tema) => {
      if (!tema.temaPadreId) {
        return grupos;
      }

      grupos[tema.temaPadreId] = [...(grupos[tema.temaPadreId] ?? []), tema];

      return grupos;
    }, {});
  });

  public constructor(private readonly roadmap: RoadmapService) {}

  public ngOnInit(): void {
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const vista = this.vista();
      if (vista) this.seleccionarFaseInicial(vista);
    });
    this.cargarVista();
  }

  protected seleccionarFase(faseId: string): void {
    this.faseSeleccionadaId.set(faseId);
  }

  protected recargar(): void {
    this.cargarVista(true);
  }

  protected estadoFase(fase: FaseRoadmapVistaV1): 'completada' | 'actual' | 'pendiente' {
    if (fase.estaCompletada) {
      return 'completada';
    }

    if (fase.id === this.faseActualId() || fase.esFaseActual) {
      return 'actual';
    }

    return 'pendiente';
  }

  protected textoEstadoFase(fase: FaseRoadmapVistaV1): string {
    const estado = this.estadoFase(fase);

    if (estado === 'completada') {
      return 'Completada';
    }

    if (estado === 'actual') {
      return 'Fase actual';
    }

    return 'Pendiente';
  }

  protected hijosDeTema(temaId: string): TemaRoadmapVistaV1[] {
    return this.temasPorPadre()[temaId] ?? [];
  }

  protected trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  private cargarVista(forzarRefresco = false): void {
    this.cargando.set(true);
    this.error.set(null);

    const solicitud = forzarRefresco ? this.roadmap.refrescarVista() : this.roadmap.obtenerVista();

    solicitud.subscribe({
      next: (vista) => {
        this.vista.set({
          ...vista,
          fases: [...vista.fases].sort((a, b) => a.orden - b.orden),
        });
        this.seleccionarFaseInicial(vista);
        this.error.set(null);
        this.cargando.set(false);
      },
      error: (error: Error) => {
        this.error.set(error.message);
        this.cargando.set(false);
      },
    });
  }

  private seleccionarFaseInicial(vista: RoadmapVistaV1): void {
    const fasePorQuery = this.route.snapshot.queryParamMap.get('fase');
    const faseValida = vista.fases.find((fase) => fase.id === fasePorQuery)?.id;

    this.faseSeleccionadaId.set(faseValida ?? vista.faseActualId ?? vista.fases[0]?.id ?? null);
  }
}
