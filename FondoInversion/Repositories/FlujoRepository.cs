using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.EntityFrameworkCore;

public class FlujoRepository : BaseRepository<flujo>, IFlujoRepository
{
    public FlujoRepository(AppDbContext context) : base(context){ }

    public async Task BulkUpsertAsync(List<flujo> flujos)
    {
        //filtramos primero los que ya estan hechos
        await AddRangeAsync(flujos);
        await SaveAsync();
    }

    public Task<List<flujo>> GetFlujoByUserID(int userId)
    {
        var result = _dbSet.Where(p => p.inversionista_id == userId);
        return result.ToListAsync();
    }

    public async Task<int> GetLastFlujo()
    {
        
        var result = _dbSet.OrderByDescending(e => e.id).FirstOrDefault()?.id ?? 0; 
        
        return result;
    }
}