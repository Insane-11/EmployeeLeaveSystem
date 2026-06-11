import { Component } from '@angular/core';
import { ToastService } from '../services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: false,
  templateUrl: './toast-container.component.html'
})
export class ToastContainerComponent {
  constructor(public toastService: ToastService) {}
}
