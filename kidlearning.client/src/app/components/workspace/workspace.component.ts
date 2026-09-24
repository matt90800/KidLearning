import { Component, HostListener, Input, inject } from '@angular/core';
import { PencilComponent } from "../pencil/pencil.component";
import { Pencil } from '../pencil/pencil';
import { PencilStore } from './pencil.store';

@Component({
  selector: 'app-workspace',
  standalone: true,
  imports: [PencilComponent],
  templateUrl: './workspace.component.html',
  styleUrl: './workspace.component.css'
})
export class WorkspaceComponent {
  store = inject(PencilStore);
  public pencils = this.store.pencils;

  @HostListener('document:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent) {

    if (event.key !== 'Delete')
      return;

    const selected = this.store.selected();

    if (!selected)
      return;

    this.store.pencils.update(list =>
      list.filter(p => p.id !== selected.id)
    );

    this.store.selected.set(null);

  }

}
