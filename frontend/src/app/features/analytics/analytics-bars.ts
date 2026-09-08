import { Component, computed, input } from '@angular/core';
import { formatearDuracion } from '../../shared/format/duracion';
import { DatoGrafico } from './analytics.models';

@Component({
  selector: 'app-analytics-bars',
  template: `
    <ul [attr.aria-label]="titulo()">
      @for (fila of filas(); track fila.clave) {
        <li>
          <div class="label"><span>{{ fila.etiqueta }}</span><strong>{{ fila.texto }}</strong></div>
          <div class="track" aria-hidden="true"><span [style.width.%]="fila.ancho"></span></div>
        </li>
      }
    </ul>
  `,
  styles: `
    :host { display: block; min-width: 0; }
    ul { list-style: none; padding: 0; margin: 0; display: grid; gap: 1rem; }
    li { min-width: 0; }
    .label { display: flex; justify-content: space-between; align-items: baseline; gap: .75rem; font-size: .85rem; line-height: 1.5; }
    .label > span { color: var(--text-secondary); overflow-wrap: anywhere; }
    strong { flex-shrink: 0; font-weight: 500; font-variant-numeric: tabular-nums; }
    .track { margin-top: .4rem; height: .3rem; background: var(--bg-surface-3); border-radius: 1rem; overflow: hidden; }
    .track span { display: block; height: 100%; background: var(--accent-bright); border-radius: inherit; }
  `,
})
export class AnalyticsBars {
  readonly datos = input.required<readonly DatoGrafico[]>();
  readonly titulo = input.required<string>();
  readonly unidad = input<'minutos' | 'porcentaje' | 'sesiones' | 'temas' | 'registros'>('registros');
  private readonly maximo = computed(() => this.unidad() === 'porcentaje' ? 100 : Math.max(0, ...this.datos().map((d) => d.valor)));
  readonly filas = computed(() => this.datos().map((dato) => ({
    ...dato,
    ancho: this.maximo() > 0 ? dato.valor / this.maximo() * 100 : 0,
    texto: this.unidad() === 'minutos' ? formatearDuracion(dato.valor)
      : this.unidad() === 'porcentaje' ? `${dato.valor}%` : `${dato.valor} ${dato.valor === 1 ? ({ sesiones: 'sesión', temas: 'tema', registros: 'registro', minutos: 'minuto', porcentaje: '%' }[this.unidad()]) : this.unidad()}`,
  })));
}
