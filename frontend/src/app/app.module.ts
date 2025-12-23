import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { DocumentSearchComponent } from './document-search.component';
import { DocumentViewerComponent } from './document-viewer.component';

@NgModule({
  declarations: [DocumentSearchComponent, DocumentViewerComponent],
  imports: [BrowserModule, HttpClientModule, FormsModule],
  bootstrap: [DocumentSearchComponent]
})
export class AppModule {}
