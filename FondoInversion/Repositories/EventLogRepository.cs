using FondoInversion.Data;
using FondoInversion.Models;

public class EventLogRepository : BaseRepository<eventLog>, IEventLogRepository
{
    public EventLogRepository(AppDbContext context) : base(context){ }

}