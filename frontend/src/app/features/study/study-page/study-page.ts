import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FaseRoadmapVistaV1, RoadmapVistaV1, TemaRoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';
import {
  SesionEstudioResumen,
  TIPOS_SESION,
  TipoSesion,
} from '../study.models';
import { StudyService } from '../study.service';

type EstadoWrite = 'idle' | 'guardando' | 'guardado' | 'error';
type CampoFormulario = 'temaId' | 'fecha' | 'duracionMinutos' | 'tipo';

@Component({
  selector: 'app-study-page',
  imports: [RouterLink],
  templateUrl: './study-page.html',
  styleUrl: './study-page.css',
})
export class StudyPage implements OnInit {
  private static readonly guidRegex =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

  protected readonly tiposSesion = TIPOS_SESION;
  protected readonly sesiones = signal<SesionEstudioResumen[]>([]);
  protected readonly cargandoSesiones = signal(true);
  protected readonly errorSesiones = signal<string | null>(null);
  protected readonly vista = signal<RoadmapVistaV1 | null>(null);
  protected readonly cargandoRoadmap = signal(true);
  protected readonly errorRoadmap = signal<string | null>(null);
  protected readonly temaIdContextual = signal<string | null>(null);
  protected readonly temaIdMalformado = signal(false);
  protected readonly sesionEditando = signal<SesionEstudioResumen | null>(null);
  protected readonly sesionAEliminar = signal<SesionEstudioResumen | null>(null);
  protected readonly estadoWrite = signal<EstadoWrite>('idle');
  protected readonly mensajeWrite = signal<string | null>(null);
  protected readonly errorEliminar = signal<string | null>(null);
  protected readonly erroresFormulario = signal<Partial<Record<CampoFormulario, string>>>({});

  protected readonly formTemaId = signal('');
  protected readonly formFecha = signal(this.fechaHoy());
  protected readonly formDuracionMinutos = signal(45);
  protected readonly formTipo = signal<TipoSesion>('Teoria');
  protected readonly formNotas = signal('');

  protected readonly temasDisponibles = computed(() => this.obtenerTemasOrdenados());
  protected readonly temasPorId = computed(() =>
    this.temasDisponibles().reduce<Record<string, TemaRoadmapVistaV1>>((mapa, tema) => {
      mapa[tema.id] = tema;
      return mapa;
    }, {}),
  );
  protected readonly temaContextual = computed(() => {
    const temaId = this.temaIdContextual();

    return temaId ? this.temasPorId()[temaId] ?? null : null;
  });
  protected readonly contextoInvalido = computed(
    () =>
      this.temaIdMalformado() ||
      (!!this.temaIdContextual() &&
        !this.cargandoRoadmap() &&
        !this.errorRoadmap() &&
        !this.temaContextual()),
  );
  protected readonly sesionesVisibles = computed(() => {
    const temaId = this.temaContextual()?.id;

    if (!temaId) {
      return this.sesiones();
    }

    return this.sesiones().filter((sesion) => sesion.temaId === temaId);
  });
  protected readonly totalMinutos = computed(() =>
    this.sesionesVisibles().reduce((total, sesion) => total + sesion.duracionMinutos, 0),
  );
  protected readonly ultimaSesion = computed(() => this.sesionesVisibles()[0] ?? null);
  protected readonly modoEdicion = computed(() => this.sesionEditando() !== null);
  protected readonly cargando = computed(() => this.cargandoSesiones() || this.cargandoRoadmap());

  public constructor(
    private readonly route: ActivatedRoute,
    private readonly study: StudyService,
    private readonly roadmap: RoadmapService,
  ) {}

  public ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      this.aplicarTemaContextual(params.get('temaId'));
    });

    this.cargarRoadmap();
    this.cargarSesiones();
  }

  protected recargar(): void {
    this.cargarRoadmap(true);
    this.cargarSesiones();
  }

  protected onTemaChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.formTemaId.set(target.value);
      this.limpiarErrorCampo('temaId');
    }
  }

  protected onFechaInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.formFecha.set(target.value);
      this.limpiarErrorCampo('fecha');
    }
  }

  protected onDuracionInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.formDuracionMinutos.set(Number(target.value));
      this.limpiarErrorCampo('duracionMinutos');
    }
  }

  protected onTipoChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esTipoSesion(target.value)) {
      this.formTipo.set(target.value);
      this.limpiarErrorCampo('tipo');
    }
  }

  protected onNotasInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLTextAreaElement) {
      this.formNotas.set(target.value);
    }
  }

  protected guardarSesion(): void {
    this.errorEliminar.set(null);
    this.mensajeWrite.set(null);

    if (!this.formularioValido() || this.estadoWrite() === 'guardando') {
      return;
    }

    this.estadoWrite.set('guardando');

    const sesion = this.sesionEditando();

    if (sesion) {
      this.study.actualizarSesion(sesion.id, this.crearPayloadFormulario()).subscribe({
        next: () => this.finalizarWriteExitoso('Cambios guardados.'),
        error: (error: Error) => {
          this.estadoWrite.set('error');
          this.mensajeWrite.set(error.message);
        },
      });

      return;
    }

    this.study.registrarSesion(this.crearPayloadFormulario()).subscribe({
      next: () => {
        this.finalizarWriteExitoso('Sesion registrada.');
      },
      error: (error: Error) => {
        this.estadoWrite.set('error');
        this.mensajeWrite.set(error.message);
      },
    });
  }

  protected editarSesion(sesion: SesionEstudioResumen): void {
    this.sesionEditando.set(sesion);
    this.sesionAEliminar.set(null);
    this.errorEliminar.set(null);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.erroresFormulario.set({});
    this.formTemaId.set(sesion.temaId);
    this.formFecha.set(sesion.fecha);
    this.formDuracionMinutos.set(sesion.duracionMinutos);
    this.formTipo.set(sesion.tipo);
    this.formNotas.set(sesion.notas ?? '');
  }

  protected cancelarEdicion(): void {
    this.sesionEditando.set(null);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.erroresFormulario.set({});
    this.restablecerFormulario();
  }

  protected pedirEliminar(sesion: SesionEstudioResumen): void {
    this.sesionAEliminar.set(sesion);
    this.errorEliminar.set(null);
  }

  protected cancelarEliminar(): void {
    this.sesionAEliminar.set(null);
    this.errorEliminar.set(null);
  }

  protected confirmarEliminar(sesion: SesionEstudioResumen): void {
    this.errorEliminar.set(null);

    this.study.eliminarSesion(sesion.id).subscribe({
      next: () => {
        if (this.sesionEditando()?.id === sesion.id) {
          this.cancelarEdicion();
        }

        this.sesionAEliminar.set(null);
        this.refrescarDespuesDeWrite();
      },
      error: (error: Error) => {
        this.errorEliminar.set(error.message);
      },
    });
  }

  protected nombreTema(temaId: string): string {
    return this.temasPorId()[temaId]?.nombre ?? 'Tema no disponible';
  }

  protected faseDeTema(temaId: string): FaseRoadmapVistaV1 | null {
    return this.vista()?.fases.find((fase) => fase.temas.some((tema) => tema.id === temaId)) ?? null;
  }

  protected etiquetaTipo(tipo: TipoSesion): string {
    const labels: Record<TipoSesion, string> = {
      Teoria: 'Teoria',
      Practica: 'Practica',
      Laboratorio: 'Laboratorio',
      Repaso: 'Repaso',
    };

    return labels[tipo];
  }

  protected formatearFecha(fecha: string): string {
    const partes = /^(\d{4})-(\d{2})-(\d{2})$/.exec(fecha);

    if (!partes) {
      return fecha;
    }

    const [, year, month, day] = partes;

    return new Intl.DateTimeFormat('es-PE', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(new Date(Number(year), Number(month) - 1, Number(day)));
  }

  protected formatearDuracion(minutos: number): string {
    if (minutos < 60) {
      return `${minutos} min`;
    }

    const horas = Math.floor(minutos / 60);
    const resto = minutos % 60;

    return resto === 0 ? `${horas} h` : `${horas} h ${resto} min`;
  }

  protected trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  private cargarRoadmap(forzarRefresco = false): void {
    this.cargandoRoadmap.set(true);
    this.errorRoadmap.set(null);

    const solicitud = forzarRefresco ? this.roadmap.refrescarVista() : this.roadmap.obtenerVista();

    solicitud.subscribe({
      next: (vista) => {
        this.vista.set({
          ...vista,
          fases: [...vista.fases].sort((a, b) => a.orden - b.orden),
        });
        this.cargandoRoadmap.set(false);
        this.limpiarTemaContextualInexistente();
      },
      error: (error: Error) => {
        this.errorRoadmap.set(error.message);
        this.cargandoRoadmap.set(false);
      },
    });
  }

  private cargarSesiones(mostrarLoading = true): void {
    if (mostrarLoading) {
      this.cargandoSesiones.set(true);
    }

    this.errorSesiones.set(null);

    this.study.listarSesiones().subscribe({
      next: (sesiones) => {
        this.sesiones.set(sesiones);
        this.cargandoSesiones.set(false);
      },
      error: (error: Error) => {
        this.errorSesiones.set(error.message);
        this.cargandoSesiones.set(false);
      },
    });
  }

  private refrescarDespuesDeWrite(): void {
    this.cargarSesiones(false);
    this.roadmap.refrescarVista().subscribe({
      next: (vista) => {
        this.vista.set({
          ...vista,
          fases: [...vista.fases].sort((a, b) => a.orden - b.orden),
        });
      },
      error: () => {
        this.vista.set(null);
      },
    });
  }

  private finalizarWriteExitoso(mensaje: string): void {
    this.estadoWrite.set('guardado');
    this.mensajeWrite.set(mensaje);
    this.sesionEditando.set(null);
    this.restablecerFormulario();
    this.refrescarDespuesDeWrite();
  }

  private aplicarTemaContextual(temaId: string | null): void {
    this.temaIdMalformado.set(!!temaId && !StudyPage.guidRegex.test(temaId));
    this.temaIdContextual.set(temaId && StudyPage.guidRegex.test(temaId) ? temaId : null);

    if (!this.modoEdicion()) {
      this.formTemaId.set(this.temaIdContextual() ?? '');
    }
  }

  private limpiarTemaContextualInexistente(): void {
    const temaId = this.temaIdContextual();

    if (!temaId) {
      return;
    }

    if (this.temasPorId()[temaId]) {
      if (!this.modoEdicion()) {
        this.formTemaId.set(temaId);
      }

      return;
    }

    if (!this.modoEdicion()) {
      this.formTemaId.set('');
    }
  }

  private formularioValido(): boolean {
    const errores: Partial<Record<CampoFormulario, string>> = {};

    if (!this.formTemaId() || !this.temasPorId()[this.formTemaId()]) {
      errores.temaId = 'Selecciona un tema disponible.';
    }

    if (!/^\d{4}-\d{2}-\d{2}$/.test(this.formFecha())) {
      errores.fecha = 'Ingresa una fecha valida.';
    }

    if (!Number.isInteger(this.formDuracionMinutos()) || this.formDuracionMinutos() <= 0) {
      errores.duracionMinutos = 'La duracion debe ser mayor a cero minutos.';
    }

    if (!this.esTipoSesion(this.formTipo())) {
      errores.tipo = 'Selecciona un tipo de sesion valido.';
    }

    this.erroresFormulario.set(errores);

    return Object.keys(errores).length === 0;
  }

  private crearPayloadFormulario() {
    const notas = this.formNotas().trim();

    return {
      temaId: this.formTemaId(),
      fecha: this.formFecha(),
      duracionMinutos: this.formDuracionMinutos(),
      tipo: this.formTipo(),
      notas: notas.length > 0 ? notas : null,
    };
  }

  private restablecerFormulario(): void {
    this.formTemaId.set(this.temaContextual()?.id ?? '');
    this.formFecha.set(this.fechaHoy());
    this.formDuracionMinutos.set(45);
    this.formTipo.set('Teoria');
    this.formNotas.set('');
    this.erroresFormulario.set({});
  }

  private limpiarErrorCampo(campo: CampoFormulario): void {
    const errores = { ...this.erroresFormulario() };
    delete errores[campo];
    this.erroresFormulario.set(errores);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
  }

  private obtenerTemasOrdenados(): TemaRoadmapVistaV1[] {
    return (this.vista()?.fases ?? [])
      .flatMap((fase) => fase.temas)
      .sort((a, b) => a.nombre.localeCompare(b.nombre));
  }

  private esTipoSesion(value: string): value is TipoSesion {
    return TIPOS_SESION.includes(value as TipoSesion);
  }

  private fechaHoy(): string {
    const hoy = new Date();
    const year = hoy.getFullYear();
    const month = String(hoy.getMonth() + 1).padStart(2, '0');
    const day = String(hoy.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
