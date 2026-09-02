import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { App } from './app';
import { UsuarioActualService } from './core/usuario-actual.service';

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
          { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
        ]),
        { provide: UsuarioActualService, useValue: usuarioActual },
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
    expect(text).toContain('Search');
  });

  it('usa anchors para modulos y boton real para Search', async () => {
    await configure({
      obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('nav a');
    const search = fixture.nativeElement.querySelector('.search-trigger');

    expect(links.length).toBe(6);
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

    expect(fixture.nativeElement.textContent).toContain('Busqueda global');
    expect(fixture.nativeElement.textContent).toContain('Roadmap, Resources y Evidence');

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Busqueda global');
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
