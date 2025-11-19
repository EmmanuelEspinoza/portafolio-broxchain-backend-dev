using FondoInversion.Models;

public interface IAdminUserRepository: IBaseRepository<adminUser>
{
    Task<adminUser> GetUserByEmailAsync(string email);
}