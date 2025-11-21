using System.Text.Json;
using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class PreciosController : ControllerBase
{
    private readonly PrecioService _precioService;
    // private readonly ILogger<PreciosController> _logger;
    private readonly EventLogService _eventLogService;
    
    private readonly CurrentUserService _currentUser;

    public PreciosController(PrecioService precioService,  EventLogService eventLogService, CurrentUserService currentUser
    //  ILogger<PreciosController> logger
     )
    {
        // _logger = logger;
        _precioService = precioService;
        _eventLogService = eventLogService;
        _currentUser = currentUser;
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
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, id.ToString());
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
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(createPrecio));
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
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(precioUpdate));
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
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, id.ToString());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [JwtAuthorize] 
    [HttpPost("saldos")]
    public async Task<ActionResult<List<SaldosDtos>>> SaldosByUser(GetSaldoDto getSaldo)
    {
        try
        {
            var userId = int.Parse( _currentUser.GetUserId());
            var result = await _precioService.getSaldos((int)getSaldo.tipo, userId);
            return result;
        }
        catch (Exception ex)
        {

            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(getSaldo));
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [JwtAuthorize] 
    [HttpGet("valoresMercado")]
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
            return StatusCode(500, "Error al obtener el valor de mercado");
        }
    }


    [JwtAuthorize] 
    [HttpGet("flujoPrecios")]
    public async Task<ActionResult<List<FlujoPrecio>>> GetFlujosPrecioByUser()
    {
        try
        {
            var userId = int.Parse( _currentUser.GetUserId());
            var result = await _precioService.GetFlujosPrecioByUser(userId);
            return result;
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, "");
            return StatusCode(500, "Error al obtener los saldos y movimientos del usuario");
        }
    }
}