import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { SearchPalette } from './search-palette';
import { SearchService } from './search.service';
import { SearchItem, buscarEnIndice } from './search.models';

@Component({ template: '' })
class Destination {}

const items: SearchItem[] = [
  { tipo: 'Tema', id: 'tema-1', titulo: 'Bash scripting', contexto: 'Tema · Fundamentos', terminos: 'redes', ruta: '/roadmap/tema/tema-1' },
  { tipo: 'Tema', id: 'tema-2', titulo: 'Modelo OSI', contexto: 'Tema · Fundamentos', terminos: 'redes', ruta: '/roadmap/tema/tema-2' },
  { tipo: 'Fase', id: 'fase-1', titulo: 'Fundamentos', contexto: 'Fase del Roadmap', terminos: 'redes', ruta: '/roadmap', queryParams: { fase: 'fase-1' } },
  { tipo: 'Recurso', id: 'recurso-1', titulo: 'Curso redes', contexto: 'Resource · Curso', terminos: 'redes', ruta: '/resources', queryParams: { recursoId: 'recurso-1' } },
  { tipo: 'Evidence', id: 'evidence-1', titulo: 'Laboratorio redes', contexto: 'Evidence · Laboratorio · Documentado', terminos: 'redes', ruta: '/evidence', queryParams: { evidenceId: 'evidence-1', tipoEvidence: 'Laboratorio' } },
];

describe('SearchPalette', () => {
  let fixture: ComponentFixture<SearchPalette>;
  let service: { cargar: ReturnType<typeof vi.fn>; reintentar: ReturnType<typeof vi.fn>; buscar: typeof buscar; cargando: ReturnType<typeof signal<boolean>>; errores: ReturnType<typeof signal<string[]>> };
  const buscar = (query: string) => buscarEnIndice(items, query);
  const input = () => fixture.nativeElement.querySelector('input') as HTMLInputElement;
  const opciones = () => [...fixture.nativeElement.querySelectorAll('[role="option"]')] as HTMLButtonElement[];
  const texto = () => fixture.nativeElement.textContent as string;
  function escribir(query: string): void {
    input().value = query;
    input().dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }
  function tecla(key: string, shiftKey = false): void {
    document.activeElement?.dispatchEvent(new KeyboardEvent('keydown', { key, shiftKey, bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }
  beforeEach(async () => {
    service = { cargar: vi.fn(), reintentar: vi.fn(), buscar, cargando: signal(false), errores: signal<string[]>([]) };
    await TestBed.configureTestingModule({
      imports: [SearchPalette],
      providers: [provideRouter([
        { path: 'roadmap/tema/:temaId', component: Destination },
        { path: 'roadmap', component: Destination },
        { path: 'resources', component: Destination },
        { path: 'evidence', component: Destination },
      ])],
    }).overrideComponent(SearchPalette, { set: { providers: [{ provide: SearchService, useValue: service }] } }).compileComponents();
    fixture = TestBed.createComponent(SearchPalette);
    fixture.detectChanges();
  });

  it('autofocus, dialog, combobox etiquetado y query vacia sin resultados', () => {
    expect(document.activeElement).toBe(input());
    expect(fixture.nativeElement.querySelector('[role="dialog"]').getAttribute('aria-modal')).toBe('true');
    expect(input().getAttribute('aria-label')).toBe('Buscar en Ciberseguridad OS');
    expect(input().getAttribute('aria-controls')).toBe('search-results');
    expect(input().hasAttribute('aria-activedescendant')).toBe(false);
    expect(texto()).toContain('Escribe para buscar');
    expect(opciones()).toHaveLength(0);
    expect(service.cargar).toHaveBeenCalledOnce();
  });

  it('typing presenta grupos y seleccion accesible', () => {
    escribir('redes');
    expect([...fixture.nativeElement.querySelectorAll('[role="group"] h2')].map((h) => (h as HTMLElement).textContent?.trim())).toEqual(['Temas 2', 'Fases 1', 'Resources 1', 'Evidence 1']);
    expect(opciones()).toHaveLength(5);
    expect(opciones().every((option) => option instanceof HTMLButtonElement)).toBe(true);
    expect(input().getAttribute('aria-activedescendant')).toBe(opciones()[0].id);
    expect(opciones()[0].getAttribute('aria-selected')).toBe('true');
  });

  it('ArrowDown y ArrowUp recorren categorias con wrap y mantienen el foco', () => {
    escribir('redes');
    tecla('ArrowDown');
    expect(input().getAttribute('aria-activedescendant')).toBe(opciones()[1].id);
    tecla('ArrowUp');
    tecla('ArrowUp');
    expect(input().getAttribute('aria-activedescendant')).toBe(opciones()[4].id);
    expect(document.activeElement).toBe(input());
    escribir('bash');
    expect(input().getAttribute('aria-activedescendant')).toBe(opciones()[0].id);
  });

  it('no results no deja una seleccion stale ni navega con Enter', () => {
    escribir('bash');
    escribir('inexistente');
    tecla('ArrowDown');
    tecla('ArrowUp');
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');
    tecla('Enter');
    expect(texto()).toContain('No encontramos resultados para "inexistente"');
    expect(input().hasAttribute('aria-activedescendant')).toBe(false);
    expect(navigate).not.toHaveBeenCalled();
  });

  it.each([
    ['bash', '/roadmap/tema/tema-1'],
    ['fundamentos', '/roadmap?fase=fase-1'],
    ['curso', '/resources?recursoId=recurso-1'],
    ['laboratorio', '/evidence?evidenceId=evidence-1&tipoEvidence=Laboratorio'],
  ])('click navega desde %s a su destino factual', async (query, expectedUrl) => {
    escribir(query);
    const cerrar = vi.fn();
    fixture.componentInstance.cerrar.subscribe(cerrar);
    const option = query === 'fundamentos' ? opciones().find((o) => o.id.startsWith('search-Fase'))! : opciones()[0];
    option.click();
    await fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe(expectedUrl);
    expect(cerrar).toHaveBeenCalled();
  });

  it('Enter abre el resultado activo y cierra la palette', async () => {
    escribir('redes');
    tecla('ArrowDown');
    const cerrar = vi.fn();
    fixture.componentInstance.cerrar.subscribe(cerrar);
    tecla('Enter');
    await fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/roadmap/tema/tema-2');
    expect(cerrar).toHaveBeenCalled();
  });

  it('Tab y Shift+Tab quedan dentro; Escape cierra', () => {
    const cerrar = vi.fn();
    fixture.componentInstance.cerrar.subscribe(cerrar);
    tecla('Tab', true);
    expect(document.activeElement?.getAttribute('aria-label')).toBe('Cerrar busqueda global');
    tecla('Tab');
    expect(document.activeElement).toBe(input());
    tecla('Escape');
    expect(cerrar).toHaveBeenCalledOnce();
  });

  it('boton de cierre funciona sin shortcut y restaura scroll al destruir', () => {
    const cerrar = vi.fn();
    fixture.componentInstance.cerrar.subscribe(cerrar);
    (fixture.nativeElement.querySelector('.close-search') as HTMLButtonElement).click();
    expect(cerrar).toHaveBeenCalledOnce();
    expect(document.body.style.overflow).toBe('hidden');
    fixture.destroy();
    expect(document.body.style.overflow).not.toBe('hidden');
  });

  it('loading y fallo parcial conservan resultados; retry es accion real', () => {
    service.cargando.set(true);
    service.errores.set(['Resources']);
    escribir('bash');
    expect(texto()).toContain('Cargando fuentes');
    expect(texto()).toContain('No pudimos cargar Resources');
    expect(opciones()).toHaveLength(1);
    (fixture.nativeElement.querySelector('.retry') as HTMLButtonElement).click();
    expect(service.reintentar).toHaveBeenCalledOnce();
  });

  it('no cierra ante navegacion fallida e informa error recuperable', async () => {
    vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(false);
    const cerrar = vi.fn();
    fixture.componentInstance.cerrar.subscribe(cerrar);
    escribir('bash');
    opciones()[0].click();
    await fixture.whenStable();
    expect(cerrar).not.toHaveBeenCalled();
    expect(texto()).toContain('No pudimos abrir el resultado');
  });

  it('composicion IME no abre resultados con Enter', () => {
    escribir('bash');
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');
    input().dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', isComposing: true, bubbles: true }));
    expect(navigate).not.toHaveBeenCalled();
  });
});
