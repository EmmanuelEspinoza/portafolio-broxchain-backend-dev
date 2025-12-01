using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.EntityFrameworkCore;

public class PrecioRepository : BaseRepository<precio>, IPrecioRepository
{
    public PrecioRepository(AppDbContext context) : base(context) { }

    public async Task<precio> GetPricebyDate(DateOnly fecha)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.fecha_precio == fecha) ?? new precio();
    }

    public async Task BulkUpsertAsync(List<precio> precios)
    {
        await AddRangeAsync(precios);
        await SaveAsync();
    }

    public async Task<DateOnly> GetLastDate()
    {
        var result = _context.precios.OrderByDescending(e => e.fecha_precio).FirstOrDefault()?.fecha_precio ?? new DateOnly();

        return result;
    }

    public async Task<precio?> GetLastPrice()
    {
        return _dbSet.OrderByDescending(p => p.fecha_precio).FirstOrDefault();
    }

    public async Task<List<precio>> GetByLimitDescent(int limit)
    {
        return _dbSet.OrderByDescending(e => e.fecha_precio).Take(limit).ToList();
    }

    public async Task<List<FlujoPrecio>> GetFlujoPrecio(int userID)
    {
        var result = new List<FlujoPrecio>();

        var fechasPrecio = await _context.precios
            .Select(p => p.fecha_precio)
            .Distinct()
            .ToListAsync();

        var fechasFlujo = await _context.flujos
            .Where(f => f.inversionista_id == userID)
            .Select(f => f.dia_movimiento)
            .Distinct()
            .ToListAsync();

        var todasLasFechas = fechasPrecio
            .Union(fechasFlujo)
            .OrderBy(f => f)
            .ToList();

        var precios = await _context.precios.ToDictionaryAsync(p => p.fecha_precio);

        var flujos = await _context.flujos
            .Where(f => f.inversionista_id == userID)
            .ToDictionaryAsync(f => f.dia_movimiento);

        decimal unidades = 0;


        foreach (var fecha in todasLasFechas)
        {
            precios.TryGetValue(fecha, out var precio);
            flujos.TryGetValue(fecha, out var flujo);

            unidades = unidades + (decimal)(flujo?.importe ?? 0) + (decimal)(flujo?.comision ?? 0);
            decimal movimiento = (decimal)(flujo?.importe ?? 0);
            var tipoMov = (flujo?.importe ?? 0) < 0 ? "Retiro" : "Deposito";

            var resultado = new FlujoPrecio
            {
                fecha = fecha,
                precio = formatValue((decimal)(precio?.precio_mxn ?? 0), 6),
                movimientoUnidad = formatValue(movimiento, 6),
                movimientoMxn = formatValue((decimal)(movimiento * (decimal)(precio?.precio_mxn ?? 0)), 2),
                saldoUnidad = formatValue(unidades, 6),
                tipoMovimiento = tipoMov,
                saldoMxn = formatValue(unidades * (decimal)(precio?.precio_mxn ?? 0), 2)
            };

            result.Add(resultado);

            if ((flujo?.comision ?? 0) != 0)
            {
                tipoMov = "Comisión";
                resultado = new FlujoPrecio
                {
                    fecha = fecha,
                    precio = formatValue((decimal)(precio?.precio_mxn ?? 0), 6),
                    movimientoUnidad = formatValue((decimal)(flujo?.comision ?? 0), 6),
                    movimientoMxn = formatValue(((decimal)(flujo?.comision ?? 0) * (decimal)(precio?.precio_mxn ?? 0)), 2),
                    saldoUnidad = formatValue(unidades, 6),
                    tipoMovimiento = tipoMov,
                    saldoMxn = formatValue(unidades * (decimal)(precio?.precio_mxn ?? 0), 2)
                };

                result.Add(resultado);
            }
        }

        return result;
    }

    private decimal formatValue(decimal number, int decimals)
    {

        var result = Math.Round(number, decimals);
        return result;
    }


}

