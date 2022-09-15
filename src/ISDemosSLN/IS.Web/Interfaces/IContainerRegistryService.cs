using IS.Web.Models;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;

namespace IS.Web.Interfaces;

public interface IContainerRegistryService
{
    Task<IRegistry> GetRegistryRepositoriesAsync(string containerRegistryName);
    Task<List<DockerImageViewModel>> GetImagesForRepositoryAsync(string containerRegistryName);
    List<DockerImageViewModel> GetPredefinedImages();
}