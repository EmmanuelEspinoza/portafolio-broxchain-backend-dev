using FondoInversion.Models;

public interface IFlujoRepository: IBaseRepository<flujo>
{
    Task BulkUpsertAsync(List<flujo> flujos);
    Task<int> GetLastFlujo();
    Task<List<flujo>> GetFlujoByUserID(int userId);
}