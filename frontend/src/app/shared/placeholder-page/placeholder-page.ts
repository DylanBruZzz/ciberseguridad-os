import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-placeholder-page',
  template: `
    <section class="placeholder">
      <p>Modulo pendiente</p>
      <h1>{{ title }}</h1>
    </section>
  `,
  styles: `
    .placeholder {
      min-height: 340px;
      padding: 1.2rem;
      border: 1px solid #d7e2da;
      border-radius: 8px;
      background: #ffffff;
    }

    p {
      margin: 0 0 0.35rem;
      color: #5f746c;
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    h1 {
      margin: 0;
      color: #17211d;
      font-size: 2rem;
    }
  `,
})
export class PlaceholderPage {
  private readonly route = inject(ActivatedRoute);
  protected readonly title = this.route.snapshot.data['title'] ?? 'Seccion';
}
