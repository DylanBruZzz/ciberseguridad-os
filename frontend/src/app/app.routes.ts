import { Routes } from '@angular/router';
import { PlaceholderPage } from './shared/placeholder-page/placeholder-page';

export const routes: Routes = [
  {
    path: 'roadmap',
    loadComponent: () =>
      import('./features/roadmap/roadmap-page/roadmap-page').then((m) => m.RoadmapPage),
  },
  { path: 'study', component: PlaceholderPage, data: { title: 'Study' } },
  { path: 'resources', component: PlaceholderPage, data: { title: 'Resources' } },
  { path: 'evidence', component: PlaceholderPage, data: { title: 'Evidence' } },
  { path: 'portfolio', component: PlaceholderPage, data: { title: 'Portfolio' } },
  { path: '', pathMatch: 'full', redirectTo: 'roadmap' },
  { path: '**', redirectTo: 'roadmap' },
];
