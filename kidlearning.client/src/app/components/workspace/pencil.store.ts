import { Injectable, signal } from '@angular/core';
import { Pencil } from '../pencil/pencil';

@Injectable({
  providedIn: 'root'
})
export class PencilStore {

  pencils = signal<Pencil[]>([]);

  selected = signal<Pencil | null>(null);

  changeColor(color: string) {

    const selected = this.selected();

    if (!selected) {
      return;
    }

    this.pencils.update(list =>
      list.map(p =>
        p.id === selected.id
          ? {
            ...p,
            color
          }
          : p
      )
    );

    this.selected.set({
      ...selected,
      color
    });

  }
}
