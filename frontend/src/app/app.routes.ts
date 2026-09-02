import { Routes } from '@angular/router';
import { PlaceholderPage } from './shared/placeholder-page/placeholder-page';

export const routes: Routes = [
  { path: 'dashboard', component: PlaceholderPage, data: { title: 'Dashboard' } },
  {
    path: 'roadmap',
    loadComponent: () =>
      import('./features/roadmap/roadmap-page/roadmap-page').then((m) => m.RoadmapPage),
    data: { title: 'Roadmap' },
  },
  { path: 'study', component: PlaceholderPage, data: { title: 'Study' } },
  { path: 'resources', component: PlaceholderPage, data: { title: 'Resources' } },
  { path: 'evidence', component: PlaceholderPage, data: { title: 'Evidence' } },
  { path: 'portfolio', component: PlaceholderPage, data: { title: 'Portfolio' } },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' },
];
