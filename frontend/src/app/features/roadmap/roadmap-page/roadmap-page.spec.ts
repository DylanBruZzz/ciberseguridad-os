import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NEVER, of, throwError } from 'rxjs';
import { FaseResumen, TemaResumen } from '../roadmap.models';
import { RoadmapService } from '../roadmap.service';
import { RoadmapPage } from './roadmap-page';

const fase: FaseResumen = {
  id: 'fase-1',
  nombre: 'Fundamentos de Informatica y Redes',
  orden: 1,
  color: null,
  descripcion: 'Base de redes',
  objetivos: ['Comprender redes'],
  criteriosAvance: ['Explicar OSI'],
  mesInicioRecomendado: 1,
  mesFinRecomendado: 2,
  cargaSemanalRecomendada: '10 hrs/semana',
};

const tema: TemaResumen = {
  id: 'tema-1',
  faseId: 'fase-1',
  temaPadreId: null,
  nombre: 'Modelo OSI / TCP-IP',
  tipoConocimiento: 'Teorico',
  descripcion: null,
};

describe('RoadmapPage', () => {
  let fixture: ComponentFixture<RoadmapPage>;

  function configure(roadmap: Partial<RoadmapService>): Promise<void> {
    return TestBed.configureTestingModule({
      imports: [RoadmapPage],
      providers: [{ provide: RoadmapService, useValue: roadmap }],
    }).compileComponents();
  }

  it('muestra estado loading', async () => {
    await configure({ listarFases: () => NEVER });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Cargando roadmap');
  });

  it('muestra fases y temas cargados', async () => {
    await configure({
      listarFases: () => of([fase]),
      listarTemas: () => of([tema]),
    });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Fundamentos de Informatica y Redes');
    expect(text).toContain('Modelo OSI / TCP-IP');
    expect(text).toContain('Comprender redes');
  });

  it('muestra estado vacio', async () => {
    await configure({
      listarFases: () => of([]),
      listarTemas: () => of([]),
    });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Roadmap vacio');
  });

  it('muestra error de carga', async () => {
    await configure({
      listarFases: () => throwError(() => new Error('API local no disponible')),
    });

    fixture = TestBed.createComponent(RoadmapPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No se pudo cargar el roadmap');
    expect(fixture.nativeElement.textContent).toContain('API local no disponible');
  });
});
