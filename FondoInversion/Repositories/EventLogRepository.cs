using System.Linq.Expressions;
using FondoInversion.Data;
using FondoInversion.Models;
using Microsoft.EntityFrameworkCore;

public class EventLogRepository : BaseRepository<eventLog>, IEventLogRepository
{
    public EventLogRepository(AppDbContext context) : base(context){ }

}