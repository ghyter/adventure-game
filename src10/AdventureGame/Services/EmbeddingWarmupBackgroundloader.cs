using AdventureGame.Engine.Services;
using Microsoft.Extensions.Hosting;

namespace AdventureGame.Services;

public class EmbeddingWarmupBackgroundLoader : IHostedService
{
    private readonly EmbeddingService _embedding;

    public EmbeddingWarmupBackgroundLoader(EmbeddingService embedding)
    {
        _embedding = embedding;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Trigger background load without awaiting to avoid blocking app startup
            _embedding.StartLoadIfNeeded();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Embedding model failed to start loading:");
            Console.WriteLine(ex);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
