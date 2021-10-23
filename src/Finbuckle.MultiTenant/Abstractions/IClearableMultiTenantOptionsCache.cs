namespace Finbuckle.MultiTenant
{
    public interface IClearableMultiTenantOptionsCache
    {
        void Clear();
        void Clear(string tenantId);
        void ClearAll();
    }
}