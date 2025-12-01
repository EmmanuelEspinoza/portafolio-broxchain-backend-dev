using FondoInversion.Models;

public class EventLogService
{
    private readonly IEventLogRepository _eventLogRepository;
    private readonly IConfiguration _configuration;
    public EventLogService(IConfiguration configuration, IEventLogRepository eventLogRepository, ILogger<EventLogService> logger)
    {
        _eventLogRepository = eventLogRepository;
        _configuration = configuration;
    }


    public async Task<bool> SaveEventLog(string message, ETipoMessage severity, string body)
    {

        try
        {
            var lvlLog = int.Parse(_configuration["LogError:Level"]) | 4;
            if ((int)severity < lvlLog) return true;
            var logMessage = new eventLog
            {
                fecha = DateTime.UtcNow,
                message = message,
                type = (int)severity,
                body = body
            };

            await _eventLogRepository.AddAsync(logMessage);
            await _eventLogRepository.SaveAsync();
            return true;

        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
            return false;
        }

    }


}




