import { Component, Input } from '@angular/core';
import { DocumentService } from './document.service';

@Component({
  selector: 'app-document-viewer',
  template: `
    <div *ngIf="documentId">
      <h4>Viewer (prototype)</h4>
      <p>Entrez l'id de la page pour prévisualiser (ex: 1)</p>
      <input [(ngModel)]="pageId" type="number" />
      <button (click)="show()">Afficher preview</button>
      <div *ngIf="previewUrl">
        <img [src]="previewUrl" alt="Preview" style="max-width:600px; max-height:800px" />
      </div>
    </div>
  `
})
export class DocumentViewerComponent {
  @Input() documentId!: number;
  pageId = 1;
  previewUrl?: string;

  constructor(private svc: DocumentService) {}

  show() {
    this.previewUrl = this.svc.getPagePreviewUrl(this.documentId, this.pageId);
  }
}
