using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [JwtAuthorize] 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<user>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

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
            _logger.LogError(ex, "Error al obtener User con ID: {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/Users
    [JwtAuthorize] 
    [HttpPost]
    public async Task<ActionResult<user>> PostUser(CreateUserDto createUser)
    {
        try
        {
            var user = await _userService.CreateUserAsync(createUser);
            return CreatedAtAction("GetUser", new { id = user.id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nuevo User");
            return StatusCode(500, "Error interno del servidor: ");
        }
    }

    // PUT: api/Users/5
    [JwtAuthorize] 
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, EditUserDto userUpdate)
    {
        try
        {
            userUpdate.Id = id;

            await _userService.EditUserAsync(userUpdate);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar User con ID: {Id}", id);
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