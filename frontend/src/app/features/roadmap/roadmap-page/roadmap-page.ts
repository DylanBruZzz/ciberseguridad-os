import { Component, OnInit, computed, signal } from '@angular/core';
import { FaseResumen, TemaResumen } from '../roadmap.models';
import { RoadmapService } from '../roadmap.service';

@Component({
  selector: 'app-roadmap-page',
  imports: [],
  templateUrl: './roadmap-page.html',
  styleUrl: './roadmap-page.css',
})
export class RoadmapPage implements OnInit {
  protected readonly fases = signal<FaseResumen[]>([]);
  protected readonly temas = signal<TemaResumen[]>([]);
  protected readonly faseSeleccionadaId = signal<string | null>(null);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly faseSeleccionada = computed(() =>
    this.fases().find((fase) => fase.id === this.faseSeleccionadaId()) ?? this.fases()[0] ?? null,
  );

  protected readonly temasDeFase = computed(() => {
    const fase = this.faseSeleccionada();

    if (!fase) {
      return [];
    }

    return this.temas().filter((tema) => tema.faseId === fase.id && !tema.temaPadreId);
  });

  protected readonly totalTemas = computed(() => this.temas().length);

  public constructor(private readonly roadmap: RoadmapService) {}

  public ngOnInit(): void {
    this.roadmap.listarFases().subscribe({
      next: (fases) => {
        this.fases.set([...fases].sort((a, b) => a.orden - b.orden));
        this.faseSeleccionadaId.set(this.fases()[0]?.id ?? null);
        this.cargarTemas();
      },
      error: (error: Error) => {
        this.error.set(error.message);
        this.cargando.set(false);
      },
    });
  }

  protected seleccionarFase(faseId: string): void {
    this.faseSeleccionadaId.set(faseId);
  }

  private cargarTemas(): void {
    this.roadmap.listarTemas().subscribe({
      next: (temas) => {
        this.temas.set(temas);
        this.error.set(null);
        this.cargando.set(false);
      },
      error: (error: Error) => {
        this.error.set(error.message);
        this.cargando.set(false);
      },
    });
  }
}
