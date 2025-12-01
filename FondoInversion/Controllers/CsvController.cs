using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CSVController : ControllerBase
{
    private readonly ICSVService _csvService;
    private readonly PrecioService _precioService;
    private readonly FlujoService _flujoService;
    private readonly EventLogService _eventLogService;

    public CSVController(ICSVService csvService, PrecioService precioService, FlujoService flujoService, EventLogService eventLogService
    )
    {
        _csvService = csvService;
        _precioService = precioService;
        _flujoService = flujoService;
        _eventLogService = eventLogService;
    }

    
    [EndpointSummary("Subir precios")]
    [EndpointDescription("Servicio para subir los precios desde un csv en el cual debe estructurarse de la siguiente manera: 'FECHA(formato dd/MM/YYYY), PRECIO(decimal)'")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpPost("uploadPrecios")]
    public async Task<IActionResult> UploadCSV(
        [Description("Archivo CSV con el listado de precios a subir ")]IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha enviado ningún archivo");
        }

        if (Path.GetExtension(file.FileName).ToLower() != ".csv")
        {
            return BadRequest("Solo se permiten archivos CSV");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
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


    [EndpointSummary("Subir flujos")]
    [EndpointDescription("Servicio para subir los flujos de los movimientos de los usuarios, usa un csv con la siguiente estructura: 'ID(int), FECHA(formato dd/MM/YYYY), INVERSIONISTA(id del inversionista), FLUJO(movimiento en unidades del fondo 'decimal'), COMISION(movimiento en unidades del fondo que representa la comisión)'")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpPost("uploadFlujos")]
    public async Task<IActionResult> UploadFlujoCSV(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha enviado ningún archivo");
        }
        if (Path.GetExtension(file.FileName).ToLower() != ".csv")
        {
            return BadRequest("Solo se permiten archivos CSV");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
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