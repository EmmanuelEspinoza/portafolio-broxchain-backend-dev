using FondoInversion.Models;

public interface ICSVService
{
    Task<List<precio>> ProcesarPrecioCSV(Stream stream);
    Task<List<flujo>> ProcesaFlujoCSV(Stream stream);
}