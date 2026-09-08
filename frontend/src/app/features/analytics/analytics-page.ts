import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { formatearDuracion } from '../../shared/format/duracion';
import { AnalyticsBars } from './analytics-bars';
import { agregarEstudio, agregarEvidence, agregarRecursos, agregarRoadmap, fechaLocal, filtrarSesiones } from './analytics.helpers';
import { RangoEstudio } from './analytics.models';
import { AnalyticsService } from './analytics.service';

@Component({
  selector: 'app-analytics-page',
  imports: [AnalyticsBars, DecimalPipe, RouterLink],
  providers: [AnalyticsService],
  templateUrl: './analytics-page.html',
  styleUrl: './analytics-page.css',
})
export class AnalyticsPage implements OnInit {
  protected readonly fuentes = inject(AnalyticsService);
  protected readonly rango = signal<RangoEstudio>('Todo');
  protected readonly rangos = ['Todo', 30, 90] as const;
  private readonly hoy = fechaLocal(new Date());
  protected readonly duracion = formatearDuracion;
  protected readonly study = computed(() => {
    const datos = this.fuentes.study().datos;
    return datos ? agregarEstudio(filtrarSesiones(datos, this.rango(), this.hoy), this.fuentes.roadmap().datos) : null;
  });
  protected readonly roadmap = computed(() => {
    const datos = this.fuentes.roadmap().datos;
    return datos ? agregarRoadmap(datos) : null;
  });
  protected readonly evidence = computed(() => {
    const datos = this.fuentes.evidence().datos;
    return datos ? agregarEvidence(datos.items) : null;
  });
  protected readonly resources = computed(() => {
    const datos = this.fuentes.resources().datos;
    return datos ? agregarRecursos(datos) : null;
  });
  protected readonly temasDestacados = computed(() => this.study()?.porTema.slice(0, 5) ?? []);
  public ngOnInit(): void { this.fuentes.cargar(); }
}
