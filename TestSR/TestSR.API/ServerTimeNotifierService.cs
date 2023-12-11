
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace TestSR.API;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class ServerTimeNotifierService : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(1);
    private readonly ILogger<ServerTimeNotifierService> _logger;
    private readonly IHubContext<ChatHub, IChatClient> _context;

    public ServerTimeNotifierService(ILogger<ServerTimeNotifierService> logger, IHubContext<ChatHub, IChatClient> context)
    {
        _logger = logger;
        _context = context;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var dateTime = DateTime.Now;
            //_logger.LogInformation("Executing {Service} {Time}", nameof(ServerTimeNotifierService), dateTime);
            await _context.Clients.All.UpdateTime(dateTime);

        }
    }

    private string GetDebuggerDisplay()
    {
        return ToString() ?? string.Empty;
    }
}
