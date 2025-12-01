using System.ComponentModel;
using System.Text.Json;
using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly EventLogService _eventLogService;


    public UsersController(UserService userService, EventLogService eventLogService
    )
    {
        _userService = userService;
        _eventLogService = eventLogService;
    }


    [EndpointSummary("Obtener listado de usuarios")]
    [EndpointDescription("Servicio para obtener el listado de usuarios")]
    [ProducesResponseType<IEnumerable<user>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<user>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, "");
            return StatusCode(500, "Error al obtener la información");
        }
    }


    [EndpointSummary("Obtener usuario por ID")]
    [EndpointDescription("Servicio para obtener un usuario por su ID")]
    [ProducesResponseType<user>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<user>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, id.ToString());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/Users
    
    [EndpointSummary("Guardar nuevo usuario")]
    [EndpointDescription("Servicio para guardar un nuevo usuario")]
    [ProducesResponseType<user>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize]
    [HttpPost]
    public async Task<ActionResult<user>> PostUser(
        [Description("Modelo para crear un nuevo usuario")]CreateUserDto createUser)
    {
        try
        {
            var user = await _userService.CreateUserAsync(createUser);
            return CreatedAtAction("GetUser", new { id = user.id }, user);
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(createUser));
            return StatusCode(500, "Error interno del servidor: ");
        }
    }


    [EndpointSummary("Actualizar usuario usuario")]
    [EndpointDescription("Servicio para actualizar un usuario")]
    [ProducesResponseType<string>(StatusCodes.Status204NoContent, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError, "application/json")]
    [JwtAuthorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(
        [Description("Id del suario a actualizar")]int id, 
        [Description("Modelo del usuario a editar ")]EditUserDto userUpdate)
    {
        try
        {
            userUpdate.Id = id;

            await _userService.EditUserAsync(userUpdate);

            return NoContent();
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(userUpdate));
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/Users/5
    // [HttpDelete("{id}")]
    // public async Task<IActionResult> DeleteUser(int id)
    // {
    //     try
    //     {
    //         var result = await _userService.DeleteUserAsync(id);
    //         if (!result) return NotFound();
    //         return Ok(new { message = "Usuario eliminado exitosamente" });
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error al eliminar User con ID: {Id}", id);
    //         return StatusCode(500, "Error interno del servidor");
    //     }
    // }
}