import { Component, Renderer2, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { LoadingService } from './core/services/loading.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  private sub: Subscription | null = null;
  private overlayEl: HTMLElement | null = null;

  constructor(
    public loadingService: LoadingService,
    private renderer: Renderer2,
    private el: ElementRef
  ) {}

  ngOnInit(): void {
    this.overlayEl = this.el.nativeElement.querySelector('.spinner-overlay');
    this.sub = this.loadingService.loading$.subscribe(v => {
      if (this.overlayEl) {
        this.renderer.setStyle(this.overlayEl, 'display', v ? 'flex' : 'none');
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
