using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace DB_parse.Services;

public interface IProcurementXmlStorageService
{
    Task SaveAsync(string procurementId, string xmlDocument, CancellationToken cancellationToken);
}

public sealed class ProcurementXmlStorageService(IConfiguration configuration) : IProcurementXmlStorageService
{
    private readonly string _connectionString = configuration.GetConnectionString("Postgres")
                                               ?? throw new InvalidOperationException(
                                                   "Connection string 'Postgres' is not configured.");

    public async Task SaveAsync(string procurementId, string xmlDocument, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var query = @"
            INSERT INTO procurement_xml_documents (procurement_id, xml_document)
            VALUES (@id, @xml)
            ON CONFLICT (procurement_id)
            DO UPDATE SET xml_document = EXCLUDED.xml_document;
        ";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("id", procurementId);
        cmd.Parameters.Add("xml", NpgsqlDbType.Xml).Value = xmlDocument;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
