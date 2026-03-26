using System.Net;

namespace DB_parse.Services;

public sealed class ZakupkiXmlService(HttpClient httpClient)
{
    private static readonly (string DocumentType, string Path, string QueryKey)[] Routes =
    [
        ("notice", "epz/order/notice/printForm/viewXml.html", "regNumber"),
        ("contract", "epz/contract/printForm/viewXml.html", "contractReestrNumber"),
        ("contract223", "epz/contractfz223/printForm/viewXml.html", "contractNumber")
    ];

    public async Task<string> GetXmlAsync(string registryNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(registryNumber))
        {
            throw new ArgumentException("Registry number is required.");
        }

        foreach (var route in Routes)
        {
            var requestUri = $"{route.Path}?{route.QueryKey}={Uri.EscapeDataString(registryNumber)}";
            using var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Remote service returned {(int)response.StatusCode} {response.ReasonPhrase} while resolving document type '{route.DocumentType}'. Body: {TrimResponse(responseBody)}",
                    null,
                    response.StatusCode);
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        throw new HttpRequestException(
            "XML document was not found on zakupki.gov.ru for any supported document type.",
            null,
            HttpStatusCode.NotFound);
    }

    private static string TrimResponse(string responseBody)
    {
        const int maxLength = 300;

        if (responseBody.Length <= maxLength)
        {
            return responseBody;
        }

        return responseBody[..maxLength] + "...";
    }
}
