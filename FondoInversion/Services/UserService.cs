using FondoInversion.Models;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<user> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Verificar si el email ya existe

        var existUser = await _userRepository.GetUserByEmailAsync(createUserDto.Correo);
        if (existUser.correo != null)
        {
            throw new Exception("El email ya está registrado");
        }

        var user = new user
        {
            name = createUserDto.Name,
            correo = createUserDto.Correo,
            telefono = createUserDto.Telefono,
            rfc = createUserDto.Rfc,
            curp = createUserDto.Curp
        };
        await _userRepository.AddAsync(user);
        await _userRepository.SaveAsync();

        return user;
    }

    public async Task<user> EditUserAsync(EditUserDto editUserDto)
    {
        var user = await _userRepository.GetByIdAsync(editUserDto.Id);

        if (user == null)
        {
            throw new Exception("Usuario no encontrado");
        }

        user.name = editUserDto.Name ?? user.name; 
        user.correo = editUserDto.Correo ?? user.correo;
        user.telefono = editUserDto.Telefono ?? user.telefono;
        user.rfc = editUserDto.Rfc ?? user.rfc;
        user.curp = editUserDto.Curp ?? user.curp;
        

        _userRepository.Update(user);
        await _userRepository.SaveAsync();

        return user;
    }

    public async Task<user> GetUserByIdAsync(int id)
        => await _userRepository.GetByIdAsync(id);

    public async Task<IEnumerable<user>> GetAllUsersAsync()
        => await _userRepository.GetAllAsync();

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        _userRepository.Remove(user);
        return await _userRepository.SaveAsync();
    }

}