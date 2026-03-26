using System.Net;
using DB_parse.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<ZakupkiXmlService>(client =>
{
    client.BaseAddress = new Uri("https://zakupki.gov.ru/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("DB-parse-service/1.0");
});

var app = builder.Build();

app.MapGet("/api/xml/{registryNumber}", async (
    string registryNumber,
    ZakupkiXmlService xmlService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(registryNumber))
    {
        return Results.BadRequest(new { error = "Registry number is required." });
    }

    try
    {
        var xmlDocument = await xmlService.GetXmlAsync(registryNumber, cancellationToken);
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
    service = "zakupki-xml-proxy",
    endpoint = "/api/xml/{registryNumber}"
}));

app.Run();
