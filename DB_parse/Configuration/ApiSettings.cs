namespace DB_parse.Configuration;

public sealed class ApiSettings
{
    public const string SectionName = "Api";

    public string ServiceName { get; init; } = "zakupki-xml-proxy";

    public EndpointSettings Endpoints { get; init; } = new();

    public UpstreamSettings Upstream { get; init; } = new();

    public sealed class EndpointSettings
    {
        public string GetXmlByRegistryNumber { get; init; } = "/api/xml/{registryNumber}";
    }

    public sealed class UpstreamSettings
    {
        public string BaseUrl { get; init; } = "https://zakupki.gov.ru/";

        public string UserAgent { get; init; } = "DB-parse-service/1.0";

        public RouteSettings[] Routes { get; init; } = [];
    }

    public sealed class RouteSettings
    {
        public string DocumentType { get; init; } = string.Empty;

        public string Path { get; init; } = string.Empty;

        public string QueryKey { get; init; } = string.Empty;
    }
}
