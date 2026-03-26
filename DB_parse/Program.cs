using System.Net;
using DB_parse.Configuration;
using DB_parse.Services;

var builder = WebApplication.CreateBuilder(args);
var apiSettings = builder.Configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>()
                  ?? throw new InvalidOperationException("Api settings are not configured.");

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.AddSingleton<IProcurementXmlStorageService, ProcurementXmlStorageService>();
builder.Services.AddHttpClient<ZakupkiXmlService>(client =>
{
    client.BaseAddress = new Uri(apiSettings.Upstream.BaseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(apiSettings.Upstream.UserAgent);
});

var app = builder.Build();

app.MapGet(apiSettings.Endpoints.GetXmlByRegistryNumber, async (
    string registryNumber,
    ZakupkiXmlService xmlService,
    IProcurementXmlStorageService storageService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(registryNumber))
    {
        return Results.BadRequest(new { error = "Registry number is required." });
    }

    try
    {
        var xmlDocument = await xmlService.GetXmlAsync(registryNumber, cancellationToken);
        await storageService.SaveAsync(registryNumber, xmlDocument, cancellationToken);
        return Results.Content(xmlDocument, "application/xml; charset=utf-8");
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
    catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.NotFound)
    {
        return Results.NotFound(new { error = exception.Message });
    }
    catch (HttpRequestException exception)
    {
        return Results.Problem(
            title: "Failed to fetch XML document.",
            detail: exception.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
});

app.MapGet("/", () => Results.Ok(new
{
    service = apiSettings.ServiceName,
    endpoint = apiSettings.Endpoints.GetXmlByRegistryNumber
}));

app.Run();
