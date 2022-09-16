using IS.Web.Helpers;
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
        logger.LogInformation("Loaded scenario {DateLoaded}", DateTime.Now);
    }

    public async Task<RedirectToPageResult> OnPostCreateScenarioAsync()
    {
        if (string.IsNullOrEmpty(NamespaceName))
        {
            InfoText = "Name is required!";
            return RedirectToPage("/Kubek/Store");
        }
        logger.LogInformation("Creating scenario at {DateCreated}", DateTime.Now);
        StoreIp = await kubernetesCrud.CreateScenarioAsync(NamespaceName);
        StoreIp = $"http://{StoreIp}";
        logger.LogInformation("Finished scenario, IP is {StoreIp}",StoreIp);
        return RedirectToPage("/Kubek/Store");
    }
        
    public async Task<RedirectToPageResult> OnPostDeleteScenarioAsync()
    {
        if (string.IsNullOrEmpty(NamespaceName))
        {
            InfoText = "Name is required!";
            return RedirectToPage("/Kubek/Store");
        }
        logger.LogInformation("Deleting scenario - the whole namespace {NamespaceName}", NamespaceName);
        await kubernetesCrud.DeleteScenarioAsync(NamespaceName);
        logger.LogInformation("Finish deleting scenario at {DateDeleted}", DateTime.Now);
        InfoText = "Scenario has been deleted, check namespaces";
        return RedirectToPage("/Kubek/ListNamespaces");
    }
    
    public async Task<IActionResult> OnPostCreateFQDNAsync()
    {
        if (string.IsNullOrEmpty(StoreIp))
        {
            InfoText = "Store IP is required to create FQDN";
            return RedirectToPage("/Kubek/Store");
        }
        logger.LogInformation("Deleting scenario - the whole namespace {NamespaceName}", NamespaceName);
        var fqdn = await kubernetesCrud.AssignDnsAsync(DnsName, StoreIp, Constants.KubernetesRGName);
        logger.LogInformation("Finish allocating IP {StoreIP} to {FQDN}", StoreIp, fqdn);
        return Redirect(fqdn);
    }

    [BindProperty] public string NamespaceName { get; set; }
    [TempData] public string StoreIp { get; set; }
    [TempData] public string DnsName { get; set; }
    [TempData] public string InfoText { get; set; }
}