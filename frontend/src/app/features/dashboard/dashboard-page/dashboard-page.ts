import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UsuarioActual, UsuarioActualService } from '../../../core/usuario-actual.service';
import { FaseRoadmapVistaV1, RoadmapVistaV1, TemaRoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';

interface ContinuarAprendiendo {
  readonly titulo: string;
  readonly tema: TemaRoadmapVistaV1 | null;
  readonly fase: FaseRoadmapVistaV1 | null;
  readonly accion: string;
  readonly ruta: string;
  readonly queryParams?: Record<string, string>;
}

@Component({
  selector: 'app-dashboard-page',
  imports: [RouterLink],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.css',
})
export class DashboardPage implements OnInit {
  protected readonly usuario = signal<UsuarioActual | null>(null);
  protected readonly cargandoUsuario = signal(true);
  protected readonly vista = signal<RoadmapVistaV1 | null>(null);
  protected readonly cargandoRoadmap = signal(true);
  protected readonly errorRoadmap = signal<string | null>(null);

  protected readonly fases = computed(() => this.vista()?.fases ?? []);
  protected readonly faseActual = computed(() => {
    const vista = this.vista();

    if (!vista) {
      return null;
    }

    return vista.fases.find((fase) => fase.id === vista.faseActualId) ?? null;
  });
  protected readonly repasosRecomendados = computed(() =>
    this.fases().flatMap((fase) => fase.temas).filter((tema) => tema.repasoRecomendado).length,
  );
  protected readonly continuar = computed(() => this.obtenerContinuidad());

  public constructor(
    private readonly usuarioActual: UsuarioActualService,
    private readonly roadmap: RoadmapService,
  ) {}

  public ngOnInit(): void {
    this.usuarioActual.obtener().subscribe({
      next: (usuario) => {
        this.usuario.set(usuario);
        this.cargandoUsuario.set(false);
      },
      error: () => {
        this.usuario.set(null);
        this.cargandoUsuario.set(false);
      },
    });

    this.cargarRoadmap();
  }

  protected recargarRoadmap(): void {
    this.cargarRoadmap(true);
  }

  protected estadoFase(fase: FaseRoadmapVistaV1): 'completada' | 'actual' | 'pendiente' {
    if (fase.estaCompletada) {
      return 'completada';
    }

    if (fase.esFaseActual || fase.id === this.vista()?.faseActualId) {
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

  protected trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  private cargarRoadmap(forzarRefresco = false): void {
    this.cargandoRoadmap.set(true);
    this.errorRoadmap.set(null);

    const solicitud = forzarRefresco ? this.roadmap.refrescarVista() : this.roadmap.obtenerVista();

    solicitud.subscribe({
      next: (vista) => {
        this.vista.set({
          ...vista,
          fases: [...vista.fases].sort((a, b) => a.orden - b.orden),
        });
        this.errorRoadmap.set(null);
        this.cargandoRoadmap.set(false);
      },
      error: (error: Error) => {
        this.errorRoadmap.set(error.message);
        this.cargandoRoadmap.set(false);
      },
    });
  }

  private obtenerContinuidad(): ContinuarAprendiendo | null {
    const fases = this.fases();

    if (fases.length === 0) {
      return null;
    }

    const temas = fases.flatMap((fase) => fase.temas.map((tema) => ({ tema, fase })));
    const temaReciente = temas
      .filter((item) => item.tema.ultimaSesion)
      .sort((a, b) => {
        const fecha = b.tema.ultimaSesion!.localeCompare(a.tema.ultimaSesion!);

        if (fecha !== 0) {
          return fecha;
        }

        return a.tema.nombre.localeCompare(b.tema.nombre);
      })[0];

    if (temaReciente) {
      return {
        titulo: 'Continuar donde lo dejaste',
        tema: temaReciente.tema,
        fase: temaReciente.fase,
        accion: 'Continuar estudiando',
        ruta: '/study',
        queryParams: { temaId: temaReciente.tema.id },
      };
    }

    const faseActual = this.faseActual();

    if (faseActual) {
      return {
        titulo: 'Explorar fase actual',
        tema: null,
        fase: faseActual,
        accion: 'Ver fase',
        ruta: '/roadmap',
        queryParams: { fase: faseActual.id },
      };
    }

    return null;
  }
}
