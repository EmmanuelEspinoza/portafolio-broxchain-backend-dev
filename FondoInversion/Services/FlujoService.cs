using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FondoInversion.Models;

public class FlujoService
{
    private readonly IFlujoRepository _flujoRepository;

    public FlujoService(IFlujoRepository flujoRepository)
    {
        _flujoRepository = flujoRepository;
    }

    public async Task<flujo> CreateFlujoAsync(CreateFlujoDto createFlujoDto)
    {
        var flujo = new flujo
        {
            inversionista_id = createFlujoDto.InversionistaId,
            dia_movimiento = createFlujoDto.DiaMovimiento,
            importe = createFlujoDto.Importe,
            comision = createFlujoDto.Comision
        };
        await _flujoRepository.AddAsync(flujo);
        await _flujoRepository.SaveAsync();

        return flujo;
    }

    public async Task<flujo> EditFlujoAsync(EditFlujoDto editFlujoDto)
    {
        var flujo = await _flujoRepository.GetByIdAsync(editFlujoDto.Id);

        if (flujo == null)
        {
            throw new Exception("Flujo no encontrado");
        }

        flujo.inversionista_id = editFlujoDto.InversionistaId;
        flujo.dia_movimiento = editFlujoDto.DiaMovimiento;
        flujo.importe = editFlujoDto.Importe;
        flujo.comision = editFlujoDto.Comision;


        _flujoRepository.Update(flujo);
        await _flujoRepository.SaveAsync();

        return flujo;
    }

    public async Task<flujo> GetFlujoByIdAsync(int id)
        => await _flujoRepository.GetByIdAsync(id);

    public async Task<IEnumerable<flujo>> GetAllFlujosAsync()
        => await _flujoRepository.GetAllAsync();

    public async Task<bool> DeleteFlujoAsync(int id)
    {
        var flujo = await _flujoRepository.GetByIdAsync(id);
        if (flujo == null) return false;

        _flujoRepository.Remove(flujo);
        return await _flujoRepository.SaveAsync();
    }

    public async Task<List<flujo>> ProcesarFlujoCSV(Stream stream)
    {
        var flujos = new List<flujo>();

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
            csv.Context.RegisterClassMap<FlujoMap>();

            // Leer todos los registros y filtrar después
            var records = csv.GetRecords<FlujoCsv>();

            foreach (var record in records)
            {
                try
                {
                    // Validar y limpiar cada registro individualmente
                    if (EsRegistroValido(record))
                    {
                        flujos.Add(new flujo
                        {
                            dia_movimiento = DateOnly.ParseExact(record.DiaMovimiento, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            inversionista_id = int.Parse(record.InversionistaId),
                            importe = double.Parse(record.Importe),
                            comision = double.Parse(record.Comision),
                            id = int.Parse(record.Id)
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

        return flujos;
    }

    // Método auxiliar para validar registros
    private bool EsRegistroValido(FlujoCsv record)
    {
        if (record == null) return false;

        // Validar DiaMovimiento
        if (string.IsNullOrWhiteSpace(record.DiaMovimiento) ||
            record.DiaMovimiento.Contains("#N/A") ||
            record.DiaMovimiento.Contains("ERROR"))
            return false;

        // Validar Importe
        if (string.IsNullOrWhiteSpace(record.Importe) ||
            record.Importe.Contains("#N/A") ||
            record.Importe.Contains("ERROR"))
            return false;

        // Validar Comision
        if (string.IsNullOrWhiteSpace(record.Comision) ||
            record.Comision.Contains("#N/A") ||
            record.Comision.Contains("ERROR"))
            return false;

        // Validar InversionistaId
        if (string.IsNullOrWhiteSpace(record.InversionistaId) ||
            record.InversionistaId.Contains("#N/A") ||
            record.InversionistaId.Contains("ERROR"))
            return false;

        // Validar Id
        if (string.IsNullOrWhiteSpace(record.Id) ||
            record.Id.Contains("#N/A") ||
            record.Id.Contains("ERROR"))
            return false;

        // Validar formato de fecha
        if (!DateTime.TryParseExact(record.DiaMovimiento.Trim(), "dd/MM/yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            return false;

        // Validar formato de precio
        if (!double.TryParse(record.Importe.Trim(), NumberStyles.Any,
            CultureInfo.InvariantCulture, out double Importe))
            return false;


        // Validar formato de precio
        if (!double.TryParse(record.Comision.Trim(), NumberStyles.Any,
            CultureInfo.InvariantCulture, out double Comision))
            return false;

        return true;
    }


    public async Task<bool> GuardarFlujos(List<flujo> flujos)
    {
        try
        {
            var lastId = await _flujoRepository.GetLastFlujo();
            var result = flujos.Where(x => x.id > lastId);
            await _flujoRepository.BulkUpsertAsync(result.ToList());

            return true;
        }
        catch (Exception ex)
        {
            // Log del error
            Console.WriteLine($"Error al guardar los flujos: {ex.Message}");
            return false;
        }
    }

    public async Task<List<flujo>> GetFlujosByUserID(int userId)
    {
        var flujos = await _flujoRepository.GetFlujoByUserID(userId);
        return flujos;
    }
}


public class FlujoCsv
{
    public string DiaMovimiento { get; set; } = "01/01/1990";
    public string Importe { get; set; } = "";
    public string Comision { get; set; } = "";
    public string InversionistaId { get; set; } = "";
    public string Id { get; set; } = "";

}

public sealed class FlujoMap : ClassMap<FlujoCsv>
{
    public FlujoMap()
    {
        Map(m => m.DiaMovimiento).Name("FECHA");
        Map(m => m.Importe).Name("FLUJO");
        Map(m => m.Comision).Name("COMISION");
        Map(m => m.InversionistaId).Name("INVERSIONISTA");
        Map(m => m.Id).Name("ID");
    }
}

