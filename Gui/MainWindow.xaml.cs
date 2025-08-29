using System;
}


private async Task GoAsync()
{
var posCmd = _game.ToUciPositionCommand();
await _engine.SendAsync(posCmd);
await _engine.SendAsync("go movetime 1000");
}


private async void OnUserMoveRequested(string from, string to, string? promo)
{
if (_analyzing) return;
var uci = (from ?? string.Empty) + (to ?? string.Empty) + (promo ?? string.Empty);
if (_game.TryApplyUserMove(uci))
{
Board.SetPosition(_game.Fen);
await GoAsync();
}
else
{
AppendInfo($"Invalid move: {uci}");
}
}


private async Task OnBestMove(string bestmove)
{
if (_analyzing) return;
if (_game.ApplyEngineMove(bestmove))
{
Board.SetPosition(_game.Fen);
}
else
{
AppendInfo($"Engine sent impossible bestmove: {bestmove}");
}
}


private async void BtnAnalyze_Click(object sender, RoutedEventArgs e)
{
_analyzing = true;
await EnsureEngineAsync();
await _engine.SendAsync("uci");
await _engine.ExpectAsync("uciok", TimeSpan.FromSeconds(3));
await _engine.SendAsync("isready");
await _engine.ExpectAsync("readyok", TimeSpan.FromSeconds(3));
var cfg = ConfigService.LoadAppSettings();
await _engine.ApplyCommonOptionsAsync(cfg);
await _engine.SendAsync(_game.ToUciPositionCommand());
await _engine.SendAsync("setoption name MultiPV value 3");
await _engine.SendAsync("go infinite");
}


private async void BtnStop_Click(object sender, RoutedEventArgs e)
{
if (_engine.IsRunning)
{
await _engine.SendAsync("stop");
}
_analyzing = false;
}


private void BtnLoadEngine_Click(object sender, RoutedEventArgs e)
{
var ofd = new OpenFileDialog { Filter = "Engine (*.exe)|*.exe" };
if (ofd.ShowDialog() == true)
{
_enginePath = ofd.FileName;
var cfg = ConfigService.LoadAppSettings();
cfg.EnginePath = _enginePath;
ConfigService.SaveAppSettings(cfg);
AppendInfo($"Engine set: {_enginePath}");
}
}
}
}
