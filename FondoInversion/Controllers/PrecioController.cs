using System.ComponentModel;
using System.Text.Json;
using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class PreciosController : ControllerBase
{
    private readonly PrecioService _precioService;
    private readonly EventLogService _eventLogService;
    
    private readonly CurrentUserService _currentUser;

    public PreciosController(PrecioService precioService,  EventLogService eventLogService, CurrentUserService currentUser
    )
    {
        _precioService = precioService;
        _eventLogService = eventLogService;
        _currentUser = currentUser;
    }

    
    [EndpointSummary("Obtener listado de precios")]
    [EndpointDescription("Servicio el listado de precios")]
    [ProducesResponseType<IEnumerable<precio>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<precio>>> GetPrecios()
    {
        var precios = await _precioService.GetAllPreciosAsync();
        return Ok(precios);
    }

    
    [EndpointSummary("Obtener precio por su ID ")]
    [EndpointDescription("Obtiene el precio por su identificador ")]
    [ProducesResponseType<precio>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpGet("{id}")]
    public async Task<ActionResult<precio>> GetPrecio(
        [Description("Identificador del precio")]int id)
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


    
    [EndpointSummary("Guardar un nuevo precio ")]
    [EndpointDescription("Servicio para guardar un nuevo precio  ")]
    [ProducesResponseType<precio>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpPost]
    public async Task<ActionResult<precio>> PostPrecio(
        [Description("Modelo para crear un nuevo precio")]CreatePrecioDto createPrecio)
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


    [EndpointSummary("Actializar precio")]
    [EndpointDescription("Servicio para actualizar el precio por su ID")]
    [ProducesResponseType<string>(StatusCodes.Status204NoContent, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPrecio(
        [Description("Identificador del precio")]int id, 
        [Description("Modelo para actualizar el precio")]EditPrecioDto precioUpdate)
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

    
    // [EndpointSummary("Eliminar precio")]
    // [EndpointDescription("Servicio para eliminar el precio por su ID")]
    // [ProducesResponseType<string>(StatusCodes.Status200OK, "application/json")]
    // [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    // [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    // [JwtAuthorize] 
    // [HttpDelete("{id}")]
    // public async Task<IActionResult> DeletePrecio(int id)
    // {
    //     try
    //     {
    //         var result = await _precioService.DeletePrecioAsync(id);
    //         if (!result) return NotFound();
    //         return Ok(new { message = "Precio eliminado con exito" });
    //     }
    //     catch (Exception ex)
    //     {
    //         var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
    //         await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, id.ToString());
    //         return StatusCode(500, "Error interno del servidor");
    //     }
    // }


    [EndpointSummary("Obtener precios")]
    [EndpointDescription("Servicio para obtener el listado de saldos para un usuario o el general")]
    [ProducesResponseType<List<SaldosDtos>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
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


    [EndpointSummary("Obtener los valores del mercado ")]
    [EndpointDescription("Servicio que obtiene los valores para el fondo de inversión, unidades en el mercado y su valor en moneda MXN")]
    [ProducesResponseType<ValoresMercado>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
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


    [EndpointSummary("Obtener los flujos por usuario")]
    [EndpointDescription("Servicio que obtiene el listado de flujos del usuario junto a sus cálculos de cada fecha.")]
    [ProducesResponseType<List<FlujoPrecio>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
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