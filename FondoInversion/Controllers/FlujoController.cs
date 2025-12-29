using System.ComponentModel;
using System.Text.Json;
using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FlujosController : ControllerBase
{
    private readonly FlujoService _flujoService;
    private readonly EventLogService _eventLogService;
    public FlujosController(FlujoService flujoService,  EventLogService eventLogService 
    )
    {
        _flujoService = flujoService;
        _eventLogService = eventLogService;
    }

    [EndpointSummary("Obtiene el listado de flujos")]
    [EndpointDescription("Un servicio que obtiene un listado de todos los flujos que existen")]
    [ProducesResponseType<IEnumerable<flujo>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "application/json")]
    [JwtAuthorize] 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<flujo>>> GetFlujos()
    {
        try
        {
            var flujos = await _flujoService.GetAllFlujosAsync();
            return Ok(flujos);
            
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,"");
            
            return StatusCode(500, "Error al ");
        }
    }

    
    [EndpointSummary("Obtener flujo por ID")]
    [EndpointDescription("Servicio para obtener los datos de un flujo por su ID")]
    [ProducesResponseType<flujo>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpGet("{id}")]
    public async Task<ActionResult<flujo>> GetFlujo(int id)
    {
        try
        {
            var flujo = await _flujoService.GetFlujoByIdAsync(id);

            if (flujo == null)
                return NotFound();

            return flujo;
        }
        catch (Exception ex)
        {            
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(id.ToString()));
            return StatusCode(500, "Error interno del servidor");
        }
    }

    
    [EndpointSummary("Subir un nuevo flujo")]
    [EndpointDescription("Servicio para guardar un nuevo registro de flujo")]
    [ProducesResponseType<flujo>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpPost]
    public async Task<ActionResult<flujo>> PostFLujo(
        [Description("Modelo para crear un nuevo flujo ")]CreateFlujoDto createFlujo)
    {
        try
        {
            var flujo = await _flujoService.CreateFlujoAsync(createFlujo);
            return CreatedAtAction("GetFlujo", new { id = flujo.id }, flujo);
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(createFlujo));
            return StatusCode(500, "Error al subir el flujo");
        }
    }
    
    [EndpointSummary("Actualizar flujo")]
    [EndpointDescription("Servicio para actualizar los datos de un flujo por su ID")]
    [ProducesResponseType<object>(StatusCodes.Status204NoContent, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFlujo(int id, EditFlujoDto flujoUpdate)
    {
        try
        {
            flujoUpdate.Id = id;
            await _flujoService.EditFlujoAsync(flujoUpdate);
            return NoContent();
        }
        catch (Exception ex)
        {           
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(flujoUpdate));
            return StatusCode(500, "Error interno del servidor");
        }
    }

    
    // [EndpointSummary("Borrar flujo")]
    // [EndpointDescription("Servicio para actualizar los datos de un flujo por su ID")]
    // [ProducesResponseType<AnyType>(StatusCodes.Status204NoContent, "application/json")]
    // [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    // [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    // [JwtAuthorize] 
    // [HttpDelete("{id}")]
    // public async Task<IActionResult> DeleteUser(int id)
    // {
    //     try
    //     {
    //         var result = await _flujoService.DeleteFlujoAsync(id);
    //         if (!result) return NotFound();
    //         return Ok(new { message = "Flujo eliminado con exito" });
    //     }
    //     catch (Exception ex)
    //     {
    //         var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
    //         await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,id.ToString());
    //         return StatusCode(500, "Error interno del servidor");
    //     }
    // }

    
    [EndpointSummary("Obtener flujos por usuario")]
    [EndpointDescription("Servicio para obtener los flujos asociados a un usuario(inversor)")]
    [ProducesResponseType<List<flujo>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize] 
    [HttpGet("user/{id}")]
    public async Task<ActionResult<List<flujo>>> GetFlujoByUserId(int id)
    {
        try
        {
            var flujos = await _flujoService.GetFlujosByUserID(id);

            if (flujos == null)
                return NotFound();

            return flujos;
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,id.ToString());
            return StatusCode(500, "Error interno del servidor");
        }
    }
}