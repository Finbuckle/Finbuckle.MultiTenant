using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace IdentitySampleApp;

public class MultiTenantPageRouteModelConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        foreach (var selector in model.Selectors)
        {
            selector.AttributeRouteModel?.Template =
                AttributeRouteModel.CombineTemplates("{__tenant__}", selector.AttributeRouteModel.Template);
        }
    }
}