using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
/add-engine-selection-modal-to-settings-ui

using System.ComponentModel;
using System.Text;
using Microsoft.Win32;
main
using Chessapp.Core;
using Chessapp.Interop;

namespace Gui
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly GameController _game = new GameController();
        private readonly EngineHost _engine = new EngineHost();
        private InsightsService _insights = new InsightsService();
        private readonly System.Windows.Threading.DispatcherTimer _insightsTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        private bool _insightsEnabled = true;
        private string _enginePath = "Engines/stockfish.exe";
        private bool _analyzing = false;
        private readonly StringBuilder _engineLog = new StringBuilder();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string EngineLog => _engineLog.ToString();

        private void AppendEngineLog(string line)
        {
            _engineLog.AppendLine(line);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EngineLog)));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Board.MoveRequested += OnUserMoveRequested;
            _engine.InfoReceived += line => Dispatcher.Invoke(() => AppendInfo(line));
            _engine.BestMoveReceived += move => Dispatcher.Invoke(async () => { AppendEngineLog($"bestmove {move}"); await OnBestMove(move); });
            _engine.EngineReady += () => Dispatcher.Invoke(() => AppendInfo("readyok"));
            _insightsTimer.Tick += async (_, __) =>
            {
                if (_engine.IsRunning && _insightsEnabled)
                    await InsightsPanel.RefreshAsync();
            };
            _insightsTimer.Start();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var config = ConfigService.LoadAppSettings();
                if (!string.IsNullOrWhiteSpace(config.EnginePath)) _enginePath = config.EnginePath;
                _insightsEnabled = config.Insights;
                _insights = new InsightsService(config.InsightsDb);
                InsightsPanel.Service = _insights;
            }
            catch { }
        }

        private void AppendInfo(string line)
        {
            TxtInfo.AppendText(line + Environment.NewLine);
            TxtInfo.ScrollToEnd();

            var upd = UciParser.TryParseInfo(line);
            if (upd != null && !string.IsNullOrWhiteSpace(upd.Pv))
            {
                var score = upd.ScoreMate ? $"mate {upd.ScoreCp}" : $"cp {upd.ScoreCp}";
                AppendEngineLog($"d{upd.Depth} {score} {upd.Pv}");
                if (_insightsEnabled)
                {
                    int? cp = upd.ScoreMate ? null : upd.ScoreCp;
                    int? mate = upd.ScoreMate ? upd.ScoreCp : null;
                    _ = _insights.AppendAsync(_game.Fen, upd.Depth, cp, mate, upd.Nps, upd.Pv);
                }
            }
        }

        private async Task SendCommandAsync(string cmd)
        {
            await _engine.SendAsync(cmd);
            AppendEngineLog($"> {cmd}");
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _analyzing = false;
            await EnsureEngineAsync();
            await SendCommandAsync("uci");
            await _engine.ExpectAsync("uciok", TimeSpan.FromSeconds(3));
            await SendCommandAsync("isready");
            await _engine.ExpectAsync("readyok", TimeSpan.FromSeconds(3));

            var cfg = ConfigService.LoadAppSettings();
            await _engine.ApplyCommonOptionsAsync(cfg);

            _game.NewGame();
            Board.SetPosition(_game.Fen);
            await GoAsync();
        }

        private async Task EnsureEngineAsync()
        {
            if (!_engine.IsRunning)
            {
                if (!File.Exists(_enginePath))
                {
                    AppendInfo("Engine not found. Set the path with the Engine button.");
                    throw new FileNotFoundException("stockfish.exe missing");
                }
                _engine.Start(_enginePath);
            }
        }

        private async Task GoAsync()
        {
            var posCmd = _game.ToUciPositionCommand();
            await SendCommandAsync(posCmd);
            await SendCommandAsync("go movetime 1000");
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
            await SendCommandAsync("uci");
            await _engine.ExpectAsync("uciok", TimeSpan.FromSeconds(3));
            await SendCommandAsync("isready");
            await _engine.ExpectAsync("readyok", TimeSpan.FromSeconds(3));
            var cfg = ConfigService.LoadAppSettings();
/add-engine-selection-modal-to-settings-ui
            await _engine.SendAsync(_game.ToUciPositionCommand());
            await _engine.SendAsync("setoption name MultiPV value 3");
            await _engine.SendAsync($"go depth {cfg.Depth}");

            await SendCommandAsync(_game.ToUciPositionCommand());
            await SendCommandAsync("setoption name MultiPV value 3");
            await SendCommandAsync("go infinite");
 main
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_engine.IsRunning)
            {
                await SendCommandAsync("stop");
            }
            _analyzing = false;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsWindow();
            if (dlg.ShowDialog() == true)
            {
                var cfg = ConfigService.LoadAppSettings();
                _enginePath = cfg.EnginePath;
                AppendInfo($"Engine set: {_enginePath}");
            }
        }
    }
}
