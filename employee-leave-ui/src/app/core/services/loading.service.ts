import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private _loading = new BehaviorSubject<boolean>(false);
  private _pendingCount = 0;

  get loading$(): Observable<boolean> {
    return this._loading.asObservable();
  }

  get loading(): boolean {
    return this._loading.value;
  }

  show(): void {
    this._pendingCount++;
    this._loading.next(true);
  }

  hide(): void {
    this._pendingCount--;
    if (this._pendingCount <= 0) {
      this._pendingCount = 0;
      this._loading.next(false);
    }
  }
}
