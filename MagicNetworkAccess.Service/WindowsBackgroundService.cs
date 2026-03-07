using MagicNetworkAccess.Library.Core;
using Microsoft.Extensions.Hosting;

namespace MagicNetworkAccess.Service;

public sealed class WindowsBackgroundService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;

    public WindowsBackgroundService(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!SystemCore.Instance.Start())
        {
            _lifetime.StopApplication();
            return Task.CompletedTask;
        }

        stoppingToken.Register(() => SystemCore.Instance.Stop());
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        SystemCore.Instance.Stop();
        return base.StopAsync(cancellationToken);
    }
}
