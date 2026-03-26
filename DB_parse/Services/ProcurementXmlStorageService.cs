using Microsoft.Extensions.Configuration;

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

    public Task SaveAsync(string procurementId, string xmlDocument, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(procurementId))
        {
            throw new ArgumentException("Procurement identifier is required.", nameof(procurementId));
        }

        if (string.IsNullOrWhiteSpace(xmlDocument))
        {
            throw new ArgumentException("XML document is required.", nameof(xmlDocument));
        }

        _ = _connectionString;
        _ = cancellationToken;

        // Draft placeholder for PostgreSQL persistence.
        // Real implementation should open a DB connection and insert procurementId + xmlDocument.
        return Task.CompletedTask;
    }
}
