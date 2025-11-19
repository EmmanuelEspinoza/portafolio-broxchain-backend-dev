using FondoInversion.Models;

public interface IUserRepository: IBaseRepository<user>
{
    Task<user> GetUserByEmailAsync(string email);
}