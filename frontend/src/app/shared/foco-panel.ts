import { afterNextRender, ElementRef, inject, Injector } from '@angular/core';

// Una selección debe llevar al detalle incluso cuando queda debajo de una lista móvil larga.
export function crearFocoPanel(): () => void {
  const host = inject<ElementRef<HTMLElement>>(ElementRef);
  const injector = inject(Injector);
  return () => {
    afterNextRender(() => {
      const panel = host.nativeElement.querySelector<HTMLElement>('.detail-panel');
      panel?.focus({ preventScroll: true });
      if (panel && panel.getBoundingClientRect().width > host.nativeElement.getBoundingClientRect().width * 0.8) {
        panel.scrollIntoView({ block: 'start' });
      }
    }, { injector });
  };
}
