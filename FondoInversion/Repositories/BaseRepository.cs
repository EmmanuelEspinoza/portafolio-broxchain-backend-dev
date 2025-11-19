// Repositories/Repository.cs
using FondoInversion.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.SingleOrDefaultAsync(predicate);

    public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Remove(T entity) => _dbSet.Remove(entity);

    public virtual async Task<bool> SaveAsync() => await _context.SaveChangesAsync() > 0;
}