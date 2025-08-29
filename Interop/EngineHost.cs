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
    /// <summary>
    /// Hosts a UCI engine process and provides async send/receive primitives.
    /// Lifecycle is fully cancellable and deterministically disposed.
    /// </summary>
    public class EngineHost : IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;
        private Process? _proc;
        private readonly BlockingCollection<string> _outgoing = new();
        private CancellationTokenSource? _cts;
        private Task? _writerTask;
        private Task? _readerTask;

        public bool IsRunning => _proc != null && !_proc.HasExited;

        public event Action? EngineReady;
        public event Action<string>? InfoReceived;
        public event Action<string>? BestMoveReceived;
        public event Action<Exception>? EngineCrashed;

codex/add-simple-logging-with-microsoft.extensions.logging
        public EngineHost(ILogger<EngineHost>? logger = null)
        {
            _logger = logger ?? Logging.Factory.CreateLogger<EngineHost>();
        }


        /// <summary>Starts the engine process.</summary>
main
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
                EnableRaisingEvents = true,
            };
            _proc.Exited += (_, __) =>
            {
                _logger.LogWarning("Engine process exited");
                EngineCrashed?.Invoke(new Exception("Engine process exited."));
            };
            _proc.Start();
            _writerTask = Task.Run(() => WriterLoop(_cts.Token));
            _readerTask = Task.Run(() => ReaderLoop(_cts.Token));
        }

        public async Task SendAsync(string command)
        {
            if (!IsRunning) throw new InvalidOperationException("Engine not running");
            _logger.LogDebug("send: {Cmd}", command);
            _outgoing.Add(command);
            await Task.CompletedTask;
        }

        /// <summary>Waits until a line containing <paramref name="token"/> appears.</summary>
        public async Task ExpectAsync(string token, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void handler(string line)
            {
                if (line.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) tcs.TrySetResult();
            }

            InfoReceived += handler;
            try
            {
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
                await tcs.Task.WaitAsync(linked.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Timeout waiting for '{token}'");
            }
            finally
            {
                InfoReceived -= handler;
            }
        }

        /// <summary>
        /// Sends the <c>uci</c> command and awaits the <c>uciok</c> response.
        /// </summary>
        public async Task UciHandshakeAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            await SendAsync("uci");
            await ExpectAsync("uciok", timeout, cancellationToken);
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
                    string? line = await _proc!.StandardOutput.ReadLineAsync(ct);
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
            catch (OperationCanceledException) { }
            catch (Exception ex) { EngineCrashed?.Invoke(ex); }
        }

        public void Stop() => StopAsync().GetAwaiter().GetResult();

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping engine");
            try
            {
                _cts?.Cancel();

                if (_proc != null && !_proc.HasExited)
                {
                    try { await _proc.StandardInput.WriteLineAsync("quit"); } catch { }

                    using var exitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    exitCts.CancelAfter(TimeSpan.FromSeconds(1));
                    try { await _proc.WaitForExitAsync(exitCts.Token); }
                    catch (OperationCanceledException)
                    {
                        if (!_proc.HasExited) _proc.Kill(entireProcessTree: true);
                    }
                }

                if (_writerTask != null) await _writerTask.ConfigureAwait(false);
                if (_readerTask != null) await _readerTask.ConfigureAwait(false);
            }
            catch { }
            finally
            {
                _proc?.Dispose();
                _proc = null;
                _cts?.Dispose();
                _cts = null;
                _writerTask = _readerTask = null;
            }
        }

        public async Task ApplyCommonOptionsAsync(Core.AppSettings cfg)
        {
            if (!IsRunning) return;
            if (cfg.Threads > 0) await SendAsync($"setoption name Threads value {cfg.Threads}");
            if (cfg.Hash > 0) await SendAsync($"setoption name Hash value {cfg.Hash}");
            await SendAsync($"setoption name Ponder value {(cfg.Ponder ? "true" : "false")}");
            if (!string.IsNullOrWhiteSpace(cfg.SyzygyPath))
                await SendAsync($"setoption name SyzygyPath value {cfg.SyzygyPath}");
            await SendAsync($"setoption name MultiPV value {cfg.MultiPV}");
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            GC.SuppressFinalize(this);
        }
    }
}

