import { Component, inject } from '@angular/core';
import { PencilStore } from '../../components/workspace/pencil.store';
import { ToolbarComponent } from '../../components/toolbar/toolbar.component';
import { WorkspaceComponent } from '../../components/workspace/workspace.component';

@Component({
  selector: 'app-pencil-page',
  standalone: true,
  imports: [ToolbarComponent, WorkspaceComponent],
  templateUrl: './pencil-page.component.html',
  styleUrl: './pencil-page.component.css'
})
export class PencilPageComponent {

  store = inject(PencilStore);

  private nextId=1;

  constructor(){

    this.reset();

  }

  addSmall(){
    this.create(70,"#FF6B6B");
  }

  addMedium(){
    this.create(110,"#4ECDC4");
  }

  addLarge(){
    this.create(160,"#556270");
  }

  reset(){
    this.store.pencils.set([]);

    this.nextId=1;

    this.addSmall();
    this.addMedium();
    this.addLarge();

  }

  create(width:number,color:string){

    this.store.pencils.update(list => [
      ...list,
      {
        id: this.nextId++,
        x: 100,
        y: 100,
        width,
        height: 18,
        rotation: 0,
        color
      }

    ]);
  }
}
