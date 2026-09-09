import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { crearFocoPanel } from './foco-panel';

@Component({ template: '<button (click)="abrir()">Abrir</button><aside class="detail-panel" tabindex="-1">Detalle</aside>' })
class PanelPrueba {
  readonly abrir = crearFocoPanel();
}

describe('Foco del detalle', () => {
  it.each([true, false])('lleva el teclado al panel y desplaza sólo si está apilado: %s', async (apilado) => {
    await TestBed.configureTestingModule({ imports: [PanelPrueba] }).compileComponents();
    const fixture = TestBed.createComponent(PanelPrueba);
    fixture.detectChanges();
    const host = fixture.nativeElement as HTMLElement;
    const panel = host.querySelector('aside')!;
    vi.spyOn(host, 'getBoundingClientRect').mockReturnValue({ width: 1000 } as DOMRect);
    vi.spyOn(panel, 'getBoundingClientRect').mockReturnValue({ width: apilado ? 1000 : 400 } as DOMRect);
    panel.scrollIntoView = vi.fn();
    host.querySelector('button')!.click();
    fixture.detectChanges();
    await fixture.whenStable();
    expect(document.activeElement).toBe(panel);
    expect(panel.scrollIntoView).toHaveBeenCalledTimes(apilado ? 1 : 0);
  });
});
