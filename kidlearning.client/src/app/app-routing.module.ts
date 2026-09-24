import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LetterGameComponent } from './components/letters/letter-game.component';
import { PencilPageComponent } from './components/pencil-page/pencil-page.component';

const routes: Routes = [
  {
      path: '',
      component: HomeComponent,
    },
    {
      path: 'pencil',
      component: PencilPageComponent,
    },
    {
      path: 'letter',
      component: LetterGameComponent,
    },
    {
      path: 'admin',
      component: HomeComponent,
    },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
