using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentitySample.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    
    public AppTenantInfo? AppTenantInfo { get; private set; }

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        AppTenantInfo = HttpContext.GetMultiTenantContext<AppTenantInfo>()?.TenantInfo;
    }
}