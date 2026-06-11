import { Injectable } from '@angular/core';

export interface ToastMessage {
  id: number;
  text: string;
  type: 'success' | 'error' | 'warning' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _messages: ToastMessage[] = [];
  private nextId = 0;

  get messages(): ToastMessage[] {
    return this._messages;
  }

  success(text: string): void {
    this.add(text, 'success');
  }

  error(text: string): void {
    this.add(text, 'error');
  }

  warning(text: string): void {
    this.add(text, 'warning');
  }

  info(text: string): void {
    this.add(text, 'info');
  }

  private add(text: string, type: ToastMessage['type']): void {
    const id = ++this.nextId;
    this._messages.push({ id, text, type });
    setTimeout(() => this.remove(id), 5000);
  }

  remove(id: number): void {
    this._messages = this._messages.filter(m => m.id !== id);
  }
}
