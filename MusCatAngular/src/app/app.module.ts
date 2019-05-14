import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { PerformersPanelComponent } from './components/performers-panel/performers-panel.component';
import { AlbumsPanelComponent } from './components/albums-panel/albums-panel.component';
import { HttpClientModule } from '@angular/common/http';
import { PerformerService } from './services/performer.service';
import { MatCardModule, MatButtonModule, MatIconModule } from '@angular/material';
import { MatPaginatorModule } from '@angular/material/paginator';
import { PerformerComponent } from './components/performer/performer.component';

@NgModule({
  declarations: [
    AppComponent,
    PerformersPanelComponent,
    AlbumsPanelComponent,
    PerformerComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    MatButtonModule,
    MatCardModule,
    MatPaginatorModule,
    MatIconModule,
    BrowserAnimationsModule
  ],
  providers: [PerformerService],
  bootstrap: [AppComponent]
})
export class AppModule { }
