import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RoadmapVistaV1, TemaRoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';
import {
  ESTADOS_RECURSO,
  EstadoRecurso,
  RecursoDetalle,
  RecursoResumen,
  RatingRecurso,
  TIPOS_RECURSO,
  TipoRecurso,
} from '../resources.models';
import { ResourcesService } from '../resources.service';

type EstadoWrite = 'idle' | 'guardando' | 'guardado' | 'error';
type ModoFormulario = 'crear' | 'editar' | null;
type CampoFormulario = 'titulo' | 'tipo' | 'estado' | 'rating' | 'url';

@Component({
  selector: 'app-resources-page',
  imports: [RouterLink],
  templateUrl: './resources-page.html',
  styleUrl: './resources-page.css',
})
export class ResourcesPage implements OnInit {
  private static readonly guidRegex =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

  protected readonly tiposRecurso = TIPOS_RECURSO;
  protected readonly estadosRecurso = ESTADOS_RECURSO;
  protected readonly ratingOpciones: readonly (RatingRecurso | '')[] = ['', 1, 2, 3, 4, 5];

  protected readonly recursos = signal<RecursoResumen[]>([]);
  protected readonly cargandoLista = signal(true);
  protected readonly errorLista = signal<string | null>(null);
  protected readonly detalle = signal<RecursoDetalle | null>(null);
  protected readonly recursoSeleccionadoId = signal<string | null>(null);
  protected readonly cargandoDetalle = signal(false);
  protected readonly errorDetalle = signal<string | null>(null);
  protected readonly temaIdContextual = signal<string | null>(null);
  protected readonly temaIdMalformado = signal(false);
  protected readonly vista = signal<RoadmapVistaV1 | null>(null);
  protected readonly errorRoadmap = signal<string | null>(null);
  protected readonly busqueda = signal('');
  protected readonly estadoFiltro = signal<EstadoRecurso | ''>('');
  protected readonly tipoFiltro = signal<TipoRecurso | ''>('');
  protected readonly modoFormulario = signal<ModoFormulario>(null);
  protected readonly estadoWrite = signal<EstadoWrite>('idle');
  protected readonly mensajeWrite = signal<string | null>(null);
  protected readonly errorEliminar = signal<string | null>(null);
  protected readonly confirmandoEliminar = signal(false);
  protected readonly erroresFormulario = signal<Partial<Record<CampoFormulario, string>>>({});

  protected readonly formTipo = signal<TipoRecurso>('Documentacion');
  protected readonly formTitulo = signal('');
  protected readonly formUrl = signal('');
  protected readonly formEstado = signal<EstadoRecurso>('PorClasificar');
  protected readonly formRating = signal<RatingRecurso | null>(null);
  protected readonly formNotas = signal('');
  protected readonly formHerramientaIA = signal('');
  protected readonly formPromptsUtilizados = signal('');

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
  protected readonly recursosFiltrados = computed(() => {
    const texto = this.normalizarTexto(this.busqueda());
    const estado = this.estadoFiltro();
    const tipo = this.tipoFiltro();

    return this.recursos().filter((recurso) => {
      const coincideTexto =
        texto.length === 0 ||
        this.normalizarTexto(recurso.titulo).includes(texto) ||
        this.normalizarTexto(recurso.url ?? '').includes(texto);
      const coincideEstado = !estado || recurso.estado === estado;
      const coincideTipo = !tipo || recurso.tipo === tipo;

      return coincideTexto && coincideEstado && coincideTipo;
    });
  });
  protected readonly hayFiltrosLocales = computed(
    () => this.busqueda().trim().length > 0 || !!this.estadoFiltro() || !!this.tipoFiltro(),
  );
  protected readonly tituloContexto = computed(() => {
    const tema = this.temaContextual();

    if (tema) {
      return `Recursos de ${tema.nombre}`;
    }

    return this.temaIdContextual() ? 'Recursos del tema' : 'Resources';
  });

  public constructor(
    private readonly route: ActivatedRoute,
    private readonly resources: ResourcesService,
    private readonly roadmap: RoadmapService,
  ) {}

  public ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      this.aplicarTemaContextual(params.get('temaId'));
      this.cargarLista();
    });

    this.cargarRoadmap();
  }

  protected recargarLista(): void {
    this.cargarLista();
    this.cargarRoadmap(true);
  }

  protected abrirCrear(): void {
    this.modoFormulario.set('crear');
    this.detalle.set(null);
    this.recursoSeleccionadoId.set(null);
    this.errorDetalle.set(null);
    this.errorEliminar.set(null);
    this.confirmandoEliminar.set(false);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.erroresFormulario.set({});
    this.restablecerFormularioCrear();
  }

  protected abrirDetalle(recursoId: string): void {
    this.recursoSeleccionadoId.set(recursoId);
    this.detalle.set(null);
    this.modoFormulario.set(null);
    this.cargandoDetalle.set(true);
    this.errorDetalle.set(null);
    this.errorEliminar.set(null);
    this.confirmandoEliminar.set(false);

    this.resources.obtenerDetalle(recursoId).subscribe({
      next: (detalle) => {
        this.detalle.set(detalle);
        this.cargandoDetalle.set(false);
      },
      error: (error: Error) => {
        this.errorDetalle.set(error.message);
        this.cargandoDetalle.set(false);
      },
    });
  }

  protected editarDetalle(): void {
    const detalle = this.detalle();

    if (!detalle) {
      return;
    }

    this.modoFormulario.set('editar');
    this.confirmandoEliminar.set(false);
    this.errorEliminar.set(null);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.erroresFormulario.set({});
    this.formTipo.set(detalle.tipo);
    this.formTitulo.set(detalle.titulo);
    this.formUrl.set(detalle.url ?? '');
    this.formEstado.set(detalle.estado);
    this.formRating.set(detalle.rating);
    this.formNotas.set(detalle.notas ?? '');
    this.formHerramientaIA.set(detalle.herramientaIA ?? '');
    this.formPromptsUtilizados.set(detalle.promptsUtilizados ?? '');
  }

  protected cancelarFormulario(): void {
    this.modoFormulario.set(null);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.erroresFormulario.set({});

    if (this.recursoSeleccionadoId() && !this.detalle()) {
      this.abrirDetalle(this.recursoSeleccionadoId()!);
    }
  }

  protected guardarFormulario(): void {
    this.mensajeWrite.set(null);
    this.errorEliminar.set(null);

    const modo = this.modoFormulario();

    if (!modo || !this.formularioValido() || this.estadoWrite() === 'guardando') {
      return;
    }

    this.estadoWrite.set('guardando');

    if (modo === 'crear') {
      this.crearRecurso();
      return;
    }

    this.actualizarRecurso();
  }

  protected pedirEliminar(): void {
    this.confirmandoEliminar.set(true);
    this.errorEliminar.set(null);
  }

  protected cancelarEliminar(): void {
    this.confirmandoEliminar.set(false);
    this.errorEliminar.set(null);
  }

  protected confirmarEliminar(): void {
    const detalle = this.detalle();

    if (!detalle) {
      return;
    }

    this.errorEliminar.set(null);

    this.resources.eliminar(detalle.id).subscribe({
      next: () => {
        this.detalle.set(null);
        this.recursoSeleccionadoId.set(null);
        this.confirmandoEliminar.set(false);
        this.modoFormulario.set(null);
        this.cargarLista(false);
      },
      error: (error: Error) => {
        this.errorEliminar.set(error.message);
      },
    });
  }

  protected limpiarFiltros(): void {
    this.busqueda.set('');
    this.estadoFiltro.set('');
    this.tipoFiltro.set('');
  }

  protected onBusquedaInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.busqueda.set(target.value);
    }
  }

  protected onEstadoFiltroChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.estadoFiltro.set(this.esEstadoRecurso(target.value) ? target.value : '');
    }
  }

  protected onTipoFiltroChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.tipoFiltro.set(this.esTipoRecurso(target.value) ? target.value : '');
    }
  }

  protected onTipoChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esTipoRecurso(target.value)) {
      this.formTipo.set(target.value);
      this.limpiarErrorCampo('tipo');
    }
  }

  protected onTituloInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.formTitulo.set(target.value);
      this.limpiarErrorCampo('titulo');
    }
  }

  protected onUrlInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.formUrl.set(target.value);
      this.limpiarErrorCampo('url');
    }
  }

  protected onEstadoChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esEstadoRecurso(target.value)) {
      this.formEstado.set(target.value);
      this.limpiarErrorCampo('estado');
    }
  }

  protected onRatingChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.formRating.set(this.parsearRating(target.value));
      this.limpiarErrorCampo('rating');
    }
  }

  protected onNotasInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLTextAreaElement) {
      this.formNotas.set(target.value);
    }
  }

  protected onHerramientaIAInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.formHerramientaIA.set(target.value);
    }
  }

  protected onPromptsInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLTextAreaElement) {
      this.formPromptsUtilizados.set(target.value);
    }
  }

  protected etiquetaEstado(estado: EstadoRecurso): string {
    const labels: Record<EstadoRecurso, string> = {
      PorClasificar: 'Por clasificar',
      PorRevisar: 'Por revisar',
      EnUso: 'En uso',
      Consultado: 'Consultado',
      Referencia: 'Referencia',
    };

    return labels[estado];
  }

  protected etiquetaTipo(tipo: TipoRecurso): string {
    const labels: Record<TipoRecurso, string> = {
      Documentacion: 'Documentacion',
      Libro: 'Libro',
      Curso: 'Curso',
      Video: 'Video',
      Laboratorio: 'Laboratorio',
      Writeup: 'Writeup',
      Cheatsheet: 'Cheatsheet',
      Script: 'Script',
      RepositorioGitHub: 'Repositorio GitHub',
      NotebookIA: 'Notebook IA',
      Otro: 'Otro',
    };

    return labels[tipo];
  }

  protected textoRating(rating: RatingRecurso | null): string {
    return rating === null ? 'Sin valoracion' : `${rating}/5`;
  }

  protected dominioUrl(url: string | null): string {
    if (!url) {
      return 'Sin URL';
    }

    try {
      return new URL(url).hostname.replace(/^www\./, '');
    } catch {
      return url;
    }
  }

  protected textoTemas(recurso: RecursoResumen | RecursoDetalle): string {
    return recurso.temas.length === 0
      ? 'Sin tema vinculado'
      : recurso.temas.map((tema) => tema.nombre).join(', ');
  }

  protected trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  private cargarLista(mostrarLoading = true): void {
    const temaId = this.temaIdContextual();

    if (mostrarLoading) {
      this.cargandoLista.set(true);
    }

    this.errorLista.set(null);

    this.resources.listar(temaId).subscribe({
      next: (recursos) => {
        this.recursos.set(recursos);
        this.cargandoLista.set(false);

        const seleccionadoId = this.recursoSeleccionadoId();

        if (seleccionadoId && !recursos.some((recurso) => recurso.id === seleccionadoId)) {
          this.detalle.set(null);
          this.recursoSeleccionadoId.set(null);
        }
      },
      error: (error: Error) => {
        this.errorLista.set(error.message);
        this.cargandoLista.set(false);
      },
    });
  }

  private cargarRoadmap(forzarRefresco = false): void {
    this.errorRoadmap.set(null);

    const solicitud = forzarRefresco ? this.roadmap.refrescarVista() : this.roadmap.obtenerVista();

    solicitud
      .pipe(
        catchError((error: Error) => {
          this.errorRoadmap.set(error.message);
          return of(null);
        }),
      )
      .subscribe((vista) => {
        if (!vista) {
          return;
        }

        this.vista.set({
          ...vista,
          fases: [...vista.fases].sort((a, b) => a.orden - b.orden),
        });
      });
  }

  private crearRecurso(): void {
    this.resources.crear(this.payloadCrear()).subscribe({
      next: (recurso) => {
        const temaId = this.temaIdContextual();

        if (temaId && recurso.id) {
          this.vincularRecursoCreado(recurso.id, temaId);
          return;
        }

        this.finalizarWriteExitoso('Recurso creado.', recurso.id);
      },
      error: (error: Error) => {
        this.estadoWrite.set('error');
        this.mensajeWrite.set(error.message);
      },
    });
  }

  private vincularRecursoCreado(recursoId: string, temaId: string): void {
    this.resources.vincularTema(recursoId, temaId).subscribe({
      next: () => this.finalizarWriteExitoso('Recurso creado y vinculado al tema.', recursoId),
      error: (error: Error) => {
        this.estadoWrite.set('error');
        this.mensajeWrite.set(`El recurso fue creado, pero no se pudo vincular al tema. ${error.message}`);
        this.cargarLista(false);
        this.abrirDetalle(recursoId);
      },
    });
  }

  private actualizarRecurso(): void {
    const detalle = this.detalle();

    if (!detalle) {
      this.estadoWrite.set('error');
      this.mensajeWrite.set('Selecciona un recurso para editar.');
      return;
    }

    this.resources.actualizar(detalle.id, this.payloadActualizar()).subscribe({
      next: () => this.finalizarWriteExitoso('Cambios guardados.', detalle.id),
      error: (error: Error) => {
        this.estadoWrite.set('error');
        this.mensajeWrite.set(error.message);
      },
    });
  }

  private finalizarWriteExitoso(mensaje: string, recursoId: string | null): void {
    this.estadoWrite.set('guardado');
    this.mensajeWrite.set(mensaje);
    this.modoFormulario.set(null);
    this.cargarLista(false);

    if (recursoId) {
      this.abrirDetalle(recursoId);
    }
  }

  private aplicarTemaContextual(temaId: string | null): void {
    this.temaIdMalformado.set(!!temaId && !ResourcesPage.guidRegex.test(temaId));
    this.temaIdContextual.set(temaId && ResourcesPage.guidRegex.test(temaId) ? temaId : null);
  }

  private formularioValido(): boolean {
    const errores: Partial<Record<CampoFormulario, string>> = {};

    if (!this.esTipoRecurso(this.formTipo())) {
      errores.tipo = 'Selecciona un tipo valido.';
    }

    if (!this.formTitulo().trim()) {
      errores.titulo = 'Ingresa un titulo.';
    }

    if (this.formUrl().trim() && !this.esUrlValida(this.formUrl().trim())) {
      errores.url = 'Ingresa una URL valida.';
    }

    if (this.modoFormulario() === 'editar' && !this.esEstadoRecurso(this.formEstado())) {
      errores.estado = 'Selecciona un estado valido.';
    }

    const rating = this.formRating();
    if (rating !== null && ![1, 2, 3, 4, 5].includes(rating)) {
      errores.rating = 'Selecciona una valoracion valida.';
    }

    this.erroresFormulario.set(errores);

    return Object.keys(errores).length === 0;
  }

  private payloadCrear() {
    return {
      tipo: this.formTipo(),
      titulo: this.formTitulo().trim(),
      url: this.valorOpcional(this.formUrl()),
    };
  }

  private payloadActualizar() {
    return {
      titulo: this.formTitulo().trim(),
      url: this.valorOpcional(this.formUrl()),
      estado: this.formEstado(),
      rating: this.formRating(),
      notas: this.valorOpcional(this.formNotas()),
      herramientaIA: this.valorOpcional(this.formHerramientaIA()),
      promptsUtilizados: this.valorOpcional(this.formPromptsUtilizados()),
    };
  }

  private restablecerFormularioCrear(): void {
    this.formTipo.set('Documentacion');
    this.formTitulo.set('');
    this.formUrl.set('');
    this.formEstado.set('PorClasificar');
    this.formRating.set(null);
    this.formNotas.set('');
    this.formHerramientaIA.set('');
    this.formPromptsUtilizados.set('');
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

  private esEstadoRecurso(value: string): value is EstadoRecurso {
    return ESTADOS_RECURSO.includes(value as EstadoRecurso);
  }

  private esTipoRecurso(value: string): value is TipoRecurso {
    return TIPOS_RECURSO.includes(value as TipoRecurso);
  }

  private parsearRating(value: string): RatingRecurso | null {
    const numero = Number(value);

    return [1, 2, 3, 4, 5].includes(numero) ? (numero as RatingRecurso) : null;
  }

  private valorOpcional(value: string): string | null {
    const normalizado = value.trim();

    return normalizado.length > 0 ? normalizado : null;
  }

  private normalizarTexto(value: string): string {
    return value.trim().toLocaleLowerCase('es-PE');
  }

  private esUrlValida(value: string): boolean {
    try {
      const url = new URL(value);

      return url.protocol === 'http:' || url.protocol === 'https:';
    } catch {
      return false;
    }
  }
}
