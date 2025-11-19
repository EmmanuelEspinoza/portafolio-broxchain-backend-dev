using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FlujosController : ControllerBase
{
    private readonly FlujoService _flujoService;
    private readonly ILogger<FlujosController> _logger;
    public FlujosController(FlujoService flujoService, ILogger<FlujosController> logger)
    {
        _logger = logger;
        _flujoService = flujoService;
    }

    [JwtAuthorize] 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<flujo>>> GetFlujos()
    {
        var flujos = await _flujoService.GetAllFlujosAsync();
        return Ok(flujos);
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
            _logger.LogError(ex, "Error al obtener el flujo con ID: {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    
    // POST: api/Flujos
    [JwtAuthorize] 
    [HttpPost]
    public async Task<ActionResult<flujo>> PostUser(CreateFlujoDto createFlujo)
    {
        try
        {
            var flujo = await _flujoService.CreateFlujoAsync(createFlujo);

            return CreatedAtAction("GetUser", new { id = flujo.id }, flujo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nuevo Flujo");
            return StatusCode(500, "Error interno del servidor");
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
            _logger.LogError(ex, "Error al actualizar el flujo con ID: {Id}", id);
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
            _logger.LogError(ex, "Error al eliminar flujo con ID: {Id}", id);
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
            _logger.LogError(ex, "Error al obtener el flujo con ID: {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}