import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RoadmapVistaV1, TemaRoadmapVistaV1 } from '../../roadmap/roadmap.models';
import { RoadmapService } from '../../roadmap/roadmap.service';
import {
  CertificacionResumen,
  ESTADOS_MADUREZ,
  ESTADOS_PROYECTO,
  EstadoMadurez,
  EstadoProyecto,
  EvidenceDetail,
  EvidenceItemV1,
  NotaResumen,
  TIPOS_ARTEFACTO,
  TIPOS_EVIDENCE,
  TIPOS_NOTA,
  TipoArtefacto,
  TipoEvidenceV1,
  TipoNota,
} from '../evidence.models';
import { EvidenceService } from '../evidence.service';

type EstadoWrite = 'idle' | 'guardando' | 'guardado' | 'error';
type ModoFormulario = 'crear' | 'editar' | null;
type CampoFormulario =
  | 'tipoEvidence'
  | 'nombre'
  | 'titulo'
  | 'url'
  | 'estadoProyecto'
  | 'estadoMadurez'
  | 'tipoArtefacto'
  | 'certificacionId'
  | 'fecha'
  | 'tiempoInvertidoMinutos'
  | 'fechaInicio'
  | 'fechaFin'
  | 'notaTexto';

@Component({
  selector: 'app-evidence-page',
  imports: [RouterLink],
  templateUrl: './evidence-page.html',
  styleUrl: './evidence-page.css',
})
export class EvidencePage implements OnInit {
  private static readonly guidRegex =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

  protected readonly tiposEvidence = TIPOS_EVIDENCE;
  protected readonly estadosMadurez = ESTADOS_MADUREZ;
  protected readonly estadosProyecto = ESTADOS_PROYECTO;
  protected readonly tiposArtefacto = TIPOS_ARTEFACTO;
  protected readonly tiposNota = TIPOS_NOTA;

  protected readonly lista = signal<EvidenceItemV1[]>([]);
  protected readonly totalServidor = signal(0);
  protected readonly cargandoLista = signal(true);
  protected readonly errorLista = signal<string | null>(null);
  protected readonly detalle = signal<EvidenceDetail | null>(null);
  protected readonly seleccionado = signal<EvidenceItemV1 | null>(null);
  protected readonly cargandoDetalle = signal(false);
  protected readonly errorDetalle = signal<string | null>(null);
  protected readonly temaIdContextual = signal<string | null>(null);
  protected readonly temaIdMalformado = signal(false);
  protected readonly vista = signal<RoadmapVistaV1 | null>(null);
  protected readonly errorRoadmap = signal<string | null>(null);
  protected readonly certificaciones = signal<CertificacionResumen[]>([]);
  protected readonly errorCertificaciones = signal<string | null>(null);
  protected readonly notas = signal<NotaResumen[]>([]);
  protected readonly cargandoNotas = signal(false);
  protected readonly errorNotas = signal<string | null>(null);
  protected readonly busqueda = signal('');
  protected readonly tipoFiltro = signal<TipoEvidenceV1 | ''>('');
  protected readonly madurezFiltro = signal<EstadoMadurez | ''>('');
  protected readonly modoFormulario = signal<ModoFormulario>(null);
  protected readonly estadoWrite = signal<EstadoWrite>('idle');
  protected readonly mensajeWrite = signal<string | null>(null);
  protected readonly errorEliminar = signal<string | null>(null);
  protected readonly confirmandoEliminar = signal(false);
  protected readonly erroresFormulario = signal<Partial<Record<CampoFormulario, string>>>({});

  protected readonly formTipoEvidence = signal<TipoEvidenceV1>('Proyecto');
  protected readonly formNombre = signal('');
  protected readonly formTitulo = signal('');
  protected readonly formDescripcion = signal('');
  protected readonly formObjetivo = signal('');
  protected readonly formEntornoVms = signal('');
  protected readonly formHallazgos = signal('');
  protected readonly formTiempoInvertidoMinutos = signal('');
  protected readonly formEstadoProyecto = signal<EstadoProyecto>('Idea');
  protected readonly formEstadoMadurez = signal<EstadoMadurez>('Borrador');
  protected readonly formRepositorioUrl = signal('');
  protected readonly formFechaInicio = signal('');
  protected readonly formFechaFin = signal('');
  protected readonly formPlataformaOrigen = signal('');
  protected readonly formUrl = signal('');
  protected readonly formFecha = signal('');
  protected readonly formTipoArtefacto = signal<TipoArtefacto>('Script');
  protected readonly formContenidoOUrl = signal('');
  protected readonly formLenguajeTecnologia = signal('');
  protected readonly formCertificacionId = signal('');
  protected readonly formFechaObtencion = signal('');
  protected readonly formEvidenciaUrl = signal('');
  protected readonly notaTexto = signal('');
  protected readonly notaTipo = signal<TipoNota>('Nota');
  protected readonly estadoNota = signal<EstadoWrite>('idle');
  protected readonly mensajeNota = signal<string | null>(null);

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
  protected readonly tituloContexto = computed(() => {
    const tema = this.temaContextual();

    if (tema) {
      return `Evidence de ${tema.nombre}`;
    }

    return this.temaIdContextual() ? 'Evidence del tema' : 'Evidence';
  });
  protected readonly evidenciasFiltradas = computed(() => {
    const texto = this.normalizarTexto(this.busqueda());

    return this.lista().filter((item) => {
      if (texto.length === 0) {
        return true;
      }

      return (
        this.normalizarTexto(item.titulo).includes(texto) ||
        this.normalizarTexto(item.tipoEvidence).includes(texto) ||
        this.normalizarTexto(item.estadoMadurez).includes(texto) ||
        item.temas.some((tema) => this.normalizarTexto(tema.nombre).includes(texto)) ||
        item.herramientas.some((herramienta) => this.normalizarTexto(herramienta.nombre).includes(texto))
      );
    });
  });
  protected readonly hayFiltros = computed(
    () => !!this.tipoFiltro() || !!this.madurezFiltro() || this.busqueda().trim().length > 0,
  );
  protected readonly notasDetalle = computed(() => {
    const actual = this.detalle();

    if (!actual || !this.soportaNotas(actual.tipoEvidence)) {
      return [];
    }

    return this.notas()
      .filter((nota) => this.notaPerteneceADetalle(nota, actual.tipoEvidence, actual.item.id))
      .sort((a, b) => Date.parse(b.fecha) - Date.parse(a.fecha));
  });

  public constructor(
    private readonly route: ActivatedRoute,
    private readonly evidence: EvidenceService,
    private readonly roadmap: RoadmapService,
  ) {}

  public ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      this.aplicarTemaContextual(params.get('temaId'));
      this.cargarLista();
    });

    this.cargarRoadmap();
    this.cargarCertificaciones();
  }

  protected recargarLista(): void {
    this.cargarLista();
    this.cargarRoadmap(true);
  }

  protected abrirCrear(tipo: TipoEvidenceV1 | null = null): void {
    this.modoFormulario.set('crear');
    this.detalle.set(null);
    this.seleccionado.set(null);
    this.errorDetalle.set(null);
    this.errorEliminar.set(null);
    this.confirmandoEliminar.set(false);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.erroresFormulario.set({});
    this.restablecerFormularioCrear(tipo ?? this.formTipoEvidence());
  }

  protected abrirDetalle(item: EvidenceItemV1): void {
    this.seleccionado.set(item);
    this.detalle.set(null);
    this.modoFormulario.set(null);
    this.cargandoDetalle.set(true);
    this.errorDetalle.set(null);
    this.errorEliminar.set(null);
    this.confirmandoEliminar.set(false);
    this.limpiarNotaState();

    this.evidence.obtenerDetalle(item).subscribe({
      next: (detalle) => {
        this.detalle.set(detalle);
        this.cargandoDetalle.set(false);
        this.cargarNotasSiAplica(detalle.tipoEvidence);
      },
      error: (error: Error) => {
        this.errorDetalle.set(error.message);
        this.cargandoDetalle.set(false);
      },
    });
  }

  protected editarDetalle(): void {
    const actual = this.detalle();

    if (!actual) {
      return;
    }

    this.modoFormulario.set('editar');
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.errorEliminar.set(null);
    this.confirmandoEliminar.set(false);
    this.erroresFormulario.set({});
    this.cargarFormularioEdicion(actual);
  }

  protected cancelarFormulario(): void {
    this.modoFormulario.set(null);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
    this.erroresFormulario.set({});
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
      this.crearEvidence();
      return;
    }

    this.actualizarEvidence();
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
    const actual = this.detalle();

    if (!actual) {
      return;
    }

    this.errorEliminar.set(null);

    this.evidence.eliminar(actual.tipoEvidence, actual.item.id).subscribe({
      next: () => {
        this.detalle.set(null);
        this.seleccionado.set(null);
        this.confirmandoEliminar.set(false);
        this.modoFormulario.set(null);
        this.cargarLista(false);
        this.cargarRoadmap(true);
      },
      error: (error: Error) => {
        this.errorEliminar.set(error.message);
      },
    });
  }

  protected limpiarFiltros(): void {
    this.busqueda.set('');
    this.tipoFiltro.set('');
    this.madurezFiltro.set('');
    this.cargarLista();
  }

  protected agregarNota(): void {
    const actual = this.detalle();

    if (!actual || !this.soportaNotas(actual.tipoEvidence)) {
      return;
    }

    this.mensajeNota.set(null);

    if (!this.notaTexto().trim()) {
      this.erroresFormulario.set({ ...this.erroresFormulario(), notaTexto: 'Ingresa una nota.' });
      return;
    }

    this.estadoNota.set('guardando');

    this.evidence
      .agregarNota(actual.tipoEvidence, actual.item.id, {
        texto: this.notaTexto().trim(),
        tipo: this.notaTipo(),
      })
      .subscribe({
        next: () => {
          this.notaTexto.set('');
          this.estadoNota.set('guardado');
          this.mensajeNota.set('Nota agregada.');
          this.cargarNotasSiAplica(actual.tipoEvidence);
        },
        error: (error: Error) => {
          this.estadoNota.set('error');
          this.mensajeNota.set(error.message);
        },
      });
  }

  protected onBusquedaInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.busqueda.set(target.value);
    }
  }

  protected onTipoFiltroChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.tipoFiltro.set(this.esTipoEvidence(target.value) ? target.value : '');
      this.cargarLista();
    }
  }

  protected onMadurezFiltroChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.madurezFiltro.set(this.esEstadoMadurez(target.value) ? target.value : '');
      this.cargarLista();
    }
  }

  protected onTipoEvidenceChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esTipoEvidence(target.value)) {
      this.restablecerFormularioCrear(target.value);
      this.limpiarErrorCampo('tipoEvidence');
    }
  }

  protected onTextInput(event: Event, campo: WritableTextField): void {
    const target = event.target;

    if (target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement) {
      this.setTextField(campo, target.value);
      this.limpiarErrorCampo(this.mapearCampoError(campo));
    }
  }

  protected onEstadoProyectoChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esEstadoProyecto(target.value)) {
      this.formEstadoProyecto.set(target.value);
      this.limpiarErrorCampo('estadoProyecto');
    }
  }

  protected onEstadoMadurezChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esEstadoMadurez(target.value)) {
      this.formEstadoMadurez.set(target.value);
      this.limpiarErrorCampo('estadoMadurez');
    }
  }

  protected onTipoArtefactoChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esTipoArtefacto(target.value)) {
      this.formTipoArtefacto.set(target.value);
      this.limpiarErrorCampo('tipoArtefacto');
    }
  }

  protected onCertificacionChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.formCertificacionId.set(target.value);
      this.limpiarErrorCampo('certificacionId');
    }
  }

  protected onNotaTipoChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement && this.esTipoNota(target.value)) {
      this.notaTipo.set(target.value);
    }
  }

  protected onNotaTextoInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLTextAreaElement) {
      this.notaTexto.set(target.value);
      this.limpiarErrorCampo('notaTexto');
      this.estadoNota.set('idle');
      this.mensajeNota.set(null);
    }
  }

  protected etiquetaTipo(tipo: TipoEvidenceV1): string {
    const labels: Record<TipoEvidenceV1, string> = {
      Proyecto: 'Proyecto',
      Laboratorio: 'Laboratorio',
      Writeup: 'Writeup',
      ArtefactoTecnico: 'Artefacto tecnico',
      CertificacionObtenida: 'Certificacion obtenida',
    };

    return labels[tipo];
  }

  protected etiquetaMadurez(estado: EstadoMadurez): string {
    const labels: Record<EstadoMadurez, string> = {
      Borrador: 'Borrador',
      Documentado: 'Documentado',
      ListoPortafolio: 'Listo para Portfolio',
      Publicado: 'Publicado',
    };

    return labels[estado];
  }

  protected etiquetaEstadoProyecto(estado: EstadoProyecto): string {
    const labels: Record<EstadoProyecto, string> = {
      Idea: 'Idea',
      Desarrollo: 'Desarrollo',
      Documentado: 'Documentado',
      Publicado: 'Publicado',
    };

    return labels[estado];
  }

  protected etiquetaTipoArtefacto(tipo: TipoArtefacto): string {
    const labels: Record<TipoArtefacto, string> = {
      Script: 'Script',
      Herramienta: 'Herramienta',
      Cheatsheet: 'Cheatsheet',
      Dashboard: 'Dashboard',
      Playbook: 'Playbook',
      ReglaDeteccion: 'Regla de deteccion',
      ConsultaSiem: 'Consulta SIEM',
      Automatizacion: 'Automatizacion',
      Plantilla: 'Plantilla',
      Otro: 'Otro',
    };

    return labels[tipo];
  }

  protected etiquetaNota(tipo: TipoNota): string {
    const labels: Record<TipoNota, string> = {
      Nota: 'Nota',
      Hallazgo: 'Hallazgo',
      Actualizacion: 'Actualizacion',
      Autoexplicacion: 'Autoexplicacion',
    };

    return labels[tipo];
  }

  protected fechaCorta(fecha: string | null): string {
    if (!fecha) {
      return 'Sin fecha';
    }

    const parsed = fecha.includes('T') ? new Date(fecha) : new Date(`${fecha}T00:00:00`);

    if (Number.isNaN(parsed.getTime())) {
      return fecha;
    }

    return new Intl.DateTimeFormat('es-PE', { day: '2-digit', month: 'short', year: 'numeric' }).format(parsed);
  }

  protected textoTemas(item: EvidenceItemV1): string {
    return item.temas.length === 0 ? 'Sin tema vinculado' : item.temas.map((tema) => tema.nombre).join(', ');
  }

  protected textoHerramientas(item: EvidenceItemV1): string {
    return item.herramientas.length === 0
      ? 'Sin herramientas vinculadas'
      : item.herramientas.map((herramienta) => herramienta.nombre).join(', ');
  }

  protected certificacionNombre(id: string): string {
    return this.certificaciones().find((certificacion) => certificacion.id === id)?.nombre ?? id;
  }

  protected puedeVincularTema(tipo: TipoEvidenceV1): boolean {
    return tipo !== 'CertificacionObtenida';
  }

  protected soportaNotas(tipo: TipoEvidenceV1): boolean {
    return tipo !== 'CertificacionObtenida';
  }

  protected trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  private cargarLista(mostrarLoading = true, reabrirId: string | null = null): void {
    if (mostrarLoading) {
      this.cargandoLista.set(true);
    }

    this.errorLista.set(null);

    this.evidence
      .listar({
        temaId: this.temaIdContextual(),
        tipoEvidence: this.tipoFiltro(),
        estadoMadurez: this.madurezFiltro(),
      })
      .subscribe({
        next: (lista) => {
          this.lista.set(lista.items);
          this.totalServidor.set(lista.total);
          this.cargandoLista.set(false);

          if (reabrirId) {
            const item = lista.items.find((candidate) => candidate.id === reabrirId);

            if (item) {
              this.abrirDetalle(item);
              return;
            }
          }

          const actual = this.seleccionado();

          if (!actual) {
            return;
          }

          const itemActualizado = lista.items.find(
            (item) => item.id === actual.id && item.tipoEvidence === actual.tipoEvidence,
          );

          if (!itemActualizado) {
            this.detalle.set(null);
            this.seleccionado.set(null);
            return;
          }

          this.seleccionado.set(itemActualizado);
          this.sincronizarDetalleConItem(itemActualizado);
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

  private cargarCertificaciones(): void {
    this.errorCertificaciones.set(null);

    this.evidence.listarCertificaciones().subscribe({
      next: (certificaciones) => this.certificaciones.set(certificaciones),
      error: (error: Error) => this.errorCertificaciones.set(error.message),
    });
  }

  private cargarNotasSiAplica(tipoEvidence: TipoEvidenceV1): void {
    if (!this.soportaNotas(tipoEvidence)) {
      this.notas.set([]);
      return;
    }

    this.cargandoNotas.set(true);
    this.errorNotas.set(null);

    this.evidence.listarNotas().subscribe({
      next: (notas) => {
        this.notas.set(notas);
        this.cargandoNotas.set(false);
      },
      error: (error: Error) => {
        this.errorNotas.set(error.message);
        this.cargandoNotas.set(false);
      },
    });
  }

  private crearEvidence(): void {
    const solicitud = this.payloadCrear();

    this.evidence.crear(solicitud).subscribe({
      next: (respuesta) => {
        const id = respuesta.id;
        const temaId = this.temaIdContextual();

        if (id && temaId && this.puedeVincularTema(solicitud.tipoEvidence)) {
          this.vincularEvidenceCreada(solicitud.tipoEvidence, id, temaId);
          return;
        }

        this.finalizarWriteExitoso(
          solicitud.tipoEvidence === 'CertificacionObtenida' && temaId
            ? 'Certificacion obtenida creada. Su contexto por tema depende de Certificacion-Tema.'
            : 'Evidencia creada.',
          id,
        );
      },
      error: (error: Error) => {
        this.estadoWrite.set('error');
        this.mensajeWrite.set(error.message);
      },
    });
  }

  private vincularEvidenceCreada(tipoEvidence: TipoEvidenceV1, id: string, temaId: string): void {
    this.evidence.vincularTema(tipoEvidence, id, temaId).subscribe({
      next: () => this.finalizarWriteExitoso('Evidencia creada y vinculada al tema.', id),
      error: (error: Error) => {
        this.estadoWrite.set('error');
        this.mensajeWrite.set(`La evidencia fue creada, pero no se pudo vincular al tema. ${error.message}`);
        this.cargarLista(false);
      },
    });
  }

  private actualizarEvidence(): void {
    const actual = this.detalle();

    if (!actual) {
      this.estadoWrite.set('error');
      this.mensajeWrite.set('Selecciona una evidencia para editar.');
      return;
    }

    this.evidence.actualizar(this.payloadActualizar(actual)).subscribe({
      next: () => this.finalizarWriteExitoso('Cambios guardados.', actual.item.id),
      error: (error: Error) => {
        this.estadoWrite.set('error');
        this.mensajeWrite.set(error.message);
      },
    });
  }

  private finalizarWriteExitoso(mensaje: string, id: string | null): void {
    this.estadoWrite.set('guardado');
    this.mensajeWrite.set(mensaje);
    this.modoFormulario.set(null);
    this.cargarLista(false, id);
    this.cargarRoadmap(true);
  }

  private sincronizarDetalleConItem(item: EvidenceItemV1): void {
    const actual = this.detalle();

    if (!actual || actual.item.id !== item.id || actual.item.tipoEvidence !== item.tipoEvidence) {
      return;
    }

    switch (actual.tipoEvidence) {
      case 'Proyecto':
      case 'Laboratorio':
      case 'Writeup':
      case 'ArtefactoTecnico':
      case 'CertificacionObtenida':
        this.detalle.set({ ...actual, item });
        break;
    }
  }

  private aplicarTemaContextual(temaId: string | null): void {
    this.temaIdMalformado.set(!!temaId && !EvidencePage.guidRegex.test(temaId));
    this.temaIdContextual.set(temaId && EvidencePage.guidRegex.test(temaId) ? temaId : null);
  }

  private formularioValido(): boolean {
    const errores: Partial<Record<CampoFormulario, string>> = {};
    const tipo = this.formTipoEvidence();

    if (!this.esTipoEvidence(tipo)) {
      errores.tipoEvidence = 'Selecciona un tipo valido.';
    }

    if (this.modoFormulario() === 'editar' && !this.esEstadoMadurez(this.formEstadoMadurez())) {
      errores.estadoMadurez = 'Selecciona una madurez valida.';
    }

    if (tipo === 'Proyecto') {
      this.validarTextoRequerido(this.formNombre(), 'nombre', errores);
      this.validarUrlOpcional(this.formRepositorioUrl(), 'url', errores);
      this.validarRangoFechas(this.formFechaInicio(), this.formFechaFin(), errores);
    }

    if (tipo === 'Laboratorio') {
      this.validarTextoRequerido(this.formNombre(), 'nombre', errores);
      this.validarEnteroOpcional(this.formTiempoInvertidoMinutos(), 'tiempoInvertidoMinutos', errores);
    }

    if (tipo === 'Writeup') {
      this.validarTextoRequerido(this.formTitulo(), 'titulo', errores);
      this.validarUrlOpcional(this.formUrl(), 'url', errores);
    }

    if (tipo === 'ArtefactoTecnico') {
      this.validarTextoRequerido(this.formNombre(), 'nombre', errores);
      if (!this.esTipoArtefacto(this.formTipoArtefacto())) {
        errores.tipoArtefacto = 'Selecciona un tipo de artefacto valido.';
      }
      this.validarUrlOpcional(this.formContenidoOUrl(), 'url', errores);
    }

    if (tipo === 'CertificacionObtenida') {
      if (!this.formCertificacionId()) {
        errores.certificacionId = 'Selecciona una certificacion.';
      }
      if (!this.formFechaObtencion()) {
        errores.fecha = 'Ingresa la fecha de obtencion.';
      }
      this.validarUrlOpcional(this.formEvidenciaUrl(), 'url', errores);
    }

    this.erroresFormulario.set(errores);

    return Object.keys(errores).length === 0;
  }

  private payloadCrear() {
    const tipoEvidence = this.formTipoEvidence();

    switch (tipoEvidence) {
      case 'Proyecto':
        return { tipoEvidence, payload: { nombre: this.formNombre().trim() } };
      case 'Laboratorio':
        return {
          tipoEvidence,
          payload: {
            nombre: this.formNombre().trim(),
            objetivo: this.valorOpcional(this.formObjetivo()),
            entornoVms: this.valorOpcional(this.formEntornoVms()),
            hallazgos: this.valorOpcional(this.formHallazgos()),
            tiempoInvertidoMinutos: this.numeroOpcional(this.formTiempoInvertidoMinutos()),
            fecha: this.valorOpcional(this.formFecha()),
          },
        };
      case 'Writeup':
        return { tipoEvidence, payload: { titulo: this.formTitulo().trim() } };
      case 'ArtefactoTecnico':
        return {
          tipoEvidence,
          payload: {
            tipoArtefacto: this.formTipoArtefacto(),
            nombre: this.formNombre().trim(),
          },
        };
      case 'CertificacionObtenida':
        return {
          tipoEvidence,
          payload: {
            certificacionId: this.formCertificacionId(),
            fechaObtencion: this.formFechaObtencion(),
          },
        };
    }
  }

  private payloadActualizar(actual: EvidenceDetail) {
    switch (actual.tipoEvidence) {
      case 'Proyecto':
        return {
          tipoEvidence: actual.tipoEvidence,
          id: actual.item.id,
          payload: {
            nombre: this.formNombre().trim(),
            descripcion: this.valorOpcional(this.formDescripcion()),
            estado: this.formEstadoProyecto(),
            estadoMadurez: this.formEstadoMadurez(),
            repositorioUrl: this.valorOpcional(this.formRepositorioUrl()),
            fechaInicio: this.valorOpcional(this.formFechaInicio()),
            fechaFin: this.valorOpcional(this.formFechaFin()),
          },
        };
      case 'Laboratorio':
        return {
          tipoEvidence: actual.tipoEvidence,
          id: actual.item.id,
          payload: {
            nombre: this.formNombre().trim(),
            objetivo: this.valorOpcional(this.formObjetivo()),
            entornoVms: this.valorOpcional(this.formEntornoVms()),
            hallazgos: this.valorOpcional(this.formHallazgos()),
            tiempoInvertidoMinutos: this.numeroOpcional(this.formTiempoInvertidoMinutos()),
            fecha: this.valorOpcional(this.formFecha()),
            estadoMadurez: this.formEstadoMadurez(),
          },
        };
      case 'Writeup':
        return {
          tipoEvidence: actual.tipoEvidence,
          id: actual.item.id,
          payload: {
            titulo: this.formTitulo().trim(),
            plataformaOrigen: this.valorOpcional(this.formPlataformaOrigen()),
            url: this.valorOpcional(this.formUrl()),
            fecha: this.valorOpcional(this.formFecha()),
            estadoMadurez: this.formEstadoMadurez(),
          },
        };
      case 'ArtefactoTecnico':
        return {
          tipoEvidence: actual.tipoEvidence,
          id: actual.item.id,
          payload: {
            tipoArtefacto: this.formTipoArtefacto(),
            nombre: this.formNombre().trim(),
            contenidoOUrl: this.valorOpcional(this.formContenidoOUrl()),
            lenguajeTecnologia: this.valorOpcional(this.formLenguajeTecnologia()),
            estadoMadurez: this.formEstadoMadurez(),
          },
        };
      case 'CertificacionObtenida':
        return {
          tipoEvidence: actual.tipoEvidence,
          id: actual.item.id,
          payload: {
            evidenciaUrl: this.valorOpcional(this.formEvidenciaUrl()),
            estadoMadurez: this.formEstadoMadurez(),
          },
        };
    }
  }

  private cargarFormularioEdicion(actual: EvidenceDetail): void {
    this.formTipoEvidence.set(actual.tipoEvidence);
    this.formEstadoMadurez.set(actual.datos.estadoMadurez);

    switch (actual.tipoEvidence) {
      case 'Proyecto':
        this.formNombre.set(actual.datos.nombre);
        this.formDescripcion.set(actual.datos.descripcion ?? '');
        this.formEstadoProyecto.set(actual.datos.estado);
        this.formRepositorioUrl.set(actual.datos.repositorioUrl ?? '');
        this.formFechaInicio.set(actual.datos.fechaInicio ?? '');
        this.formFechaFin.set(actual.datos.fechaFin ?? '');
        break;
      case 'Laboratorio':
        this.formNombre.set(actual.datos.nombre);
        this.formObjetivo.set(actual.datos.objetivo ?? '');
        this.formEntornoVms.set(actual.datos.entornoVms ?? '');
        this.formHallazgos.set(actual.datos.hallazgos ?? '');
        this.formTiempoInvertidoMinutos.set(actual.datos.tiempoInvertidoMinutos?.toString() ?? '');
        this.formFecha.set(actual.datos.fecha ?? '');
        break;
      case 'Writeup':
        this.formTitulo.set(actual.datos.titulo);
        this.formPlataformaOrigen.set(actual.datos.plataformaOrigen ?? '');
        this.formUrl.set(actual.datos.url ?? '');
        this.formFecha.set(actual.datos.fecha ?? '');
        break;
      case 'ArtefactoTecnico':
        this.formTipoArtefacto.set(actual.datos.tipoArtefacto);
        this.formNombre.set(actual.datos.nombre);
        this.formContenidoOUrl.set(actual.datos.contenidoOUrl ?? '');
        this.formLenguajeTecnologia.set(actual.datos.lenguajeTecnologia ?? '');
        break;
      case 'CertificacionObtenida':
        this.formCertificacionId.set(actual.datos.certificacionId);
        this.formFechaObtencion.set(actual.datos.fechaObtencion);
        this.formEvidenciaUrl.set(actual.datos.evidenciaUrl ?? '');
        break;
    }
  }

  private restablecerFormularioCrear(tipo: TipoEvidenceV1): void {
    this.formTipoEvidence.set(tipo);
    this.formNombre.set('');
    this.formTitulo.set('');
    this.formDescripcion.set('');
    this.formObjetivo.set('');
    this.formEntornoVms.set('');
    this.formHallazgos.set('');
    this.formTiempoInvertidoMinutos.set('');
    this.formEstadoProyecto.set('Idea');
    this.formEstadoMadurez.set(tipo === 'CertificacionObtenida' ? 'Documentado' : 'Borrador');
    this.formRepositorioUrl.set('');
    this.formFechaInicio.set('');
    this.formFechaFin.set('');
    this.formPlataformaOrigen.set('');
    this.formUrl.set('');
    this.formFecha.set('');
    this.formTipoArtefacto.set('Script');
    this.formContenidoOUrl.set('');
    this.formLenguajeTecnologia.set('');
    this.formCertificacionId.set(this.certificaciones()[0]?.id ?? '');
    this.formFechaObtencion.set(new Date().toISOString().slice(0, 10));
    this.formEvidenciaUrl.set('');
  }

  private obtenerTemasOrdenados(): TemaRoadmapVistaV1[] {
    return (this.vista()?.fases ?? [])
      .flatMap((fase) => fase.temas)
      .sort((a, b) => a.nombre.localeCompare(b.nombre));
  }

  private notaPerteneceADetalle(nota: NotaResumen, tipoEvidence: TipoEvidenceV1, id: string): boolean {
    const campos: Partial<Record<TipoEvidenceV1, keyof NotaResumen>> = {
      Proyecto: 'proyectoId',
      Laboratorio: 'laboratorioId',
      Writeup: 'writeupId',
      ArtefactoTecnico: 'artefactoTecnicoId',
    };

    const campo = campos[tipoEvidence];

    return campo ? nota[campo] === id : false;
  }

  private validarTextoRequerido(
    value: string,
    campo: 'nombre' | 'titulo',
    errores: Partial<Record<CampoFormulario, string>>,
  ): void {
    if (!value.trim()) {
      errores[campo] = campo === 'nombre' ? 'Ingresa un nombre.' : 'Ingresa un titulo.';
    }
  }

  private validarEnteroOpcional(
    value: string,
    campo: 'tiempoInvertidoMinutos',
    errores: Partial<Record<CampoFormulario, string>>,
  ): void {
    if (!value.trim()) {
      return;
    }

    const numero = Number(value);

    if (!Number.isInteger(numero) || numero < 0) {
      errores[campo] = 'Ingresa minutos como numero entero positivo.';
    }
  }

  private validarUrlOpcional(
    value: string,
    campo: 'url',
    errores: Partial<Record<CampoFormulario, string>>,
  ): void {
    const normalizado = value.trim();

    if (!normalizado || !normalizado.includes('://')) {
      return;
    }

    try {
      const url = new URL(normalizado);

      if (url.protocol !== 'http:' && url.protocol !== 'https:') {
        errores[campo] = 'Ingresa una URL http o https.';
      }
    } catch {
      errores[campo] = 'Ingresa una URL valida.';
    }
  }

  private validarRangoFechas(
    inicio: string,
    fin: string,
    errores: Partial<Record<CampoFormulario, string>>,
  ): void {
    if (inicio && fin && inicio > fin) {
      errores.fechaFin = 'La fecha fin no puede ser anterior al inicio.';
    }
  }

  private limpiarErrorCampo(campo: CampoFormulario): void {
    const errores = { ...this.erroresFormulario() };
    delete errores[campo];
    this.erroresFormulario.set(errores);
    this.estadoWrite.set('idle');
    this.mensajeWrite.set(null);
  }

  private limpiarNotaState(): void {
    this.notaTexto.set('');
    this.notaTipo.set('Nota');
    this.estadoNota.set('idle');
    this.mensajeNota.set(null);
    this.errorNotas.set(null);
  }

  private valorOpcional(value: string): string | null {
    const normalizado = value.trim();

    return normalizado.length > 0 ? normalizado : null;
  }

  private numeroOpcional(value: string): number | null {
    const normalizado = value.trim();

    return normalizado.length > 0 ? Number(normalizado) : null;
  }

  private normalizarTexto(value: string): string {
    return value.trim().toLocaleLowerCase('es-PE');
  }

  private esTipoEvidence(value: string): value is TipoEvidenceV1 {
    return TIPOS_EVIDENCE.includes(value as TipoEvidenceV1);
  }

  private esEstadoMadurez(value: string): value is EstadoMadurez {
    return ESTADOS_MADUREZ.includes(value as EstadoMadurez);
  }

  private esEstadoProyecto(value: string): value is EstadoProyecto {
    return ESTADOS_PROYECTO.includes(value as EstadoProyecto);
  }

  private esTipoArtefacto(value: string): value is TipoArtefacto {
    return TIPOS_ARTEFACTO.includes(value as TipoArtefacto);
  }

  private esTipoNota(value: string): value is TipoNota {
    return TIPOS_NOTA.includes(value as TipoNota);
  }

  private setTextField(campo: WritableTextField, value: string): void {
    const setters: Record<WritableTextField, (value: string) => void> = {
      nombre: (next) => this.formNombre.set(next),
      titulo: (next) => this.formTitulo.set(next),
      descripcion: (next) => this.formDescripcion.set(next),
      objetivo: (next) => this.formObjetivo.set(next),
      entornoVms: (next) => this.formEntornoVms.set(next),
      hallazgos: (next) => this.formHallazgos.set(next),
      tiempoInvertidoMinutos: (next) => this.formTiempoInvertidoMinutos.set(next),
      repositorioUrl: (next) => this.formRepositorioUrl.set(next),
      fechaInicio: (next) => this.formFechaInicio.set(next),
      fechaFin: (next) => this.formFechaFin.set(next),
      plataformaOrigen: (next) => this.formPlataformaOrigen.set(next),
      url: (next) => this.formUrl.set(next),
      fecha: (next) => this.formFecha.set(next),
      contenidoOUrl: (next) => this.formContenidoOUrl.set(next),
      lenguajeTecnologia: (next) => this.formLenguajeTecnologia.set(next),
      fechaObtencion: (next) => this.formFechaObtencion.set(next),
      evidenciaUrl: (next) => this.formEvidenciaUrl.set(next),
    };

    setters[campo](value);
  }

  private mapearCampoError(campo: WritableTextField): CampoFormulario {
    if (campo === 'repositorioUrl' || campo === 'contenidoOUrl' || campo === 'evidenciaUrl') {
      return 'url';
    }

    if (campo === 'fechaObtencion') {
      return 'fecha';
    }

    return campo as CampoFormulario;
  }
}

type WritableTextField =
  | 'nombre'
  | 'titulo'
  | 'descripcion'
  | 'objetivo'
  | 'entornoVms'
  | 'hallazgos'
  | 'tiempoInvertidoMinutos'
  | 'repositorioUrl'
  | 'fechaInicio'
  | 'fechaFin'
  | 'plataformaOrigen'
  | 'url'
  | 'fecha'
  | 'contenidoOUrl'
  | 'lenguajeTecnologia'
  | 'fechaObtencion'
  | 'evidenciaUrl';
