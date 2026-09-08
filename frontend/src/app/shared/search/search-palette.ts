import { DOCUMENT } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, computed, inject, linkedSignal, output, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { SearchItem } from './search.models';
import { SearchService } from './search.service';

@Component({
  selector: 'app-search-palette',
  templateUrl: './search-palette.html',
  styleUrl: './search-palette.css',
  providers: [SearchService],
})
export class SearchPalette implements AfterViewInit, OnDestroy {
  private readonly documento = inject(DOCUMENT);
  private readonly router = inject(Router);
  private readonly input = viewChild.required<ElementRef<HTMLInputElement>>('queryInput');
  private readonly dialog = viewChild.required<ElementRef<HTMLElement>>('dialog');
  private readonly overflowAnterior = this.documento.body.style.overflow;
  protected readonly search = inject(SearchService);
  protected readonly query = signal('');
  protected readonly grupos = computed(() => this.search.buscar(this.query()));
  protected readonly resultados = computed(() => this.grupos().flatMap((grupo) => grupo.items));
  protected readonly activo = linkedSignal({ source: this.resultados, computation: () => 0 });
  protected readonly activoId = computed(() => {
    const item = this.resultados()[this.activo()];
    return item ? this.optionId(item) : null;
  });
  protected readonly hayMas = computed(() => this.grupos().some((grupo) => grupo.total > grupo.items.length));
  protected readonly navegando = signal(false);
  protected readonly errorNavegacion = signal(false);
  public readonly cerrar = output<void>();

  public ngAfterViewInit(): void {
    this.documento.body.style.overflow = 'hidden';
    this.input().nativeElement.focus();
    this.search.cargar();
  }

  public ngOnDestroy(): void {
    this.documento.body.style.overflow = this.overflowAnterior;
  }

  protected escribir(event: Event): void {
    if (event.target instanceof HTMLInputElement) this.query.set(event.target.value);
  }

  protected optionId(item: SearchItem): string {
    return `search-${item.tipo}-${item.tipo === 'Evidence' ? item.queryParams.tipoEvidence + '-' : ''}${item.id}`;
  }

  protected async abrir(item: SearchItem): Promise<void> {
    if (this.navegando()) return;
    this.navegando.set(true);
    this.errorNavegacion.set(false);
    try {
      const url = this.router.createUrlTree([item.ruta], { queryParams: item.queryParams });
      if (this.router.url === this.router.serializeUrl(url) || await this.router.navigateByUrl(url)) {
        this.cerrar.emit();
      } else {
        this.errorNavegacion.set(true);
      }
    } catch {
      this.errorNavegacion.set(true);
    } finally {
      this.navegando.set(false);
    }
  }

  protected reintentar(): void {
    this.search.reintentar();
    this.input().nativeElement.focus();
  }

  protected teclado(event: KeyboardEvent): void {
    if (event.isComposing) return;
    if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
      this.cerrar.emit();
    } else if (event.key === 'Tab') {
      const controles = [...this.dialog().nativeElement.querySelectorAll<HTMLElement>('input, button:not([tabindex="-1"]):not(:disabled)')];
      const actual = controles.indexOf(this.documento.activeElement as HTMLElement);
      const siguiente = (actual + (event.shiftKey ? -1 : 1) + controles.length) % controles.length;
      event.preventDefault();
      controles[siguiente]?.focus();
    } else if (event.target === this.input().nativeElement) {
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault();
        const total = this.resultados().length;
        if (!total) return;
        this.activo.update((actual) => (actual + (event.key === 'ArrowDown' ? 1 : -1) + total) % total);
        this.documento.getElementById(this.activoId()!)?.scrollIntoView?.({ block: 'nearest' });
      } else if (event.key === 'Enter' && this.resultados()[this.activo()]) {
        event.preventDefault();
        void this.abrir(this.resultados()[this.activo()]);
      }
    }
  }
}
