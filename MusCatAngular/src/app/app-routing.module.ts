import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { PerformersPanelComponent } from './components/performers-panel/performers-panel.component';
import { AlbumsPanelComponent } from './components/albums-panel/albums-panel.component';

const routes: Routes = [
  { path: 'performers', component: PerformersPanelComponent },
  { path: 'albums', component: AlbumsPanelComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    scrollPositionRestoration: 'enabled',
    anchorScrolling: 'enabled',
    scrollOffset: [0, 50]
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
