using FondoInversion.Models;

public interface IPrecioRepository : IBaseRepository<precio>
{
    Task<precio> GetPricebyDate(DateOnly fecha);
    Task BulkUpsertAsync(List<precio> precios);
    Task<DateOnly> GetLastDate();
    Task<precio?> GetLastPrice();
    Task<List<precio>> GetByLimitDescent(int limit);

    Task<List<FlujoPrecio>> GetFlujoPrecio(int idUser);
}