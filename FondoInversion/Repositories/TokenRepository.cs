using System.Linq.Expressions;
using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.EntityFrameworkCore;

public class TokenRepository : BaseRepository<token>, ITokenRepository
{
    public TokenRepository(AppDbContext context) : base(context){ }

    public async Task<token?> GetDataByRefreshToken(string refreshToken)
    {
        return await _dbSet.FirstOrDefaultAsync(token => token.refreshToken == refreshToken);

    }
}