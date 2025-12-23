-- Schéma minimal
CREATE TABLE documents (
  id SERIAL PRIMARY KEY,
  title_number TEXT NOT NULL UNIQUE,
  title_type TEXT,
  description TEXT,
  created_by TEXT,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE TABLE pages (
  id SERIAL PRIMARY KEY,
  document_id INTEGER NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
  page_number INTEGER NOT NULL,
  oid OID NOT NULL,
  mime_type TEXT NOT NULL,
  size_bytes BIGINT,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
  CONSTRAINT pages_doc_page_unique UNIQUE (document_id, page_number)
);

CREATE INDEX idx_documents_title_number ON documents (title_number);
