import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
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
        provideRouter([{ path: 'roadmap', component: EmptyRouteComponent }]),
        { provide: UsuarioActualService, useValue: usuarioActual },
      ],
    }).compileComponents();
  }

  it('muestra el usuario actual en el shell', async () => {
    await configure({
      obtener: () => of({ id: 'usuario-1', nombre: 'Dylan' }),
    });

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Aprendizaje');
    expect(fixture.nativeElement.textContent).toContain('Dylan');
    expect(fixture.nativeElement.textContent).toContain('Roadmap');
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
