using System.Text.Json;
using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FlujosController : ControllerBase
{
    private readonly FlujoService _flujoService;
    // private readonly ILogger<FlujosController> _logger;
    private readonly EventLogService _eventLogService;
    public FlujosController(FlujoService flujoService,  EventLogService eventLogService 
    // ILogger<FlujosController> logger,
    )
    {
        // _logger = logger;
        _flujoService = flujoService;
        _eventLogService = eventLogService;
    }

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

    
    // POST: api/Flujos
    [JwtAuthorize] 
    [HttpPost]
    public async Task<ActionResult<flujo>> PostFLujo(CreateFlujoDto createFlujo)
    {
        try
        {
            var flujo = await _flujoService.CreateFlujoAsync(createFlujo);

            return CreatedAtAction("GetUser", new { id = flujo.id }, flujo);
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(createFlujo));
            return StatusCode(500, "Error al subir el flujo");
        }
    }

    // PUT: api/Flujos/5
    [JwtAuthorize] 
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, EditFlujoDto flujoUpdate)
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

    // DELETE: api/Flujos/5
    [JwtAuthorize] 
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _flujoService.DeleteFlujoAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Flujo eliminado con exito" });
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,id.ToString());
            return StatusCode(500, "Error interno del servidor");
        }
    }

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