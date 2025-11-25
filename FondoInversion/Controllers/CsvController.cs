using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CSVController : ControllerBase
{
    private readonly ICSVService _csvService;
    private readonly PrecioService _precioService;
    private readonly FlujoService _flujoService;
    // private readonly ILogger<CSVController> _logger;
    private readonly EventLogService _eventLogService;

    public CSVController(ICSVService csvService, PrecioService precioService, FlujoService flujoService, EventLogService eventLogService
    // ILogger<CSVController> logger, 
    )
    {
        _csvService = csvService;
        // _logger = logger;
        _precioService = precioService;
        _flujoService = flujoService;
        _eventLogService = eventLogService;
    }

    [JwtAuthorize] 
    [HttpPost("uploadPrecios")]
    public async Task<IActionResult> UploadCSV(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha enviado ningún archivo");
        }

        // Validar que sea un archivo CSV
        if (Path.GetExtension(file.FileName).ToLower() != ".csv")
        {
            return BadRequest("Solo se permiten archivos CSV");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                // Procesar el CSV
                var resultado = await _csvService.ProcesarPrecioCSV(stream);

                if (resultado.Count == 0)
                {
                    return BadRequest("El archivo CSV está vacío o no tiene el formato correcto");
                }
                else if (resultado.Count > 0)
                {
                    await _precioService.GuardarPrecios(resultado);
                    return Ok(new
                    {
                        message = "Archivo procesado correctamente",
                        registros = resultado.Count
                    });
                }

                return StatusCode(500, "Error al guardar los datos en la base de datos");
            }
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,file.Name);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [JwtAuthorize] 
    [HttpPost("uploadFlujos")]
    public async Task<IActionResult> UploadFlujoCSV(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha enviado ningún archivo");
        }

        // Validar que sea un archivo CSV
        if (Path.GetExtension(file.FileName).ToLower() != ".csv")
        {
            return BadRequest("Solo se permiten archivos CSV");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                // Procesar el CSV
                var resultado = await _csvService.ProcesaFlujoCSV(stream);

                if (resultado.Count == 0)
                {
                    return BadRequest("El archivo CSV está vacío o no tiene el formato correcto");
                }
                else if (resultado.Count > 0)
                {
                    await _flujoService.GuardarFlujos(resultado);
                    return Ok(new
                    {
                        message = "Archivo procesado correctamente",
                        registros = resultado.Count
                    });
                }

                return StatusCode(500, "Error al guardar los datos en la base de datos");
            }
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,file.Name);
            
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

}