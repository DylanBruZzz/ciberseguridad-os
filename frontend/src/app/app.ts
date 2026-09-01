import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { UsuarioActual, UsuarioActualService } from './core/usuario-actual.service';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly usuario = signal<UsuarioActual | null>(null);
  protected readonly cargandoUsuario = signal(true);
  protected readonly errorUsuario = signal<string | null>(null);
  protected readonly iniciales = computed(() => this.usuario()?.nombre.slice(0, 1).toUpperCase() ?? 'A');

  protected readonly navegacion = [
    { label: 'Roadmap', route: '/roadmap' },
    { label: 'Study', route: '/study' },
    { label: 'Resources', route: '/resources' },
    { label: 'Evidence', route: '/evidence' },
    { label: 'Portfolio', route: '/portfolio' },
  ];

  public constructor(private readonly usuarioActual: UsuarioActualService) {}

  public ngOnInit(): void {
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
}
