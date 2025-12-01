using FondoInversion.Data;
using FondoInversion.Models;

public class CSVService : ICSVService
{
    private readonly PrecioService _precioService;
    private readonly FlujoService _flujoService;

    public CSVService(PrecioService precioService, FlujoService flujoService)
    {
        _precioService = precioService;
        _flujoService = flujoService;
    }

    public async Task<List<flujo>> ProcesaFlujoCSV(Stream stream)
    {
        return await _flujoService.ProcesarFlujoCSV(stream);
    }

    public async Task<List<precio>> ProcesarPrecioCSV(Stream stream)
    {
        return await _precioService.ProcesarPrecioCSV(stream);
    }

    


}




