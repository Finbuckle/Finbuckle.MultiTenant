using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BasePathStrategySample.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    
    public TenantInfo? TenantInfo { get; set; }
    public IEnumerable<TenantInfo> Tenants { get; set; }

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    
    public void OnGet()
    {
        TenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        var store = HttpContext.RequestServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Tenants = store.GetAllAsync().Result;
    }
}