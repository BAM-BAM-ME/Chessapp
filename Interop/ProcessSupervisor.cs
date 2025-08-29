using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Core;

namespace Interop
{
    public class ProcessSupervisor
    {
        private readonly ILogger _logger;
        private readonly EngineHost _host;
        private readonly string _path;
        private readonly int _maxRestarts;
        private int _restarts;

        public ProcessSupervisor(EngineHost host, string enginePath, int maxRestarts = 2, ILogger<ProcessSupervisor>? logger = null)
        {
            _logger = logger ?? Logging.Factory.CreateLogger<ProcessSupervisor>();
            _host = host; _path = enginePath; _maxRestarts = Math.Max(0, maxRestarts);
            _host.EngineCrashed += OnEngineCrashed;
        }

        private async void OnEngineCrashed(Exception ex)
        {
            if (_restarts >= _maxRestarts) return;
            _restarts++;
            _logger.LogWarning(ex, "Engine crashed, restarting {Restart}/{Max}", _restarts, _maxRestarts);
            await Task.Delay(500);
            try { _host.Start(_path); } catch (Exception restartEx) { _logger.LogError(restartEx, "Engine restart failed"); }
        }
    }
}
