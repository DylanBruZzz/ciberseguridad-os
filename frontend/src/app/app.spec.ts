import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { App } from './app';
import { UsuarioActualService } from './core/usuario-actual.service';
import { RoadmapService } from './features/roadmap/roadmap.service';
import { ResourcesService } from './features/resources/resources.service';
import { EvidenceService } from './features/evidence/evidence.service';

@Component({ template: '' })
class EmptyRouteComponent {}

describe('App', () => {
  let fixture: ComponentFixture<App>;

  function configure(usuarioActual: Partial<UsuarioActualService>): Promise<void> {
    return TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([
          { path: 'dashboard', component: EmptyRouteComponent, data: { title: 'Dashboard' } },
          { path: 'roadmap', component: EmptyRouteComponent, data: { title: 'Roadmap' } },
          { path: 'study', component: EmptyRouteComponent, data: { title: 'Study' } },
          { path: 'resources', component: EmptyRouteComponent, data: { title: 'Resources' } },
          { path: 'evidence', component: EmptyRouteComponent, data: { title: 'Evidence' } },
          { path: 'portfolio', component: EmptyRouteComponent, data: { title: 'Portfolio' } },
          { path: 'analytics', component: EmptyRouteComponent, data: { title: 'Analytics' } },
          { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
        ]),
        { provide: UsuarioActualService, useValue: usuarioActual },
        { provide: RoadmapService, useValue: { obtenerVista: () => of({ fases: [] }) } },
        { provide: ResourcesService, useValue: { listar: () => of([]) } },
        { provide: EvidenceService, useValue: { listar: () => of({ items: [] }) } },
      ],
    }).compileComponents();
  }

  it('renderiza la shell visual con usuario actual', async () => {
    await configure({
      obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Ciberseguridad OS');
    expect(fixture.nativeElement.textContent).toContain('Ciberseguridad OS · Dashboard');
    expect(fixture.nativeElement.textContent).toContain('Dylan');
  });

  it('muestra todos los items oficiales del sidebar', async () => {
    await configure({
      obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Dashboard');
    expect(text).toContain('Roadmap');
    expect(text).toContain('Study');
    expect(text).toContain('Resources');
    expect(text).toContain('Evidence');
    expect(text).toContain('Portfolio');
    expect(text).toContain('Analytics');
    expect(text).toContain('Search');
  });

  it('Analytics sigue a Portfolio y actualiza el título y estado activo de la shell', async () => {
    await configure({ obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }) });
    fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await TestBed.inject(Router).navigateByUrl('/analytics');
    fixture.detectChanges();
    const links = fixture.nativeElement.querySelectorAll('nav a');
    expect(links[5].getAttribute('href')).toBe('/portfolio');
    expect(links[6].getAttribute('href')).toBe('/analytics');
    expect(links[6].classList.contains('active')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Ciberseguridad OS · Analytics');
  });

  it('usa anchors para modulos y boton real para Search', async () => {
    await configure({
      obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('nav a');
    const search = fixture.nativeElement.querySelector('.search-trigger');

    expect(links.length).toBe(7);
    expect(Array.from(links).every((link) => link instanceof HTMLAnchorElement)).toBe(true);
    expect(search instanceof HTMLButtonElement).toBe(true);
    expect(search.getAttribute('aria-label')).toBe('Abrir busqueda global');
  });

  it('actualiza topbar y active state al navegar', async () => {
    await configure({
      obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const router = TestBed.inject(Router);

    await router.navigateByUrl('/evidence');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Ciberseguridad OS · Evidence');
    const active = fixture.nativeElement.querySelector('nav a.active');
    expect(active?.textContent).toContain('Evidence');
  });

  it('abre y cierra la busqueda global con Ctrl+K y Escape', async () => {
    await configure({
      obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true }));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"]')).not.toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Escribe para buscar');

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeNull();
  });

  it.each(['button', 'meta'])('abre la misma palette desde %s y devuelve el foco', async (trigger) => {
    await configure({ obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }) });
    fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('.search-trigger');
    button.focus();
    if (trigger === 'button') button.click();
    else window.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', metaKey: true }));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('[role="dialog"]').length).toBe(1);
    expect(document.activeElement?.getAttribute('role')).toBe('combobox');
    expect(fixture.nativeElement.querySelector('.workspace').inert).toBe(true);
    const input = document.activeElement as HTMLInputElement;
    input.value = 'bash';
    input.dispatchEvent(new Event('input'));
    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true }));
    fixture.detectChanges();
    expect(input.value).toBe('bash');
    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve));
    expect(document.activeElement).toBe(button);
    expect(fixture.nativeElement.querySelector('.workspace').inert).toBe(false);
  });

  it('muestra error operativo cuando no puede resolver usuario actual', async () => {
    await configure({
      obtener: () => throwError(() => new Error('Configuracion local invalida')),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No se pudo resolver el usuario local');
    expect(fixture.nativeElement.textContent).toContain('Configuracion local invalida');
  });
});
