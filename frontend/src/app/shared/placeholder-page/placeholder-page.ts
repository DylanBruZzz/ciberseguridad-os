import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-placeholder-page',
  template: `
    <section class="placeholder">
      <div>
        <p class="eyebrow">Vista en preparacion</p>
        <h1>{{ title }}</h1>
      </div>

      <p class="summary">{{ description }}</p>

      <div class="placeholder-grid" aria-label="Estructura reservada">
        <section>
          <span></span>
          <strong>{{ primarySlot }}</strong>
          <p>{{ primaryText }}</p>
        </section>
        <section>
          <span></span>
          <strong>Contexto</strong>
          <p>El modulo usara datos factuales de la API Personal cuando se implemente la vista completa.</p>
        </section>
      </div>
    </section>
  `,
  styles: `
    .placeholder {
      display: grid;
      min-height: 26rem;
      align-content: start;
      gap: var(--space-6);
      padding: var(--space-2) 0;
    }

    .eyebrow {
      margin: 0 0 var(--space-2);
      color: var(--accent-bright);
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    h1 {
      margin: 0;
      color: var(--text-primary);
      font-size: clamp(2rem, 6vw, 4.5rem);
      line-height: 0.96;
    }

    .summary {
      max-width: 42rem;
      margin: 0;
      color: var(--text-secondary);
      font-size: 1rem;
      line-height: 1.7;
    }

    .placeholder-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--space-4);
      max-width: 48rem;
    }

    .placeholder-grid section {
      display: grid;
      gap: var(--space-3);
      min-height: 10rem;
      padding: var(--space-4);
      border: 1px solid var(--border-subtle);
      border-radius: 8px;
      background: var(--bg-surface-2);
    }

    .placeholder-grid span {
      width: 2rem;
      height: 0.2rem;
      border-radius: 999px;
      background: var(--accent);
    }

    .placeholder-grid strong {
      color: var(--text-primary);
      font-size: 1rem;
    }

    .placeholder-grid p {
      margin: 0;
      color: var(--text-muted);
      line-height: 1.55;
    }

    @media (max-width: 720px) {
      .placeholder {
        padding: 0;
      }

      .placeholder-grid {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class PlaceholderPage {
  private readonly route = inject(ActivatedRoute);
  protected readonly title = this.route.snapshot.data['title'] ?? 'Seccion';
  protected readonly description = this.obtenerDescripcion(this.title);
  protected readonly primarySlot = this.obtenerSlot(this.title);
  protected readonly primaryText = this.obtenerTextoPrimario(this.title);

  private obtenerDescripcion(title: string): string {
    const descriptions: Record<string, string> = {
      Dashboard: 'Panel principal reservado para progreso global, fase actual y continuidad de estudio sin inventar metricas antes del bloque correspondiente.',
      Study: 'Espacio preparado para sesiones de estudio, correcciones y seguimiento factual de practica.',
      Resources: 'Biblioteca preparada para consultar y organizar recursos vinculados al roadmap.',
      Evidence: 'Indice visual preparado para los cinco tipos de Evidence y sus filtros contextuales.',
      Portfolio: 'Vista preparada para presentar piezas listas de portafolio sin mezclar borradores.',
    };

    return descriptions[title] ?? 'Vista preparada para el siguiente bloque funcional.';
  }

  private obtenerSlot(title: string): string {
    const slots: Record<string, string> = {
      Dashboard: 'Progreso global',
      Study: 'Continuar sesion',
      Resources: 'Biblioteca',
      Evidence: 'Galeria',
      Portfolio: 'Presentacion',
    };

    return slots[title] ?? 'Modulo';
  }

  private obtenerTextoPrimario(title: string): string {
    const textos: Record<string, string> = {
      Dashboard: 'Los datos de progreso se integraran cuando el Dashboard real quede definido.',
      Study: 'El temporizador y el flujo de sesiones quedan diferidos.',
      Resources: 'Los filtros y listas profundas se conectaran en su bloque V1.',
      Evidence: 'La lista se conectara luego a GET /api/evidence.',
      Portfolio: 'El read-side de Portfolio ya existe; la pantalla visual queda pendiente.',
    };

    return textos[title] ?? 'Contenido reservado sin datos simulados.';
  }
}
