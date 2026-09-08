import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AnalyticsPage } from './analytics-page';
import { evidencia, fase, recurso, sesion, vista } from './testing/analytics.fixtures';

describe('AnalyticsPage con servicios HTTP reales', () => {
  let fixture: ComponentFixture<AnalyticsPage>;
  let http: HttpTestingController;
  let root: HTMLElement;
  const urls = ['/api/sesiones-estudio', '/api/roadmap/vista', '/api/evidence', '/api/recursos'];
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [AnalyticsPage], providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(AnalyticsPage);
    root = fixture.nativeElement;
    fixture.detectChanges();
  });
  afterEach(() => http.verify());
  const texto = () => root.textContent ?? '';
  function vacio(): void {
    http.expectOne(urls[0]).flush([]);
    http.expectOne(urls[1]).flush(vista([]));
    http.expectOne(urls[2]).flush({ total: 0, items: [] });
    http.expectOne(urls[3]).flush([]);
    fixture.detectChanges();
  }
  function exitoso(): void {
    http.expectOne(urls[0]).flush([sesion(), sesion({ duracionMinutos: 60, tipo: 'Practica' })]);
    http.expectOne(urls[1]).flush(vista());
    http.expectOne(urls[2]).flush({ total: 1, items: [evidencia({ estadoMadurez: 'ListoPortafolio' })] });
    http.expectOne(urls[3]).flush([recurso()]);
    fixture.detectChanges();
  }
  it('tiene skeleton y aria-busy por cada fuente durante loading', () => {
    expect(root.querySelectorAll('section[aria-busy="true"]')).toHaveLength(4);
    expect(root.querySelectorAll('.skeleton[role="status"]')).toHaveLength(4);
    vacio();
  });
  it('carga exactamente cuatro LIST/read-sides sin usuarioId, filtros o details', () => {
    const requests = http.match(() => true);
    expect(requests.map((r) => r.request.urlWithParams).sort()).toEqual([...urls].sort());
    expect(requests.every((r) => r.request.method === 'GET')).toBe(true);
    for (const r of requests) r.flush(r.request.url === urls[1] ? vista([]) : r.request.url === urls[2] ? { total: 0, items: [] } : []);
  });
  it('empty independiente sin una pared de tarjetas cero', () => {
    vacio();
    expect(root.querySelectorAll('.empty')).toHaveLength(4);
    expect(root.querySelector('dl')).toBeNull();
    expect(root.querySelector('app-analytics-bars')).toBeNull();
    expect(texto()).not.toMatch(/NaN|Infinity/);
    expect(root.querySelector('a[href="/study"]')).not.toBeNull();
  });
  it('presenta métricas y equivalentes textuales accesibles para los gráficos', () => {
    exitoso();
    expect(texto()).toContain('1 h 30 min');
    expect(texto()).toContain('45');
    expect(texto()).toContain('Media por sesión');
    expect(texto()).toContain('37%');
    expect(root.querySelector('ul[aria-label="Sesiones por tipo"]')?.textContent).toContain('Teoría1 sesión');
    expect(root.querySelector('ul[aria-label="Evidence por madurez"]')?.textContent).toContain('Listo para Portfolio1 registro');
    expect(root.querySelector('ul[aria-label="Resources por estado"]')?.textContent).toContain('Referencia1 registro');
    expect(root.querySelectorAll('.track[aria-hidden="true"]').length).toBeGreaterThan(0);
  });
  it('resuelve fuentes sin esperar Study: Roadmap y Resources visibles mientras Study carga', () => {
    http.expectOne(urls[1]).flush(vista());
    http.expectOne(urls[3]).flush([recurso()]);
    fixture.detectChanges();
    expect(texto()).toContain('Fundamentos');
    expect(texto()).toContain('1 recurso registrado');
    expect(texto()).toContain('Cargando Study');
    http.expectOne(urls[0]).flush([]);
    http.expectOne(urls[2]).flush({ total: 0, items: [] });
  });
  it.each([0, 1, 2, 3])('tolera fallo de fuente %s y reintenta únicamente esa fuente', (index) => {
    const nombres = ['Study', 'Roadmap', 'Evidence', 'Resources'];
    for (const [i, url] of urls.entries()) {
      const request = http.expectOne(url);
      if (i === index) request.flush({}, { status: 503, statusText: 'Unavailable' });
      else request.flush(i === 0 ? [sesion()] : i === 1 ? vista() : i === 2 ? { total: 1, items: [evidencia()] } : [recurso()]);
    }
    fixture.detectChanges();
    expect(root.querySelectorAll('.error')).toHaveLength(1);
    const button = [...root.querySelectorAll('button')].find((b) => b.textContent?.includes(`Reintentar ${nombres[index]}`))!;
    button.click();
    fixture.detectChanges();
    expect(root.querySelectorAll('.skeleton')).toHaveLength(1);
    button.click(); // Doble activación durante retry no crea otra solicitud.
    const request = http.expectOne(urls[index]);
    request.flush(index === 0 ? [sesion()] : index === 1 ? vista() : index === 2 ? { total: 1, items: [evidencia()] } : [recurso()]);
    fixture.detectChanges();
    expect(root.querySelector('.error')).toBeNull();
    expect(texto()).toContain('Fundamentos');
    expect(texto()).not.toMatch(/NaN|Infinity/);
    http.expectNone(() => true);
  });
  it('cambiar periodo sólo filtra Study localmente y mantiene Evidence/Roadmap/Resources', () => {
    http.expectOne(urls[0]).flush([sesion({ fecha: '2000-01-01' })]);
    http.expectOne(urls[1]).flush(vista());
    http.expectOne(urls[2]).flush({ total: 1, items: [evidencia()] });
    http.expectOne(urls[3]).flush([recurso()]);
    fixture.detectChanges();
    const button = [...root.querySelectorAll('.range button')].find((b) => b.textContent?.includes('30 días')) as HTMLButtonElement;
    button.click();
    fixture.detectChanges();
    expect(button.getAttribute('aria-pressed')).toBe('true');
    expect(texto()).toContain('Sin sesiones en este periodo');
    expect(texto()).toContain('Fundamentos');
    expect(texto()).toContain('1 evidencia registrada');
    expect(texto()).toContain('1 recurso registrado');
    http.expectNone(() => true);
  });
  it('ofrece encabezados, listas y controles nativos aptos para lectura móvil y teclado', () => {
    exitoso();
    expect(root.querySelectorAll('h1')).toHaveLength(1);
    expect(root.querySelectorAll('h2')).toHaveLength(4);
    for (const section of root.querySelectorAll('section')) {
      expect(root.querySelector(`#${section.getAttribute('aria-labelledby')}`)?.tagName).toBe('H2');
    }
    expect(root.querySelectorAll('details > summary')).toHaveLength(3);
    expect(root.querySelector('[role="group"][aria-label="Periodo de estudio"]')).not.toBeNull();
  });
  it('no genera NaN/Infinity en barras con ceros ni métricas inventadas', () => {
    exitoso();
    expect(root.innerHTML).not.toMatch(/NaN|Infinity/);
    expect(texto()).not.toMatch(/productividad|eficiencia|score|streak|ranking|disciplina|predicción|rendimiento|concentración|conversión/i);
    expect(root.querySelector('ul[aria-label="Sesiones por tipo"] .track:last-child')?.innerHTML).not.toContain('NaN');
  });
  it('nueva entrada a Analytics refresca las cuatro fuentes, incluido Roadmap cacheado', () => {
    exitoso();
    fixture.destroy();
    fixture = TestBed.createComponent(AnalyticsPage);
    root = fixture.nativeElement;
    fixture.detectChanges();
    vacio();
  });
  it('Fase sin Temas usa empty textual sin inventar estados o repaso', () => {
    http.expectOne(urls[0]).flush([]);
    http.expectOne(urls[1]).flush(vista([fase({ temas: [], totalTemas: 0 })]));
    http.expectOne(urls[2]).flush({ total: 0, items: [] });
    http.expectOne(urls[3]).flush([]);
    fixture.detectChanges();
    expect(texto()).toContain('Aún no hay Temas asociados');
    expect(root.querySelector('ul[aria-label="Temas por estado"]')).toBeNull();
  });
  it('un fallo total ofrece cuatro reintentos sin presentar ceros como datos cargados', () => {
    for (const url of urls) http.expectOne(url).flush({}, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(root.querySelectorAll('.error')).toHaveLength(4);
    expect(root.querySelector('.empty')).toBeNull();
    expect(root.querySelector('dl')).toBeNull();
    expect(root.querySelector('app-analytics-bars')).toBeNull();
  });
  it('reintentar Roadmap resuelve Tema/Fase sin volver a pedir Study ni alterar su total', () => {
    http.expectOne(urls[0]).flush([sesion()]);
    http.expectOne(urls[1]).flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne(urls[2]).flush({ total: 0, items: [] });
    http.expectOne(urls[3]).flush([]);
    fixture.detectChanges();
    expect(texto()).toContain('Tema no disponible en Roadmap');
    expect(texto()).toContain('Sin fase resuelta en Roadmap');
    const button = [...root.querySelectorAll('button')].find((b) => b.textContent?.includes('Reintentar Roadmap'))!;
    button.click();
    http.expectOne(urls[1]).flush(vista());
    fixture.detectChanges();
    expect(root.querySelector('ul[aria-label="Tiempo por Tema"]')?.textContent).toContain('Redes30 min');
    expect(root.querySelector('ul[aria-label="Tiempo por Fase"]')?.textContent).toContain('Fundamentos30 min');
    expect(root.querySelector('dl')?.textContent).toContain('30 min');
    http.expectNone(urls[0]);
  });
  it('destruir la página cancela listas pendientes y no inicia polling', () => {
    const requests = http.match(() => true);
    fixture.destroy();
    for (const request of requests) {
      // Roadmap mantiene su shareReplay raíz existente; las otras tres listas se cancelan.
      if (request.request.url === urls[1]) request.flush(vista([]));
      else expect(request.cancelled).toBe(true);
    }
    http.expectNone(() => true);
  });
});
