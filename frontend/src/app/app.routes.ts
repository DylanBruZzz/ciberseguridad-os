import { Routes } from '@angular/router';

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
  {
    path: 'resources',
    loadComponent: () =>
      import('./features/resources/resources-page/resources-page').then((m) => m.ResourcesPage),
    data: { title: 'Resources' },
  },
  {
    path: 'evidence',
    loadComponent: () =>
      import('./features/evidence/evidence-page/evidence-page').then((m) => m.EvidencePage),
    data: { title: 'Evidence' },
  },
  {
    path: 'portfolio',
    loadComponent: () =>
      import('./features/portfolio/portfolio-page/portfolio-page').then((m) => m.PortfolioPage),
    data: { title: 'Portfolio' },
  },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' },
];
