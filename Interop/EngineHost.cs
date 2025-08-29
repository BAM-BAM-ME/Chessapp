using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Core;

namespace Interop
{
    public class EngineHost : IDisposable
    {
        private readonly ILogger _logger;
        private Process? _proc;
        private readonly BlockingCollection<string> _outgoing = new();
        private CancellationTokenSource? _cts;
        public bool IsRunning => _proc != null && !_proc.HasExited;

        public event Action? EngineReady;
        public event Action<string>? InfoReceived;
        public event Action<string>? BestMoveReceived;
        public event Action<Exception>? EngineCrashed;

        public EngineHost(ILogger<EngineHost>? logger = null)
        {
            _logger = logger ?? Logging.Factory.CreateLogger<EngineHost>();
        }

        public void Start(string enginePath)
        {
            _logger.LogInformation("Starting engine: {Path}", enginePath);
            Stop();
            _cts = new CancellationTokenSource();
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = enginePath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(enginePath) ?? Environment.CurrentDirectory
                },
                EnableRaisingEvents = true
            };
            _proc.Exited += (_, __) =>
            {
                _logger.LogWarning("Engine process exited");
                EngineCrashed?.Invoke(new Exception("Engine process exited."));
            };
            _proc.Start();
            Task.Run(() => WriterLoop(_cts.Token));
            Task.Run(() => ReaderLoop(_cts.Token));
        }

        public async Task SendAsync(string command)
        {
            if (!IsRunning) throw new InvalidOperationException("Engine not running");
            _logger.LogDebug("send: {Cmd}", command);
            _outgoing.Add(command);
            await Task.CompletedTask;
        }

        public async Task ExpectAsync(string token, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void handler(string line)
            {
                if (line.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) tcs.TrySetResult(true);
            }
            InfoReceived += handler;
            try
            {
                using var cts = new CancellationTokenSource(timeout);
                await Task.WhenAny(tcs.Task, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token));
                if (!tcs.Task.IsCompleted) throw new TimeoutException($"Timeout waiting for '{token}'");
            }
            finally { InfoReceived -= handler; }
        }

        private async Task WriterLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsRunning)
                {
                    string line = _outgoing.Take(ct);
                    await _proc!.StandardInput.WriteLineAsync(line);
                    await _proc!.StandardInput.FlushAsync();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { EngineCrashed?.Invoke(ex); }
        }

        private async Task ReaderLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsRunning && !_proc!.StandardOutput.EndOfStream)
                {
                    string? line = await _proc!.StandardOutput.ReadLineAsync();
                    if (line is null) break;
                    _logger.LogDebug("recv: {Line}", line);
                    if (line.StartsWith("info ")) InfoReceived?.Invoke(line);
                    else if (line.StartsWith("bestmove "))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2) BestMoveReceived?.Invoke(parts[1]);
                    }
                    else
                    {
                        InfoReceived?.Invoke(line);
                        if (line.IndexOf("readyok", StringComparison.OrdinalIgnoreCase) >= 0) EngineReady?.Invoke();
                    }
                }
            }
            catch (Exception ex) { EngineCrashed?.Invoke(ex); }
        }

        public void Stop()
        {
            _logger.LogInformation("Stopping engine");
            try
            {
                _cts?.Cancel();
                if (_proc != null && !_proc.HasExited)
                {
                    try { _proc.StandardInput.WriteLine("quit"); } catch { }
                    _proc.Kill(entireProcessTree: true);
                }
            }
            catch { }
            finally { _proc?.Dispose(); _proc = null; }
        }

        public async Task ApplyCommonOptionsAsync(Core.AppSettings cfg)
        {
            if (!IsRunning) return;
            if (cfg.Threads > 0) await SendAsync($"setoption name Threads value {cfg.Threads}");
            if (cfg.Hash > 0) await SendAsync($"setoption name Hash value {cfg.Hash}");
            await SendAsync($"setoption name Ponder value {(cfg.Ponder ? "true" : "false")}");
            if (!string.IsNullOrWhiteSpace(cfg.SyzygyPath)) await SendAsync($"setoption name SyzygyPath value {cfg.SyzygyPath}");
            await SendAsync($"setoption name MultiPV value {cfg.MultiPV}");
        }

        public void Dispose() => Stop();
    }
}
