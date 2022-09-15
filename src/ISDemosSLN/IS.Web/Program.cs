using IS.Web.Interfaces;
using IS.Web.Options;
using IS.Web.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.Configure<StorageOption>(builder.Configuration.GetSection("StorageOptions"));

var storageOption = builder.Configuration.GetSection("StorageOptions").Get<StorageOption>();
builder.Services.AddTransient<IStorageWorker, AzureStorageWorker>(_ =>
    new AzureStorageWorker(storageOption.ConnectionString, storageOption.Container));

builder.Services.AddHealthChecks();
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
        options.Conventions.AddPageRoute("/Info/Index", ""))
    .AddMicrosoftIdentityUI();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health").AllowAnonymous();
    endpoints.MapRazorPages();
    endpoints.MapControllers();
});

app.Run();