using IS.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IS.Web.Pages.Kubek;

public class StorePageModel : PageModel
{
    private readonly ILogger<StorePageModel> logger;
    private readonly IKubernetesCrud kubernetesCrud;

    public StorePageModel(ILogger<StorePageModel> logger, IKubernetesCrud kubernetesCrud)
    {
        this.logger = logger;
        this.kubernetesCrud = kubernetesCrud;
    }

    public void OnGet()
    {
        logger.LogInformation("Loaded scenario {0}", DateTime.Now);
    }

    public async Task<RedirectToPageResult> OnPostCreateScenarioAsync()
    {
        if (string.IsNullOrEmpty(NamespaceName))
        {
            InfoText = "Name is required!";
            return RedirectToPage("/Kubek/Store");
        }
        logger.LogInformation("Creating scenario");
        StoreIp = await kubernetesCrud.CreateScenarioAsync(NamespaceName);
        StoreIp = $"http://{StoreIp}";
        logger.LogInformation($"Finished scenario, IP is {StoreIp}");
        return RedirectToPage("/Kubek/Store");
    }
        
    public async Task<RedirectToPageResult> OnPostDeleteScenarioAsync()
    {
        if (string.IsNullOrEmpty(NamespaceName))
        {
            InfoText = "Name is required!";
            return RedirectToPage("/Kubek/Store");
        }
        logger.LogInformation("Deleting scenario");
        await kubernetesCrud.DeleteScenarioAsync(NamespaceName);
        logger.LogInformation("Finish deleting scenario");
        InfoText = "Scenario has been deleted";
        return RedirectToPage("/Kubek/Store");
    }

    [BindProperty] public string NamespaceName { get; set; }
    [TempData] public string StoreIp { get; set; }
    [TempData] public string InfoText { get; set; }
}