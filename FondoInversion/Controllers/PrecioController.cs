using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class PreciosController : ControllerBase
{
    private readonly PrecioService _precioService;
    private readonly ILogger<PreciosController> _logger;
    private readonly EventLogService _eventLogService;

    public PreciosController(PrecioService precioService, ILogger<PreciosController> logger, EventLogService eventLogService)
    {
        _logger = logger;
        _precioService = precioService;
        _eventLogService = eventLogService;
    }

    [JwtAuthorize] 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<precio>>> GetPrecios()
    {
        var precios = await _precioService.GetAllPreciosAsync();
        return Ok(precios);
    }

    [JwtAuthorize] 
    [HttpGet("{id}")]
    public async Task<ActionResult<precio>> GetPrecio(int id)
    {
        try
        {
            var precio = await _precioService.GetPrecioByIdAsync(id);
            if (precio == null)
                return NotFound();
            return precio;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Precio con ID: {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/Precios
    [JwtAuthorize] 
    [HttpPost]
    public async Task<ActionResult<precio>> PostPrecio(CreatePrecioDto createPrecio)
    {
        try
        {
            var precio = await _precioService.CreatePrecioAsync(createPrecio);
            return CreatedAtAction("GetPrecio", new { id = precio.id }, precio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nuevo Precio");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/Precios/5
    [JwtAuthorize] 
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPrecio(int id, EditPrecioDto precioUpdate)
    {
        try
        {
            precioUpdate.Id = id;
            await _precioService.EditPrecioAsync(precioUpdate);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar Precio con ID: {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/Precios/5
    [JwtAuthorize] 
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePrecio(int id)
    {
        try
        {
            var result = await _precioService.DeletePrecioAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Precio eliminado con exito" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar Precio con ID: {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [JwtAuthorize] 
    [HttpPost("saldos")]
    public async Task<ActionResult<List<SaldosDtos>>> SaldosByUser(GetSaldoDto getSaldo)
    {
        try
        {
            var result = await _precioService.getSaldos(getSaldo);
            return result;
        }
        catch (Exception ex)
        {

            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, "");
            _logger.LogError(ex, "Error al realizar los calculos ");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [JwtAuthorize] 
    [HttpGet("saldos")]
    public async Task<ActionResult<ValoresMercado>> GetValoresMercado()
    {
        try
        {
            var result = await _precioService.GetValoresMercado();
            return result;
        }
        catch (Exception ex)
        {
            await _eventLogService.SaveEventLog(ex.Message, ETipoMessage.Error, "");
            _logger.LogError(ex, "Error al obtener los valores del mercado");
            return StatusCode(500, "Error al obtener el valor de mercado");
        }
    }


    [JwtAuthorize] 
    [HttpGet("flujoPrecios/{id}")]
    public async Task<ActionResult<List<FlujoPrecio>>> GetFlujosPrecioByUser(int id)
    {
        try
        {
            var result = await _precioService.GetFlujosPrecioByUser(id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los saldos y movimientos del usuario");
            return StatusCode(500, "Error al obtener los saldos y movimientos del usuario");
        }
    }
}