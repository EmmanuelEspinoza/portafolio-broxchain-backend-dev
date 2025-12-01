using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.EntityFrameworkCore;

public class UserRepository : BaseRepository<user>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) {}

    public async Task<user> GetUserByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.correo == email) ?? new user();
    }
    
}