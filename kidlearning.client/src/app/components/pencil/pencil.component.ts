import { Component, EventEmitter, Input, Output, computed, inject } from '@angular/core';
import { CdkDrag } from '@angular/cdk/drag-drop';
import { Pencil } from './pencil';
import { PencilStore } from '../workspace/pencil.store';

@Component({
  selector: 'app-pencil',
  standalone: true,
  imports: [CdkDrag],
  templateUrl: './pencil.component.html',
  styleUrl: './pencil.component.css'
})
export class PencilComponent {

  @Input() pencil!: Pencil;
  store = inject(PencilStore);

  rotate(event: MouseEvent) {
    console.log(event)
    event.preventDefault();

    let step = 45;

    if (event.shiftKey) {
      step = step/2;
    }

    if (event.ctrlKey) {
      step = step*2;
    }

    this.pencil.rotation += step;

  }

  onWheel(event: WheelEvent) {

    if (!event.altKey) {
      return;
    }

    event.preventDefault();

    const direction = event.deltaY > 0 ? -1 : 1;

    this.pencil.rotation += direction * 5;

  }

  isSelected = computed(() =>
    this.store.selected()?.id === this.pencil.id
  );

  select() {
    this.store.selected.set(this.pencil);
  }

}
