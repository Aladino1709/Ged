import { Component } from '@angular/core';
import { DocumentService } from './document.service';

@Component({
  selector: 'app-document-search',
  template: `
  <h2>Recherche par numéro de titre</h2>
  <input [(ngModel)]="titleNumber" placeholder="Numéro de titre" />
  <button (click)="search()">Rechercher</button>

  <div *ngIf="document">
    <h3>Document: {{document.titleNumber}} (id: {{document.id}})</h3>
    <input type="file" (change)="onFile($event)" />
    <input type="number" [(ngModel)]="pageNumber" placeholder="Numéro de page" />
    <button (click)="upload()">Uploader page</button>

    <app-document-viewer [documentId]="document.id"></app-document-viewer>
  </div>
  `
})
export class DocumentSearchComponent {
  titleNumber = '';
  document: any = null;
  file?: File;
  pageNumber = 1;

  constructor(private svc: DocumentService) {}

  search() {
    // endpoint GET /api/documents?titleNumber=xxx (not yet implemented server-side list; use GetDocument by id after creation)
    // For prototype, call createDocument if not exists
    this.svc.createDocument({ titleNumber: this.titleNumber, description: '' }).subscribe((res: any) => {
      this.document = res;
    }, err => {
      // if already exists, attempt to find by title via direct call (not implemented here)
      alert('Erreur ou déjà existant, voir backend');
    });
  }

  onFile(e: any) { this.file = e.target.files[0]; }

  upload() {
    if (!this.file || !this.document) return alert('Fichier ou document manquant');
    this.svc.uploadPage(this.document.id, this.file, this.pageNumber).subscribe(event => {
      // simplifié: rafraîchir ou afficher message
      alert('Upload en cours / terminé (événement)');
    }, err => alert('Erreur upload'));
  }
}
