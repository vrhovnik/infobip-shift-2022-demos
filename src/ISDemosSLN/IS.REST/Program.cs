Console.WriteLine("Calling REST to get back the information!");

using var client = new  HttpClient();
var clusterApiUrl = Environment.GetEnvironmentVariable("APIMASTERURL") ?? "https://localhost:8090";
client.BaseAddress = new Uri(clusterApiUrl, UriKind.RelativeOrAbsolute);
