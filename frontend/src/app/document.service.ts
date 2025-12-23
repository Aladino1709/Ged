import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DocumentService {
  constructor(private http: HttpClient) {}

  createDocument(payload: any) {
    return this.http.post('/api/documents', payload);
  }

  searchByTitle(titleNumber: string) {
    return this.http.get(`/api/documents?titleNumber=${encodeURIComponent(titleNumber)}`);
  }

  uploadPage(documentId: number, file: File, pageNumber: number): Observable<HttpEvent<any>> {
    const fd = new FormData();
    fd.append('file', file);
    fd.append('pageNumber', String(pageNumber));
    return this.http.post<any>(`/api/documents/${documentId}/pages`, fd, {
      reportProgress: true,
      observe: 'events'
    });
  }

  getPagePreviewUrl(documentId: number, pageId: number, pageIndex = 0) {
    return `/api/documents/${documentId}/pages/${pageId}/preview?pageIndex=${pageIndex}`;
  }
}
