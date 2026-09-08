import { DestroyRef, Injectable, WritableSignal, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';
import { EvidenceListaV1 } from '../evidence/evidence.models';
import { EvidenceService } from '../evidence/evidence.service';
import { RecursoResumen } from '../resources/resources.models';
import { ResourcesService } from '../resources/resources.service';
import { RoadmapVistaV1 } from '../roadmap/roadmap.models';
import { RoadmapService } from '../roadmap/roadmap.service';
import { SesionEstudioResumen } from '../study/study.models';
import { StudyService } from '../study/study.service';

export type FuenteAnalytics = 'Study' | 'Roadmap' | 'Evidence' | 'Resources';
type EstadoFuente<T> = { estado: 'cargando' | 'error'; datos: null } | { estado: 'listo'; datos: T };

// Instancia por entrada a la ruta. Cada fuente se resuelve y reintenta independientemente.
@Injectable()
export class AnalyticsService {
  private readonly destroyRef = inject(DestroyRef);
  private readonly studyApi = inject(StudyService);
  private readonly roadmapApi = inject(RoadmapService);
  private readonly evidenceApi = inject(EvidenceService);
  private readonly resourcesApi = inject(ResourcesService);
  private readonly studyEstado = signal<EstadoFuente<SesionEstudioResumen[]>>({ estado: 'cargando', datos: null });
  private readonly roadmapEstado = signal<EstadoFuente<RoadmapVistaV1>>({ estado: 'cargando', datos: null });
  private readonly evidenceEstado = signal<EstadoFuente<EvidenceListaV1>>({ estado: 'cargando', datos: null });
  private readonly resourcesEstado = signal<EstadoFuente<RecursoResumen[]>>({ estado: 'cargando', datos: null });
  readonly study = this.studyEstado.asReadonly();
  readonly roadmap = this.roadmapEstado.asReadonly();
  readonly evidence = this.evidenceEstado.asReadonly();
  readonly resources = this.resourcesEstado.asReadonly();
  private iniciada = false;

  cargar(): void {
    if (this.iniciada) return;
    this.iniciada = true;
    for (const fuente of ['Study', 'Roadmap', 'Evidence', 'Resources'] as const) this.cargarFuente(fuente);
  }

  reintentar(fuente: FuenteAnalytics): void {
    const estado = { Study: this.study(), Roadmap: this.roadmap(), Evidence: this.evidence(), Resources: this.resources() }[fuente];
    if (estado.estado === 'error') this.cargarFuente(fuente);
  }

  private cargarFuente(fuente: FuenteAnalytics): void {
    switch (fuente) {
      case 'Study': this.cargarEstado(this.studyEstado, this.studyApi.listarSesiones()); break;
      case 'Roadmap': this.cargarEstado(this.roadmapEstado, this.roadmapApi.refrescarVista()); break;
      case 'Evidence': this.cargarEstado(this.evidenceEstado, this.evidenceApi.listar({})); break;
      case 'Resources': this.cargarEstado(this.resourcesEstado, this.resourcesApi.listar()); break;
    }
  }

  private cargarEstado<T>(destino: WritableSignal<EstadoFuente<T>>, solicitud: Observable<T>): void {
    destino.set({ estado: 'cargando', datos: null });
    solicitud.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (datos) => destino.set({ estado: 'listo', datos }),
      error: () => destino.set({ estado: 'error', datos: null }),
    });
  }
}
