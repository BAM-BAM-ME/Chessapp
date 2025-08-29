using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Core;
using Interop;

namespace Gui
{
    public partial class MainWindow : Window
    {
        private readonly GameController _game = new GameController();
        private readonly EngineHost _engine = new EngineHost();
        private InsightsService _insights = new InsightsService();
        private readonly System.Windows.Threading.DispatcherTimer _insightsTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        private bool _insightsEnabled = true;
        private string _enginePath = "Engines/stockfish.exe";
        private bool _analyzing = false;

        public MainWindow()
        {
            InitializeComponent();
            Board.MoveRequested += OnUserMoveRequested;
            _engine.InfoReceived += line => Dispatcher.Invoke(() => AppendInfo(line));
            _engine.BestMoveReceived += move => Dispatcher.Invoke(async () => await OnBestMove(move));
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

            if (_insightsEnabled)
            {
                var upd = UciParser.TryParseInfo(line);
                if (upd != null && !string.IsNullOrWhiteSpace(upd.Pv) && line.Contains(" score "))
                {
                    int? cp = upd.ScoreMate ? null : upd.ScoreCp;
                    int? mate = upd.ScoreMate ? upd.ScoreCp : null;
                    _ = _insights.AppendAsync(_game.Fen, upd.Depth, cp, mate, upd.Nps, upd.Pv);
                }
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _analyzing = false;
            await EnsureEngineAsync();
            await _engine.SendAsync("uci");
            await _engine.ExpectAsync("uciok", TimeSpan.FromSeconds(3));
            await _engine.SendAsync("isready");
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
