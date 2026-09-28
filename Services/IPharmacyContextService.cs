namespace PharmacyIMS.Services
{
    public interface IPharmacyContextService
    {
        Task<string> BuildContextSummaryAsync();
    }
}
