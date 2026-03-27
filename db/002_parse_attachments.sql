CREATE TABLE IF NOT EXISTS procurement_attachments (
    id SERIAL PRIMARY KEY,
    procurement_xml_id INTEGER NOT NULL,

    file_name TEXT,
    url TEXT,
    description TEXT,

    created_at TIMESTAMP DEFAULT NOW(),

    FOREIGN KEY (procurement_xml_id)
        REFERENCES procurement_xml_documents(id)
        ON DELETE CASCADE
);



CREATE OR REPLACE FUNCTION parse_procurement_attachments()
RETURNS void AS
$$
DECLARE
    rec RECORD;
    attachment XML;
BEGIN
    FOR rec IN
        SELECT id, xml_document
        FROM procurement_xml_documents
    LOOP
        FOR attachment IN
            SELECT unnest(xpath(
                '//*[local-name()="attachmentInfo"]',
                rec.xml_document
            ))
        LOOP
            INSERT INTO procurement_attachments (
                procurement_xml_id,
                file_name,
                url,
                description
            )
            VALUES (
                rec.id,

                -- fileName
                (xpath('//*[local-name()="fileName"]/text()', attachment))[1]::text,

                -- url
                (xpath('//*[local-name()="url"]/text()', attachment))[1]::text,

                -- description
                (xpath('//*[local-name()="docDescription"]/text()', attachment))[1]::text
            );
        END LOOP;
    END LOOP;
END;
$$ LANGUAGE plpgsql;



CREATE EXTENSION IF NOT EXISTS pg_cron;

SELECT cron.unschedule('parse-attachments-job');

SELECT cron.schedule(
    'parse-attachments-job',
    '*/5 * * * *',
    $$SELECT parse_procurement_attachments();$$
);