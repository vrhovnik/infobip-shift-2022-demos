using Spectre.Console;

Console.WriteLine("Calling REST to get back the information!");

using var client = new HttpClient(new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});
var clusterApiUrl = Environment.GetEnvironmentVariable("APIMASTERURL") ?? "https://localhost:8090";
AnsiConsole.WriteLine($"Reading from {clusterApiUrl}");
client.BaseAddress = new Uri(clusterApiUrl, UriKind.RelativeOrAbsolute);
client.DefaultRequestHeaders.Add("Accept", "application/json");

var bearerToken = Environment.GetEnvironmentVariable("BearerToken");

if (string.IsNullOrEmpty(bearerToken))
{
    AnsiConsole.WriteException(new UnauthorizedAccessException("Missing bearer token"), ExceptionFormats.ShowLinks);
    return;
}

var namespaceName = AnsiConsole.Ask("Provide namespace name to traverse through pods", string.Empty);

if (string.IsNullOrEmpty(namespaceName))
{
    AnsiConsole.WriteLine("Namespace was not provided, continuing with default namespace");
    namespaceName = "default";
}

var requestData = new HttpRequestMessage
{
    Method = HttpMethod.Get,
    RequestUri = new Uri($"{clusterApiUrl}/api/v1/namespaces/{namespaceName}/pods", UriKind.RelativeOrAbsolute)
};
requestData.Headers.TryAddWithoutValidation("Authorization", $"Bearer {bearerToken}");

var result = await client.SendAsync(requestData);
if (!result.IsSuccessStatusCode)
{
    AnsiConsole.WriteLine("There has been an error accessing cluster, check content for more detais");
    AnsiConsole.WriteLine(result.ReasonPhrase);
    return;
}
var pods = await result.Content.ReadAsStringAsync();
AnsiConsole.WriteLine(pods);