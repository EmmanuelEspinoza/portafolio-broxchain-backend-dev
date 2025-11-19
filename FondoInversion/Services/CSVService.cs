
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using FondoInversion.Data;
using FondoInversion.Models;
using Humanizer;
using System.Linq;

public class CSVService : ICSVService
{
    private readonly AppDbContext _context;
    private readonly PrecioService _precioService;
    private readonly FlujoService _flujoService;

    public CSVService(AppDbContext context, PrecioService precioService, FlujoService flujoService)
    {
        _context = context;
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




