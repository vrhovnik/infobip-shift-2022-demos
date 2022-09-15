using Docker.DotNet;
using Docker.DotNet.BasicAuth;
using Docker.DotNet.Models;
using IS.Web.Interfaces;
using IS.Web.Models;
using IS.Web.Options;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Options;

namespace IS.Web.Services;

public class ACRService : IContainerRegistryService
{
    private readonly IAzure azure;
    private readonly AzureAdOptions azureAdOptions;
    private readonly ILogger<ACRService> logger;

    public ACRService(IOptions<AzureAdOptions> azureAdOptionsValue, ILogger<ACRService> logger)
    {
        this.logger = logger;
        azureAdOptions = azureAdOptionsValue.Value;

        var credentials = SdkContext.AzureCredentialsFactory
            .FromServicePrincipal(azureAdOptions.ClientId, azureAdOptions.ClientSecret,
                azureAdOptions.TenantId, AzureEnvironment.AzureGlobalCloud);

        azure = Microsoft.Azure.Management.Fluent.Azure
            .Configure()
            .Authenticate(credentials)
            .WithSubscription(azureAdOptions.SubscriptionId);
    }

    public async Task<IRegistry> GetRegistryRepositoriesAsync(string containerRegistryName)
    {
        logger.LogInformation("Retrieving info about registry {RegistryName}", containerRegistryName);
        var registry =
            await azure.ContainerRegistries.GetByResourceGroupAsync(azureAdOptions.AcrRG,
                containerRegistryName);
        logger.LogInformation("Registry info retrieved!");
        return registry;
    }

    public async Task<List<DockerImageViewModel>> GetImagesForRepositoryAsync(string containerRegistryName)
    {
        var list = new List<DockerImageViewModel>();
        logger.LogInformation("Getting docker client to call VM to get images at {DateCalled}", DateTime.Now);

        try
        {
            var registry = await GetRegistryRepositoriesAsync(containerRegistryName);
            var credRegistry = await registry.GetCredentialsAsync();

            var credentials =
                new BasicAuthCredentials(credRegistry.Username, credRegistry.AccessKeys[AccessKeyType.Primary]);

            using var client = new DockerClientConfiguration(new Uri(registry.LoginServerUrl), credentials)
                .CreateClient();

            var listImages = await client.Images.ListImagesAsync(
                new ImagesListParameters { All = true });

            logger.LogInformation("Retrieved client, doing list images on the system");
            foreach (var img in listImages)
            {
                if (img.RepoTags is not { Count: > 0 }) continue;
                var name = img.RepoTags[0];
                if (!name.Contains("none"))
                    list.Add(new DockerImageViewModel
                    {
                        Id = img.ID,
                        Name = name
                    });
            }
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }

        logger.LogInformation("Listening images done");
        return list;
    }

    public List<DockerImageViewModel> GetPredefinedImages()
    {
        var list = new List<DockerImageViewModel>
        {
            new()
            {
                Id = "csacoreimages.azurecr.io/tta/web:1.0",
                Name = "csacoreimages.azurecr.io/tta/web:1.0"
            },
            new()
            {
                Id = "csacoreimages.azurecr.io/ew/web:latest",
                Name = "csacoreimages.azurecr.io/ew/web:latest"
            },
            new()
            {
                Id = "csacoreimages.azurecr.io/ew/init:v2",
                Name = "csacoreimages.azurecr.io/ew/init:v2"
            },
            new()
            {
                Id = "csacoreimages.azurecr.io/ew/status:v2",
                Name = "csacoreimages.azurecr.io/ew/status:v2"
            },
            new()
            {
                Id = "csacoreimages.azurecr.io/ew/report:v1",
                Name = "csacoreimages.azurecr.io/ew/report:v1"
            },
            new()
            {
                Id = "ubuntu:latest",
                Name = "ubuntu:latest"
            }
        };
        return list;
    }
}