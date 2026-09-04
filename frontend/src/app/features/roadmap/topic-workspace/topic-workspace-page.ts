import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  TemaWorkspaceCriterio,
  TemaWorkspaceEvidenceResumen,
  TemaWorkspaceV1,
} from './tema-workspace.models';
import { TemaWorkspaceService } from './tema-workspace.service';

type EstadoGuardado = 'idle' | 'guardando' | 'guardado' | 'error';

@Component({
  selector: 'app-topic-workspace-page',
  imports: [RouterLink],
  templateUrl: './topic-workspace-page.html',
  styleUrl: './topic-workspace-page.css',
})
export class TopicWorkspacePage implements OnInit {
  private static readonly guidRegex =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

  protected readonly temaId = signal<string | null>(null);
  protected readonly workspace = signal<TemaWorkspaceV1 | null>(null);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly borradorApuntes = signal('');
  protected readonly estadoGuardado = signal<EstadoGuardado>('idle');
  protected readonly errorGuardado = signal<string | null>(null);

  protected readonly tema = computed(() => this.workspace()?.tema ?? null);
  protected readonly fase = computed(() => this.workspace()?.fase ?? null);
  protected readonly apuntes = computed(() => this.workspace()?.apuntes ?? null);
  protected readonly ultimaSesion = computed(() => this.workspace()?.ultimaSesion ?? null);
  protected readonly repaso = computed(() => this.workspace()?.repaso ?? null);
  protected readonly hayCambiosApuntes = computed(
    () => this.borradorApuntes() !== (this.apuntes()?.contenido ?? ''),
  );

  public constructor(
    private readonly route: ActivatedRoute,
    private readonly workspaceService: TemaWorkspaceService,
  ) {}

  public ngOnInit(): void {
    const temaId = this.route.snapshot.paramMap.get('temaId');
    this.temaId.set(temaId);

    if (!temaId || !TopicWorkspacePage.guidRegex.test(temaId)) {
      this.cargando.set(false);
      this.error.set('Este tema no esta disponible.');
      return;
    }

    this.cargarWorkspace();
  }

  protected recargar(): void {
    this.cargarWorkspace();
  }

  protected onApuntesInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLTextAreaElement) {
      this.borradorApuntes.set(target.value);
      this.estadoGuardado.set('idle');
      this.errorGuardado.set(null);
    }
  }

  protected guardarApuntes(): void {
    const temaId = this.temaId();

    if (!temaId || !this.hayCambiosApuntes() || this.estadoGuardado() === 'guardando') {
      return;
    }

    const contenido = this.borradorApuntes();
    this.estadoGuardado.set('guardando');
    this.errorGuardado.set(null);

    this.workspaceService.guardarApuntes(temaId, contenido).subscribe({
      next: () => {
        const actual = this.workspace();

        if (actual) {
          this.workspace.set({
            ...actual,
            apuntes: {
              ...actual.apuntes,
              contenido,
            },
          });
        }

        this.estadoGuardado.set('guardado');
      },
      error: (error: Error) => {
        this.errorGuardado.set(error.message);
        this.estadoGuardado.set('error');
      },
    });
  }

  protected textoVolverRoadmap(): string {
    return this.fase() ? `Volver a ${this.fase()!.nombre}` : 'Volver al Roadmap';
  }

  protected textoPercepcion(valor: number | null, etiqueta: string): string {
    return valor === null ? `${etiqueta} no definida` : `${etiqueta} ${valor}/5`;
  }

  protected textoUltimaSesion(): string {
    const sesion = this.ultimaSesion();

    if (!sesion) {
      return 'Nunca registraste una sesion para este tema.';
    }

    return `${this.formatearFechaSimple(sesion.fecha)} · ${sesion.duracionMinutos} min · ${sesion.tipo}`;
  }

  protected textoProximoRepaso(): string {
    const repaso = this.repaso();

    if (!repaso?.proximaFechaRepaso) {
      return 'Sin proxima fecha de repaso.';
    }

    return this.formatearFechaSimple(repaso.proximaFechaRepaso);
  }

  protected fechaSimple(fecha: string | null): string {
    return fecha ? this.formatearFechaSimple(fecha) : 'Sin fecha';
  }

  protected fechaHora(fecha: string | null): string {
    return fecha ? this.formatearFechaHora(fecha) : 'Sin guardar';
  }

  protected evidenciaDesglose(resumen: TemaWorkspaceEvidenceResumen): string {
    const partes = [
      this.textoConteo(resumen.proyectos, 'proyecto', 'proyectos'),
      this.textoConteo(resumen.laboratorios, 'laboratorio', 'laboratorios'),
      this.textoConteo(resumen.writeups, 'writeup', 'writeups'),
      this.textoConteo(resumen.artefactosTecnicos, 'artefacto', 'artefactos'),
      this.textoConteo(resumen.certificacionesObtenidas, 'certificacion', 'certificaciones'),
    ].filter(Boolean);

    return partes.length > 0 ? partes.join(' · ') : 'Sin evidence vinculada';
  }

  protected trackByCriterio(_: number, criterio: TemaWorkspaceCriterio): string {
    return criterio.id;
  }

  private cargarWorkspace(): void {
    const temaId = this.temaId();

    if (!temaId) {
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.estadoGuardado.set('idle');
    this.errorGuardado.set(null);

    this.workspaceService.obtenerWorkspace(temaId).subscribe({
      next: (workspace) => {
        this.workspace.set(workspace);
        this.borradorApuntes.set(workspace.apuntes.contenido);
        this.cargando.set(false);
      },
      error: (error: Error) => {
        this.workspace.set(null);
        this.error.set(error.message);
        this.cargando.set(false);
      },
    });
  }

  private textoConteo(total: number, singular: string, plural: string): string | null {
    if (total === 0) {
      return null;
    }

    return `${total} ${total === 1 ? singular : plural}`;
  }

  private formatearFechaSimple(fecha: string): string {
    const partes = /^(\d{4})-(\d{2})-(\d{2})$/.exec(fecha);

    if (partes) {
      const [, year, month, day] = partes;
      return new Intl.DateTimeFormat('es-PE', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      }).format(new Date(Number(year), Number(month) - 1, Number(day)));
    }

    return this.formatearFechaHora(fecha);
  }

  private formatearFechaHora(fecha: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(new Date(fecha));
  }
}
