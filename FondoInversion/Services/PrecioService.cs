using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FondoInversion.Models;

public class PrecioService
{
    private readonly IPrecioRepository _precioRepository;
    private readonly FlujoService _flujoService;

    public PrecioService(IPrecioRepository precioRepository, FlujoService flujoService)
    {
        _precioRepository = precioRepository;
        _flujoService = flujoService;
    }

    public async Task<precio> CreatePrecioAsync(CreatePrecioDto createPrecioDto)
    {
        var precio = new precio
        {
            fecha_precio = createPrecioDto.FechaPrecio,
            precio_mxn = createPrecioDto.PrecioMxn,
        };
        await _precioRepository.AddAsync(precio);
        await _precioRepository.SaveAsync();

        return precio;
    }

    public async Task<precio> EditPrecioAsync(EditPrecioDto editPrecioDto)
    {
        var precio = await _precioRepository.GetByIdAsync(editPrecioDto.Id);

        if (precio == null)
        {
            throw new Exception("Precio no encontrado");
        }

        precio.fecha_precio = editPrecioDto.FechaPrecio;
        precio.precio_mxn = editPrecioDto.PrecioMxn;

        _precioRepository.Update(precio);
        await _precioRepository.SaveAsync();

        return precio;
    }

    public async Task<precio> GetPrecioByIdAsync(int id)
        => await _precioRepository.GetByIdAsync(id);

    public async Task<IEnumerable<precio>> GetAllPreciosAsync()
        => await _precioRepository.GetAllAsync();

    public async Task<bool> DeletePrecioAsync(int id)
    {
        var precio = await _precioRepository.GetByIdAsync(id);
        if (precio == null) return false;

        _precioRepository.Remove(precio);
        return await _precioRepository.SaveAsync();
    }


    public async Task<bool> GuardarPrecios(List<precio> precios)
    {
        try
        {
            var lastDate = await _precioRepository.GetLastDate();
            var result = precios.Where(x => x.fecha_precio > lastDate);
            await _precioRepository.BulkUpsertAsync(result.ToList());
            return true;
        }
        catch (Exception ex)
        {
            // Log del error
            Console.WriteLine($"Error al guardar los precios: {ex.Message}");
            return false;
        }
    }

    public async Task<ValoresMercado> GetValoresMercado()
    {

        var movimientos = await _flujoService.GetAllFlujosAsync();
        var monto = (decimal)movimientos.Sum(m => m.importe + m.comision);
        var precioDia = await _precioRepository.GetLastPrice();
        var precio = precioDia?.precio_mxn ?? 0;
        var valorMerc = monto * (decimal)precio;

        var result = new ValoresMercado()
        {
            montoCirculacion = getPriceDecimal(monto, 0, 6),
            valorMercado = getPriceDecimal(valorMerc, 0, 2)
        };

        return result;

    }

    public async Task<List<precio>> ProcesarPrecioCSV(Stream stream)
    {
        var precios = new List<precio>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
            // Eliminamos ShouldSkipRecord ya que no funciona como esperábamos
        };

        using (var reader = new StreamReader(stream))

        using (var csv = new CsvReader(reader, config))
        {
            // Configurar el mapeo
            csv.Context.RegisterClassMap<PrecioMap>();

            // Leer todos los registros y filtrar después
            var records = csv.GetRecords<PrecioCsv>();

            foreach (var record in records)
            {
                try
                {
                    // Validar y limpiar cada registro individualmente
                    if (EsRegistroValido(record))
                    {
                        precios.Add(new precio
                        {
                            fecha_precio = DateOnly.ParseExact(record.FechaPrecio, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            precio_mxn = double.Parse(record.PrecioMxn)
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Opcional: registrar el error pero continuar procesando
                    Console.WriteLine($"Error procesando registro: {ex.Message}");
                    continue;
                }
            }
        }

        return precios;
    }

    // Método auxiliar para validar registros
    private bool EsRegistroValido(PrecioCsv record)
    {
        if (record == null) return false;

        // Validar FechaPrecio
        if (string.IsNullOrWhiteSpace(record.FechaPrecio) ||
            record.FechaPrecio.Contains("#N/A") ||
            record.FechaPrecio.Contains("ERROR"))
            return false;

        // Validar PrecioMxn
        if (string.IsNullOrWhiteSpace(record.PrecioMxn) ||
            record.PrecioMxn.Contains("#N/A") ||
            record.PrecioMxn.Contains("ERROR"))
            return false;

        // Validar formato de fecha
        if (!DateTime.TryParseExact(record.FechaPrecio.Trim(), "dd/MM/yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            return false;

        // Validar formato de precio
        if (!double.TryParse(record.PrecioMxn.Trim(), NumberStyles.Any,
            CultureInfo.InvariantCulture, out double precio) || precio < 0)
            return false;

        return true;
    }

    public async Task<List<SaldosDtos>> getSaldos(int tipo, int userID = 0)
    {
        if (tipo == (int)TipoSaldo.General)
            return await GetSaldoGeneric();
        
        return await GetSaldoByUser(userID);
    }

    private async Task<List<SaldosDtos>> GetSaldoByUser(int userId)
    {

        var result = new List<SaldosDtos>();

        var precioAlDia = await _precioRepository.GetLastPrice();
        var preciosAyer = await _precioRepository.GetByLimitDescent(2);
        var preciosSemana = await _precioRepository.GetByLimitDescent(6);
        var preciosMensual = await _precioRepository.GetByLimitDescent(22);

        var movimientos = await _flujoService.GetFlujosByUserID(userId);

        var saldoUnidades = (decimal)movimientos.Sum(m => m.importe + m.comision);

        var saldoAlDia = saldoUnidades * (decimal)(precioAlDia?.precio_mxn ?? 0);
        var saldoAyer = saldoUnidades * (decimal)(preciosAyer.LastOrDefault()?.precio_mxn ?? 0);
        var saldoSemana = saldoUnidades * (decimal)(preciosSemana.LastOrDefault()?.precio_mxn ?? 0);
        var saldoMes = saldoUnidades * (decimal)(preciosMensual.LastOrDefault()?.precio_mxn ?? 0);

        result.Add(new SaldosDtos()
        {
            valorPrincipal = getPriceDecimal(saldoAlDia, 0, 2),
            valorSecundario = getPriceDecimal(saldoUnidades, 0, 2),
            titulo = "Inversión al día"
        });

        result.Add(new SaldosDtos()
        {
            valorPrincipal = getPriceDecimal(saldoAlDia, saldoAyer, 2),
            valorSecundario = getPercentDecimal(saldoAlDia, saldoAyer, 2),
            valorTerciario = getPriceDecimal(saldoAyer, 0, 2),
            titulo = "Variación al día anterior",
            color = (getPriceDecimal(saldoAlDia, saldoAyer, 2) > 0) ? "text-green-600" : "text-red-600",
        });

        result.Add(new SaldosDtos()
        {
            valorPrincipal = getPriceDecimal(saldoAlDia, saldoSemana, 2),
            valorSecundario = getPercentDecimal(saldoAlDia, saldoSemana, 2),
            valorTerciario = getPriceDecimal(saldoSemana, 0, 2),
            titulo = "Variación a una semana anterior",
            color = (getPriceDecimal(saldoAlDia, saldoSemana, 2) > 0) ? "text-green-600" : "text-red-600",
        });

        result.Add(new SaldosDtos()
        {
            valorPrincipal = getPriceDecimal(saldoAlDia, saldoMes, 2),
            valorSecundario = getPercentDecimal(saldoAlDia, saldoMes, 2),
            valorTerciario = getPriceDecimal(saldoMes, 0, 2),
            titulo = "Variación al mes anterior",
            color = (getPriceDecimal(saldoAlDia, saldoMes, 2) > 0) ? "text-green-600" : "text-red-600",
        });


        return result;

    }

    private async Task<List<SaldosDtos>> GetSaldoGeneric()
    {
        var saldos = new List<SaldosDtos>();

        var precioAlDia = await _precioRepository.GetLastPrice();
        var preciosAyer = await _precioRepository.GetByLimitDescent(2);
        var preciosSemana = await _precioRepository.GetByLimitDescent(6);
        var preciosMensual = await _precioRepository.GetByLimitDescent(22);

        if (precioAlDia is null) throw new Exception("No existen precios cargados");

        saldos.Add(new SaldosDtos()
        {
            valorPrincipal = (decimal)precioAlDia.precio_mxn,
            titulo = "Precio",
            textoSecundario = "Último precio",
            tooltip = "Último precio en MXN"
        });

        saldos.Add(new SaldosDtos()
        {
            valorPrincipal = getPriceDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosAyer.LastOrDefault()?.precio_mxn ?? 0), 6),
            valorSecundario = getPercentDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosAyer.LastOrDefault()?.precio_mxn ?? 0), 2),
            titulo = "Variación al día",
            tooltip = "Variación del precio en un día en MXN",
            color = (getPriceDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosAyer.LastOrDefault()?.precio_mxn ?? 0), 6) > 0) ? "text-green-600" : "text-red-600",
        });

        saldos.Add(new SaldosDtos()
        {
            valorPrincipal = getPriceDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosSemana.LastOrDefault()?.precio_mxn ?? 0), 6),
            valorSecundario = getPercentDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosSemana.LastOrDefault()?.precio_mxn ?? 0), 2),
            titulo = "Variación a la semana",
            tooltip = "Variación del precio en una semana en MXN",
            color = (getPriceDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosSemana.LastOrDefault()?.precio_mxn ?? 0), 6) > 0) ? "text-green-600" : "text-red-600",
        });

        saldos.Add(new SaldosDtos()
        {
            valorPrincipal = getPriceDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosMensual.LastOrDefault()?.precio_mxn ?? 0), 6),
            valorSecundario = getPercentDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosMensual.LastOrDefault()?.precio_mxn ?? 0), 2),
            titulo = "Variación al mes",
            tooltip = "Variación del precio en un mes MXN",
            color = (getPriceDecimal((decimal)precioAlDia.precio_mxn, (decimal)(preciosMensual.LastOrDefault()?.precio_mxn ?? 0), 6) > 0) ? "text-green-600" : "text-red-600",
        });

        return saldos;

    }

    private decimal getPriceDecimal(decimal precioDia, decimal precioAnterior, int decimals)
    {
        var price = precioDia - precioAnterior;
        price = Math.Round(price, decimals);
        return price;

    }

    private decimal getPercentDecimal(decimal precioDia, decimal precioAnterior, int decimals)
    {
        var price = precioDia - precioAnterior;
        var percent = (price / precioAnterior) * 100;
        percent = Math.Round(percent, decimals);
        return percent;

    }

    public async Task<List<FlujoPrecio>> GetFlujosPrecioByUser(int userID)
    {
        var result = await _precioRepository.GetFlujoPrecio(userID);
        return result;
    }

}


public class PrecioCsv
{
    public string FechaPrecio { get; set; } = "01/01/1990";
    public string PrecioMxn { get; set; } = "";
}

public sealed class PrecioMap : ClassMap<PrecioCsv>
{
    public PrecioMap()
    {
        Map(m => m.FechaPrecio).Name("FECHA");
        Map(m => m.PrecioMxn).Name("PRECIO");
    }
}

