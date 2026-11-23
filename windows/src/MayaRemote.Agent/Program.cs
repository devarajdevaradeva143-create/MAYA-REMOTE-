using System;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;
using MayaRemote.Infrastructure.Logging;

namespace MayaRemote.Agent;

public static class Program
{
    private static AgentHost? _host;
    private static readonly CancellationTokenSource _cts = new();

    public static async Task Main(string[] args)
    {
        Console.Title = "MayaRemote Agent";
        var config = new AgentConfiguration
        {
            DeviceName = Environment.MachineName + " Agent",
            TcpPort = 47110,
            DiscoveryPort = 47111,
            AutoStart = true
        };

        _host = new AgentHost(config);
        await _host.StartAsync();

        var stopTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var input = Console.ReadLine();
                if (string.Equals(input, "stop", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
                {
                    _cts.Cancel();
                    break;
                }
            }
        }, _cts.Token);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _cts.Cancel();
        };

        await Task.WhenAny(stopTask, Task.Delay(Timeout.Infinite, _cts.Token));

        await _host.StopAsync();
    }
}
