import { TestBed } from '@angular/core/testing';
import { AnalyticsBars } from './analytics-bars';

describe('AnalyticsBars', () => {
  it('distribución vacía o completamente cero conserva escala finita', () => {
    const fixture = TestBed.createComponent(AnalyticsBars);
    fixture.componentRef.setInput('titulo', 'Sin actividad');
    fixture.componentRef.setInput('datos', [{ clave: 'a', etiqueta: 'Teoría', valor: 0 }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.innerHTML).not.toMatch(/NaN|Infinity/);
    expect(fixture.nativeElement.querySelector('.track span').style.width).toBe('0%');
    fixture.componentRef.setInput('datos', []);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('li')).toHaveLength(0);
  });
  it('el porcentaje de backend usa escala 100, sin normalizar al máximo de las Fases', () => {
    const fixture = TestBed.createComponent(AnalyticsBars);
    fixture.componentRef.setInput('titulo', 'Fases');
    fixture.componentRef.setInput('unidad', 'porcentaje');
    fixture.componentRef.setInput('datos', [{ clave: 'a', etiqueta: 'Fase A', valor: 37 }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.track span').style.width).toBe('37%');
    expect(fixture.nativeElement.textContent).toContain('37%');
  });
  it('barras de minutos conservan valor textual y proporción relativa', () => {
    const fixture = TestBed.createComponent(AnalyticsBars);
    fixture.componentRef.setInput('titulo', 'Tiempo');
    fixture.componentRef.setInput('unidad', 'minutos');
    fixture.componentRef.setInput('datos', [{ clave: 'a', etiqueta: 'Redes', valor: 90 }, { clave: 'b', etiqueta: 'Linux', valor: 30 }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Redes1 h 30 min');
    const barras = fixture.nativeElement.querySelectorAll('.track span');
    expect(barras[0].style.width).toBe('100%');
    expect(parseFloat(barras[1].style.width)).toBeCloseTo(100 / 3);
  });
});
