import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { PerformersPanelComponent } from './components/performers-panel/performers-panel.component';
import { AlbumsPanelComponent } from './components/albums-panel/albums-panel.component';
import { HttpClientModule } from '@angular/common/http';
import { PerformerService } from './services/performer.service';
import { MatCardModule, MatButtonModule, MatIconModule, MatChipsModule,
  MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule } from '@angular/material';
import { MatPaginatorModule } from '@angular/material/paginator';
import { PerformerComponent } from './components/performer/performer.component';
import { PerformerDialog } from './components/performer-dialog/performer-dialog.component';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AppComponent,
    PerformersPanelComponent,
    AlbumsPanelComponent,
    PerformerComponent,
    PerformerDialog
  ],
  entryComponents: [
    PerformerDialog
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    MatButtonModule,
    MatCardModule,
    MatPaginatorModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    FormsModule,
    BrowserAnimationsModule
  ],
  providers: [PerformerService],
  bootstrap: [AppComponent]
})
export class AppModule { }
