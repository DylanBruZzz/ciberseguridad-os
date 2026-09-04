import { Routes } from '@angular/router';
import { PlaceholderPage } from './shared/placeholder-page/placeholder-page';

export const routes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard-page/dashboard-page').then((m) => m.DashboardPage),
    data: { title: 'Dashboard' },
  },
  {
    path: 'roadmap',
    loadComponent: () =>
      import('./features/roadmap/roadmap-page/roadmap-page').then((m) => m.RoadmapPage),
    data: { title: 'Roadmap' },
  },
  {
    path: 'roadmap/tema/:temaId',
    loadComponent: () =>
      import('./features/roadmap/topic-workspace/topic-workspace-page').then(
        (m) => m.TopicWorkspacePage,
      ),
    data: { title: 'Roadmap' },
  },
  {
    path: 'study',
    loadComponent: () =>
      import('./features/study/study-page/study-page').then((m) => m.StudyPage),
    data: { title: 'Study' },
  },
  { path: 'resources', component: PlaceholderPage, data: { title: 'Resources' } },
  { path: 'evidence', component: PlaceholderPage, data: { title: 'Evidence' } },
  { path: 'portfolio', component: PlaceholderPage, data: { title: 'Portfolio' } },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' },
];
