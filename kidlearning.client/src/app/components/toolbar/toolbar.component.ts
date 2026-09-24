import { Component, EventEmitter, Output, inject } from '@angular/core';
import { PencilStore } from '../workspace/pencil.store';

@Component({
  selector: 'app-toolbar',
  standalone: true,
  imports: [],
  templateUrl: './toolbar.component.html',
  styleUrl: './toolbar.component.css'
})
export class ToolbarComponent {

  store = inject(PencilStore);

   @Output() small = new EventEmitter<void>();
  @Output() medium = new EventEmitter<void>();
  @Output() large = new EventEmitter<void>();
  @Output() reset = new EventEmitter<void>();

  changeColor(event: Event) {

    const input = event.target as HTMLInputElement;

    this.store.changeColor(input.value);

  }
}
