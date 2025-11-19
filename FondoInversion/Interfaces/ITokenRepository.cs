using FondoInversion.Models;

public interface ITokenRepository: IBaseRepository<token>
{
    Task<token?> GetDataByRefreshToken(string refreshToken);
}