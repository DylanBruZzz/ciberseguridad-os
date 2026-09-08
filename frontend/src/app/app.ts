import { DOCUMENT } from '@angular/common';
import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { UsuarioActual, UsuarioActualService } from './core/usuario-actual.service';
import { SearchPalette } from './shared/search/search-palette';

interface NavItem {
  readonly label: string;
  readonly route: string;
  readonly ariaLabel: string;
  readonly icon: string;
}

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, SearchPalette],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly documento = inject(DOCUMENT);
  private focoAnterior: HTMLElement | null = null;
  protected readonly usuario = signal<UsuarioActual | null>(null);
  protected readonly cargandoUsuario = signal(true);
  protected readonly errorUsuario = signal<string | null>(null);
  protected readonly moduloActual = signal('Dashboard');
  protected readonly paletteAbierta = signal(false);
  protected readonly iniciales = computed(() => this.usuario()?.nombre.slice(0, 1).toUpperCase() ?? 'A');

  protected readonly navegacion: readonly NavItem[] = [
    {
      label: 'Dashboard',
      route: '/dashboard',
      ariaLabel: 'Abrir Dashboard',
      icon: 'M4 13h6V4H4v9Zm0 7h6v-5H4v5Zm10 0h6v-9h-6v9Zm0-11h6V4h-6v5Z',
    },
    {
      label: 'Roadmap',
      route: '/roadmap',
      ariaLabel: 'Abrir Roadmap',
      icon: 'M5 18.5V6.5A2.5 2.5 0 0 1 7.5 4H19v14H7.5A2.5 2.5 0 0 0 5 20.5Zm3-11h8m-8 4h6',
    },
    {
      label: 'Study',
      route: '/study',
      ariaLabel: 'Abrir Study',
      icon: 'M12 6v6l4 2m5-2a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z',
    },
    {
      label: 'Resources',
      route: '/resources',
      ariaLabel: 'Abrir Resources',
      icon: 'M5 5.5A2.5 2.5 0 0 1 7.5 3H19v15H7.5A2.5 2.5 0 0 0 5 20.5v-15Zm4 2.5h6m-6 4h6',
    },
    {
      label: 'Evidence',
      route: '/evidence',
      ariaLabel: 'Abrir Evidence',
      icon: 'M12 3 4 7v6c0 4 3.5 7 8 8 4.5-1 8-4 8-8V7l-8-4Zm-3 9 2 2 4-5',
    },
    {
      label: 'Portfolio',
      route: '/portfolio',
      ariaLabel: 'Abrir Portfolio',
      icon: 'M4 7a2 2 0 0 1 2-2h3l1.5 2H18a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V7Zm4 5h8',
    },
    {
      label: 'Analytics',
      route: '/analytics',
      ariaLabel: 'Abrir Analytics',
      icon: 'M4 20h16M7 16v-5m5 5V4m5 12V8',
    },
  ];

  public constructor(
    private readonly usuarioActual: UsuarioActualService,
    private readonly router: Router,
    private readonly activatedRoute: ActivatedRoute,
  ) {}

  public ngOnInit(): void {
    this.actualizarModuloActual();
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => {
        this.actualizarModuloActual();
        if (this.paletteAbierta()) this.cerrarBusqueda();
      });

    this.usuarioActual.obtener().subscribe({
      next: (usuario) => {
        this.usuario.set(usuario);
        this.errorUsuario.set(null);
        this.cargandoUsuario.set(false);
      },
      error: (error: Error) => {
        this.usuario.set(null);
        this.errorUsuario.set(error.message);
        this.cargandoUsuario.set(false);
      },
    });
  }

  @HostListener('window:keydown', ['$event'])
  protected manejarAtajoGlobal(event: KeyboardEvent): void {
    if (!event.isComposing && !event.altKey && !event.repeat && (event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.abrirBusqueda();
    }

    if (event.key === 'Escape' && this.paletteAbierta()) {
      this.cerrarBusqueda();
    }
  }

  protected abrirBusqueda(): void {
    if (this.paletteAbierta()) return;
    this.focoAnterior = this.documento.activeElement instanceof HTMLElement ? this.documento.activeElement : null;
    this.paletteAbierta.set(true);
  }

  protected cerrarBusqueda(): void {
    this.paletteAbierta.set(false);
    setTimeout(() => {
      if (this.paletteAbierta()) return;
      const anterior = this.focoAnterior;
      const destino = anterior?.isConnected && anterior !== this.documento.body
        ? anterior : this.documento.querySelector<HTMLElement>('.search-trigger');
      destino?.focus();
    });
  }

  private actualizarModuloActual(): void {
    let route = this.activatedRoute;

    while (route.firstChild) {
      route = route.firstChild;
    }

    this.moduloActual.set(route.snapshot.data['title'] ?? 'Dashboard');
  }
}
