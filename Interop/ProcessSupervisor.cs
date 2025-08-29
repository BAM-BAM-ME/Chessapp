using System;
using System.Threading.Tasks;

namespace Interop
{
    public class ProcessSupervisor
    {
        private readonly EngineHost _host;
        private readonly string _path;
        private readonly int _maxRestarts;
        private int _restarts;

        public ProcessSupervisor(EngineHost host, string enginePath, int maxRestarts = 2)
        {
            _host = host; _path = enginePath; _maxRestarts = Math.Max(0, maxRestarts);
            _host.EngineCrashed += OnEngineCrashed;
        }

        private async void OnEngineCrashed(Exception ex)
        {
            if (_restarts >= _maxRestarts) return;
            _restarts++;
            await Task.Delay(500);
            try { _host.Start(_path); } catch { }
        }
    }
}
