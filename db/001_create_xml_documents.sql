CREATE TABLE IF NOT EXISTS procurement_xml_documents (
    id SERIAL PRIMARY KEY,
    procurement_id TEXT NOT NULL UNIQUE,
    xml_document XML NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);