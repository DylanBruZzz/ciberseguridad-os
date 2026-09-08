import { formatearDuracion } from '../../shared/format/duracion';
import { ESTADOS_MADUREZ, TIPOS_EVIDENCE } from '../evidence/evidence.models';
import { ESTADOS_RECURSO, TIPOS_RECURSO } from '../resources/resources.models';
import { TIPOS_SESION } from '../study/study.models';
import { agregarEstudio, agregarEvidence, agregarRecursos, agregarRoadmap, fechaLocal, filtrarSesiones } from './analytics.helpers';
import { evidencia, fase, recurso, sesion, tema, vista } from './testing/analytics.fixtures';

describe('Analytics: agregación factual pura', () => {
  it('0 sesiones produce total y media cero sin NaN ni Infinity', () => {
    const datos = agregarEstudio([], null);
    expect(datos.totalSesiones).toBe(0);
    expect(datos.totalMinutos).toBe(0);
    expect(datos.mediaMinutos).toBe(0);
    expect(datos.porFecha).toEqual([]);
    expect(datos.porFase).toEqual([]);
    expect(datos.porTipo.every((d) => d.valor === 0)).toBe(true);
  });
  it('suma minutos y cuenta todas las sesiones, incluso del mismo día', () => {
    const datos = agregarEstudio([sesion(), sesion({ duracionMinutos: 60 }), sesion({ duracionMinutos: 11 })], vista());
    expect(datos.totalMinutos).toBe(101);
    expect(datos.totalSesiones).toBe(3);
    expect(datos.mediaMinutos).toBeCloseTo(101 / 3);
  });
  it('cuenta los cuatro tipos exactos de sesión sin usar duración como conteo', () => {
    const datos = agregarEstudio(TIPOS_SESION.map((tipo) => sesion({ tipo, duracionMinutos: 90 })), null);
    expect(datos.porTipo.map((d) => [d.clave, d.valor])).toEqual(TIPOS_SESION.map((tipo) => [tipo, 1]));
  });
  it('agrupa DateOnly por fecha y ordena cronológicamente sin convertir UTC', () => {
    const datos = agregarEstudio([sesion(), sesion({ fecha: '2025-12-31', duracionMinutos: 15 }), sesion({ duracionMinutos: 60 })], null);
    expect(datos.porFecha.map((d) => [d.clave, d.valor])).toEqual([['2025-12-31', 15], ['2026-09-08', 90]]);
    expect(datos.porFecha[1].etiqueta).toBe('08/09/2026');
  });
  it('separa meses de años distintos y no fabrica actividad en meses sin registros', () => {
    const datos = agregarEstudio([sesion({ fecha: '2025-09-30' }), sesion(), sesion({ fecha: '2026-09-01' })], null);
    expect(datos.porMes.map((d) => [d.clave, d.valor])).toEqual([['2025-09', 30], ['2026-09', 60]]);
  });
  it('suma minutos por id de Tema y resuelve nombres desde Roadmap', () => {
    const datos = agregarEstudio([sesion(), sesion({ duracionMinutos: 60 })], vista());
    expect(datos.porTema).toEqual([{ clave: 'tema-1', etiqueta: 'Redes', valor: 90 }]);
  });
  it('resuelve Tema a Fase sin contar padres o relaciones como sesiones adicionales', () => {
    const roadmap = vista([fase({ temas: [tema(), tema({ id: 'tema-2', temaPadreId: 'tema-1' })] }), fase({ id: 'fase-2', nombre: 'Linux', temas: [tema({ id: 'tema-3', faseId: 'fase-2' })] })]);
    const datos = agregarEstudio([sesion(), sesion({ temaId: 'tema-2' }), sesion({ temaId: 'tema-3', duracionMinutos: 90 })], roadmap);
    expect(datos.porFase.map((d) => [d.etiqueta, d.valor])).toEqual([['Linux', 90], ['Fundamentos', 60]]);
    expect(datos.porFase.reduce((n, d) => n + d.valor, 0)).toBe(datos.totalMinutos);
  });
  it('conserva tiempo de Tema desconocido con categoría explícita y sin inferir Fase', () => {
    const datos = agregarEstudio([sesion({ temaId: 'desconocido' })], vista());
    expect(datos.porTema[0].etiqueta).toBe('Tema no disponible en Roadmap');
    expect(datos.porFase).toEqual([{ clave: 'sin-fase-resuelta', etiqueta: 'Sin fase resuelta en Roadmap', valor: 30 }]);
  });
  it('con Roadmap parcial no pierde minutos de sesiones fuera del read-side', () => {
    const datos = agregarEstudio([sesion(), sesion({ temaId: 'sin-fase' })], vista());
    expect(datos.porFase.reduce((n, d) => n + d.valor, 0)).toBe(60);
    expect(datos.porFase).toHaveLength(2);
  });
  it('Study funciona sin dataset Roadmap', () => {
    const datos = agregarEstudio([sesion()], null);
    expect(datos.totalMinutos).toBe(30);
    expect(datos.porFase[0].valor).toBe(30);
  });
  it('no recalcula progreso de Fase aunque los criterios no coincidan en un fixture', () => {
    expect(agregarRoadmap(vista()).porFase[0].valor).toBe(37);
  });
  it('cuenta estados exactos y repaso sólo con el boolean del backend', () => {
    const datos = agregarRoadmap(vista([fase({ temas: [tema(), tema({ id: '2', estado: 'Dominado', repasoRecomendado: true }), tema({ id: '3', estado: 'EnRepaso', repasoRecomendado: false }), tema({ id: '4', estado: 'EnPractica' })] })]));
    expect(datos.porEstado.map((d) => [d.clave, d.valor])).toEqual([['NoIniciado', 1], ['EnPractica', 1], ['Dominado', 1], ['EnRepaso', 1]]);
    expect(datos.repasoRecomendado).toBe(1);
    expect(datos.totalTemas).toBe(4);
  });
  it('soporta Roadmap vacío y Fases sin Temas', () => {
    expect(agregarRoadmap(vista([])).porFase).toEqual([]);
    expect(agregarRoadmap(vista([fase({ temas: [] })])).totalTemas).toBe(0);
  });
  it('cuenta los cinco tipos de Evidence sin multiplicar por Temas o Herramientas', () => {
    const datos = agregarEvidence(TIPOS_EVIDENCE.map((tipoEvidence) => evidencia({ tipoEvidence, temas: [{ id: '1', nombre: 'Uno' }, { id: '2', nombre: 'Dos' }] })));
    expect(datos.total).toBe(5);
    expect(datos.porTipo.map((d) => d.valor)).toEqual([1, 1, 1, 1, 1]);
  });
  it('preserva las cuatro madureces, incluidos los dos estados elegibles para Portfolio', () => {
    const datos = agregarEvidence(ESTADOS_MADUREZ.map((estadoMadurez) => evidencia({ estadoMadurez })));
    expect(datos.total).toBe(4);
    expect(datos.porMadurez.map((d) => [d.clave, d.valor])).toEqual(ESTADOS_MADUREZ.map((e) => [e, 1]));
  });
  it('Evidence vacía es factual', () => {
    expect(agregarEvidence([]).total).toBe(0);
    expect(agregarEvidence([]).porMadurez.every((d) => d.valor === 0)).toBe(true);
  });
  it('cuenta todos los tipos reales de Resources', () => {
    const datos = agregarRecursos(TIPOS_RECURSO.map((tipo) => recurso({ tipo })));
    expect(datos.porTipo.map((d) => [d.clave, d.valor])).toEqual(TIPOS_RECURSO.map((tipo) => [tipo, 1]));
  });
  it('preserva los cinco estados Resource sin tratarlos como progreso', () => {
    const datos = agregarRecursos(ESTADOS_RECURSO.map((estado) => recurso({ estado })));
    expect(datos.porEstado.map((d) => [d.clave, d.valor])).toEqual(ESTADOS_RECURSO.map((estado) => [estado, 1]));
    expect(datos.total).toBe(5);
  });
  it('no inventa tipos presentes en Resources vacío', () => {
    expect(agregarRecursos([]).porTipo).toEqual([]);
    expect(agregarRecursos([]).total).toBe(0);
  });
  it('no muta los datasets ni su orden', () => {
    const sesiones = [sesion(), sesion({ fecha: '2025-01-01' })];
    const roadmap = vista([fase({ orden: 2 }), fase({ id: 'otra', orden: 1 })]);
    const previo = JSON.stringify({ sesiones, roadmap });
    agregarEstudio(sesiones, roadmap);
    agregarRoadmap(roadmap);
    expect(JSON.stringify({ sesiones, roadmap })).toBe(previo);
  });
});

describe('Analytics: periodo y formato', () => {
  it('Todo conserva todas las fechas factuales, incluso fechas futuras registradas', () => {
    const sesiones = [sesion({ fecha: '2027-01-01' }), sesion({ fecha: '2020-01-01' })];
    expect(filtrarSesiones(sesiones, 'Todo', '2026-09-08')).toEqual(sesiones);
  });
  it('30 días incluye hoy y día 30, excluye día anterior y futuro', () => {
    const fechas = ['2026-08-09', '2026-08-10', '2026-09-08', '2026-09-09'];
    expect(filtrarSesiones(fechas.map((fecha) => sesion({ fecha })), 30, '2026-09-08').map((s) => s.fecha)).toEqual(['2026-08-10', '2026-09-08']);
  });
  it('90 días cruza años correctamente', () => {
    const fechas = ['2025-10-10', '2025-10-11', '2026-01-08'];
    expect(filtrarSesiones(fechas.map((fecha) => sesion({ fecha })), 90, '2026-01-08').map((s) => s.fecha)).toEqual(['2025-10-11', '2026-01-08']);
  });
  it('30 días respeta febrero bisiesto', () => {
    expect(filtrarSesiones([sesion({ fecha: '2024-02-01' }), sesion({ fecha: '2024-02-02' })], 30, '2024-03-02').map((s) => s.fecha)).toEqual(['2024-02-02']);
  });
  it('extrae fecha del calendario local sin toISOString', () => {
    expect(fechaLocal(new Date(2026, 8, 8, 23, 59))).toBe('2026-09-08');
  });
  it.each([[0, '0 min'], [30, '30 min'], [60, '1 h'], [90, '1 h 30 min']])('reutiliza formato Study para %s minutos', (minutos, texto) => {
    expect(formatearDuracion(minutos as number)).toBe(texto);
  });
});
