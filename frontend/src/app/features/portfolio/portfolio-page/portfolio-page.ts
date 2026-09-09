import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { RouterLink } from '@angular/router';
import {
  ArtefactoTecnicoDetalle,
  CertificacionObtenidaDetalle,
  EstadoMadurez,
  EvidenceDetail,
  EvidenceItemV1,
  LaboratorioDetalle,
  NotaResumen,
  ProyectoDetalle,
  TipoEvidenceV1,
  WriteupDetalle,
} from '../../evidence/evidence.models';
import { EvidenceService } from '../../evidence/evidence.service';
import {
  ESTADOS_PORTFOLIO,
  EstadoMadurezPortfolio,
  PortafolioArtefactoTecnicoDto,
  PortafolioCertificacionObtenidaDto,
  PortafolioDto,
  PortafolioLaboratorioDto,
  PortafolioProyectoDto,
  PortafolioWriteupDto,
  PortfolioHerramienta,
  PortfolioItem,
  PortfolioTema,
  TIPOS_PORTFOLIO,
  TipoEvidencePortfolio,
} from '../portfolio.models';
import { PortfolioService } from '../portfolio.service';
import { crearFocoPanel } from '../../../shared/foco-panel';

@Component({
  selector: 'app-portfolio-page',
  imports: [RouterLink],
  templateUrl: './portfolio-page.html',
  styleUrl: './portfolio-page.css',
})
export class PortfolioPage implements OnInit {
  private readonly enfocarPanel = crearFocoPanel();
  private readonly destroyRef = inject(DestroyRef);
  private solicitudLista?: Subscription;
  private solicitudDetalle?: Subscription;
  private solicitudNotas?: Subscription;
  protected readonly tiposPortfolio = TIPOS_PORTFOLIO;
  protected readonly estadosPortfolio = ESTADOS_PORTFOLIO;

  protected readonly portafolio = signal<PortafolioDto | null>(null);
  protected readonly items = signal<PortfolioItem[]>([]);
  protected readonly cargandoLista = signal(true);
  protected readonly errorLista = signal<string | null>(null);
  protected readonly busqueda = signal('');
  protected readonly tipoFiltro = signal<TipoEvidencePortfolio | ''>('');
  protected readonly madurezFiltro = signal<EstadoMadurezPortfolio | ''>('');
  protected readonly seleccionado = signal<PortfolioItem | null>(null);
  protected readonly detalle = signal<EvidenceDetail | null>(null);
  protected readonly cargandoDetalle = signal(false);
  protected readonly errorDetalle = signal<string | null>(null);
  protected readonly notas = signal<NotaResumen[]>([]);
  protected readonly cargandoNotas = signal(false);
  protected readonly errorNotas = signal<string | null>(null);

  protected readonly itemsVisibles = computed(() => {
    const texto = this.normalizarTexto(this.busqueda());

    return this.items().filter((item) => {
      if (!this.esMadurezPortfolio(item.estadoMadurez)) {
        return false;
      }

      if (texto.length === 0) {
        return true;
      }

      return (
        this.normalizarTexto(item.titulo).includes(texto) ||
        this.normalizarTexto(item.tipoEvidence).includes(texto) ||
        this.normalizarTexto(this.etiquetaMadurez(item.estadoMadurez)).includes(texto) ||
        this.normalizarTexto(item.resumen ?? '').includes(texto) ||
        item.temas.some((tema) => this.normalizarTexto(tema.nombre).includes(texto)) ||
        item.herramientas.some((herramienta) => this.normalizarTexto(herramienta.nombre).includes(texto))
      );
    });
  });
  protected readonly listos = computed(() =>
    this.itemsVisibles().filter((item) => item.estadoMadurez === 'ListoPortafolio'),
  );
  protected readonly publicados = computed(() =>
    this.itemsVisibles().filter((item) => item.estadoMadurez === 'Publicado'),
  );
  protected readonly hayFiltros = computed(
    () => !!this.tipoFiltro() || !!this.madurezFiltro() || this.busqueda().trim().length > 0,
  );
  protected readonly totalServidor = computed(() => this.portafolio()?.resumen.total ?? 0);
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
    private readonly portfolio: PortfolioService,
    private readonly evidence: EvidenceService,
  ) {}

  public ngOnInit(): void {
    this.cargarLista();
  }

  protected recargarLista(): void {
    this.cargarLista();
  }

  protected abrirDetalle(item: PortfolioItem): void {
    this.enfocarPanel();
    this.solicitudDetalle?.unsubscribe();
    this.solicitudNotas?.unsubscribe();
    this.seleccionado.set(item);
    this.detalle.set(null);
    this.cargandoDetalle.set(true);
    this.errorDetalle.set(null);
    this.notas.set([]);
    this.errorNotas.set(null);
    this.cargandoNotas.set(false);

    this.solicitudDetalle = this.evidence.obtenerDetalle(this.toEvidenceItem(item))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  protected onBusquedaInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.busqueda.set(target.value);
    }
  }

  protected onTipoFiltroChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.tipoFiltro.set(this.esTipoPortfolio(target.value) ? target.value : '');
      this.cargarLista();
    }
  }

  protected onMadurezFiltroChange(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.madurezFiltro.set(this.esMadurezPortfolio(target.value) ? target.value : '');
      this.cargarLista();
    }
  }

  protected limpiarFiltros(): void {
    this.busqueda.set('');
    this.tipoFiltro.set('');
    this.madurezFiltro.set('');
    this.cargarLista();
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

  protected etiquetaTipoArtefacto(tipo: string): string {
    const labels: Record<string, string> = {
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

    return labels[tipo] ?? tipo;
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

  protected textoTemas(item: PortfolioItem): string {
    return item.temas.length === 0 ? 'Sin tema vinculado' : item.temas.map((tema) => tema.nombre).join(', ');
  }

  protected textoHerramientas(item: PortfolioItem): string {
    return item.herramientas.length === 0
      ? 'Sin herramientas vinculadas'
      : item.herramientas.map((herramienta) => herramienta.nombre).join(', ');
  }

  protected soportaNotas(tipo: TipoEvidenceV1): boolean {
    return tipo !== 'CertificacionObtenida';
  }

  protected trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  private cargarLista(mostrarLoading = true): void {
    this.solicitudLista?.unsubscribe();
    if (mostrarLoading) {
      this.cargandoLista.set(true);
    }

    this.errorLista.set(null);

    this.solicitudLista = this.portfolio
      .listar({
        tipoEvidence: this.tipoFiltro(),
        estadoMadurez: this.madurezFiltro(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (portafolio) => {
          const items = this.aplanar(portafolio);
          this.portafolio.set(portafolio);
          this.items.set(items);
          this.cargandoLista.set(false);
          this.sincronizarSeleccion(items);
        },
        error: (error: Error) => {
          this.errorLista.set(error.message);
          this.cargandoLista.set(false);
        },
      });
  }

  private cargarNotasSiAplica(tipoEvidence: TipoEvidenceV1): void {
    if (!this.soportaNotas(tipoEvidence)) {
      return;
    }

    this.cargandoNotas.set(true);
    this.errorNotas.set(null);

    this.solicitudNotas?.unsubscribe();
    this.solicitudNotas = this.evidence.listarNotas().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  private sincronizarSeleccion(items: PortfolioItem[]): void {
    const actual = this.seleccionado();

    if (!actual) {
      return;
    }

    const vigente = items.find((item) => item.id === actual.id && item.tipoEvidence === actual.tipoEvidence);

    if (!vigente || !this.esMadurezPortfolio(vigente.estadoMadurez)) {
      this.solicitudDetalle?.unsubscribe();
      this.solicitudNotas?.unsubscribe();
      this.cargandoDetalle.set(false);
      this.cargandoNotas.set(false);
      this.errorDetalle.set(null);
      this.errorNotas.set(null);
      this.notas.set([]);
      this.seleccionado.set(null);
      this.detalle.set(null);
      return;
    }

    this.seleccionado.set(vigente);
  }

  private aplanar(portafolio: PortafolioDto): PortfolioItem[] {
    return [
      ...portafolio.proyectos.map((proyecto) => this.mapearProyecto(proyecto)),
      ...portafolio.laboratorios.map((laboratorio) => this.mapearLaboratorio(laboratorio)),
      ...portafolio.writeups.map((writeup) => this.mapearWriteup(writeup)),
      ...portafolio.artefactosTecnicos.map((artefacto) => this.mapearArtefacto(artefacto)),
      ...portafolio.certificacionesObtenidas.map((certificacion) => this.mapearCertificacion(certificacion)),
    ];
  }

  private mapearProyecto(proyecto: PortafolioProyectoDto): PortfolioItem {
    return {
      id: proyecto.proyectoId,
      tipoEvidence: 'Proyecto',
      titulo: proyecto.nombre,
      estadoMadurez: proyecto.estadoMadurez,
      fechaPrincipal: proyecto.fechaFin ?? proyecto.fechaInicio,
      fechaEtiqueta: proyecto.fechaFin ? 'Fin' : 'Inicio',
      resumen: proyecto.descripcion,
      temas: this.mapearTemas(proyecto.temas),
      herramientas: this.mapearHerramientas(proyecto.herramientas),
    };
  }

  private mapearLaboratorio(laboratorio: PortafolioLaboratorioDto): PortfolioItem {
    return {
      id: laboratorio.laboratorioId,
      tipoEvidence: 'Laboratorio',
      titulo: laboratorio.nombre,
      estadoMadurez: laboratorio.estadoMadurez,
      fechaPrincipal: laboratorio.fecha,
      fechaEtiqueta: 'Fecha',
      resumen: laboratorio.objetivo ?? laboratorio.hallazgos,
      temas: this.mapearTemas(laboratorio.temas),
      herramientas: this.mapearHerramientas(laboratorio.herramientas),
    };
  }

  private mapearWriteup(writeup: PortafolioWriteupDto): PortfolioItem {
    return {
      id: writeup.writeupId,
      tipoEvidence: 'Writeup',
      titulo: writeup.titulo,
      estadoMadurez: writeup.estadoMadurez,
      fechaPrincipal: writeup.fecha,
      fechaEtiqueta: 'Fecha',
      resumen: writeup.plataformaOrigen ?? writeup.url,
      temas: this.mapearTemas(writeup.temas),
      herramientas: [],
    };
  }

  private mapearArtefacto(artefacto: PortafolioArtefactoTecnicoDto): PortfolioItem {
    return {
      id: artefacto.artefactoTecnicoId,
      tipoEvidence: 'ArtefactoTecnico',
      titulo: artefacto.nombre,
      estadoMadurez: artefacto.estadoMadurez,
      fechaPrincipal: null,
      fechaEtiqueta: 'Fecha',
      resumen: artefacto.lenguajeTecnologia ?? this.etiquetaTipoArtefacto(artefacto.tipoArtefacto),
      temas: this.mapearTemas(artefacto.temas),
      herramientas: this.mapearHerramientas(artefacto.herramientas),
    };
  }

  private mapearCertificacion(certificacion: PortafolioCertificacionObtenidaDto): PortfolioItem {
    return {
      id: certificacion.certificacionObtenidaId,
      tipoEvidence: 'CertificacionObtenida',
      titulo: certificacion.nombre,
      estadoMadurez: certificacion.estadoMadurez,
      fechaPrincipal: certificacion.fechaObtencion,
      fechaEtiqueta: 'Obtencion',
      resumen: certificacion.proveedor ?? certificacion.tipoCosto,
      temas: [],
      herramientas: [],
    };
  }

  private mapearTemas(temas: { temaId: string; nombre: string }[]): PortfolioTema[] {
    return temas.map((tema) => ({ id: tema.temaId, nombre: tema.nombre }));
  }

  private mapearHerramientas(herramientas: { herramientaId: string; nombre: string; categoria: string | null }[]): PortfolioHerramienta[] {
    return herramientas.map((herramienta) => ({
      id: herramienta.herramientaId,
      nombre: herramienta.nombre,
      categoria: herramienta.categoria,
    }));
  }

  private toEvidenceItem(item: PortfolioItem): EvidenceItemV1 {
    const fecha = item.fechaPrincipal ?? '';

    return {
      id: item.id,
      tipoEvidence: item.tipoEvidence,
      titulo: item.titulo,
      estadoMadurez: item.estadoMadurez,
      fechaCreacionUtc: fecha,
      fechaModificacionUtc: null,
      fechaActividadUtc: fecha,
      fechaReferencia: item.fechaPrincipal,
      temas: item.temas,
      herramientas: item.herramientas.map((herramienta) => ({
        id: herramienta.id,
        nombre: herramienta.nombre,
      })),
    };
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

  protected proyectoDetalle(datos: EvidenceDetail): ProyectoDetalle | null {
    return datos.tipoEvidence === 'Proyecto' ? datos.datos : null;
  }

  protected laboratorioDetalle(datos: EvidenceDetail): LaboratorioDetalle | null {
    return datos.tipoEvidence === 'Laboratorio' ? datos.datos : null;
  }

  protected writeupDetalle(datos: EvidenceDetail): WriteupDetalle | null {
    return datos.tipoEvidence === 'Writeup' ? datos.datos : null;
  }

  protected artefactoDetalle(datos: EvidenceDetail): ArtefactoTecnicoDetalle | null {
    return datos.tipoEvidence === 'ArtefactoTecnico' ? datos.datos : null;
  }

  protected certificacionDetalle(datos: EvidenceDetail): CertificacionObtenidaDetalle | null {
    return datos.tipoEvidence === 'CertificacionObtenida' ? datos.datos : null;
  }

  private normalizarTexto(value: string): string {
    return value.trim().toLocaleLowerCase('es-PE');
  }

  private esTipoPortfolio(value: string): value is TipoEvidencePortfolio {
    return TIPOS_PORTFOLIO.includes(value as TipoEvidencePortfolio);
  }

  private esMadurezPortfolio(value: string): value is EstadoMadurezPortfolio {
    return ESTADOS_PORTFOLIO.includes(value as EstadoMadurezPortfolio);
  }
}
