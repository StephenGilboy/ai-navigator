using Microsoft.Extensions.Hosting;

namespace AiNavigator.BrowserAgent;

public class WorkerService(BrowserAgent agent) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var response = await agent.DoStuff("What are the top 10 stories on Hacker News?", cancellationToken);
        Console.WriteLine(response);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Finished");
        return Task.CompletedTask;
    }
}