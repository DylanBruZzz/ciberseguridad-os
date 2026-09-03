import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-topic-placeholder-page',
  imports: [RouterLink],
  template: `
    <section class="topic-placeholder">
      <p class="eyebrow">Workspace del tema</p>
      <h1>Workspace en preparacion</h1>
      <p>El detalle del tema se conectara al read-side TemaWorkspaceV1 en el siguiente bloque.</p>
      <a routerLink="/roadmap">Volver al Roadmap</a>
    </section>
  `,
  styles: `
    .topic-placeholder {
      display: grid;
      gap: var(--space-4);
      max-width: 42rem;
      padding: var(--space-2) 0;
    }

    .eyebrow,
    h1,
    p {
      margin: 0;
    }

    .eyebrow {
      color: var(--accent-bright);
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    h1 {
      color: var(--text-primary);
      font-size: clamp(2rem, 5vw, 4rem);
      line-height: 1;
    }

    p {
      color: var(--text-secondary);
      line-height: 1.6;
    }

    a {
      display: inline-grid;
      width: fit-content;
      min-height: 2.4rem;
      place-items: center;
      padding: 0 var(--space-3);
      border: 1px solid rgba(139, 92, 246, 0.45);
      border-radius: 8px;
      color: var(--text-primary);
      background: rgba(124, 58, 237, 0.18);
      text-decoration: none;
    }

    a:focus-visible {
      outline: 2px solid var(--accent-bright);
      outline-offset: 3px;
    }
  `,
})
export class TopicPlaceholderPage {}
