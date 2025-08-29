using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Core
{
    public class InsightsService
    {
        private readonly string _connString;
        private readonly SqliteConnection? _sharedConn;
        private bool _initialized;
        private bool _loggedError;

        public InsightsService(string? connectionString = null)
        {
            _connString = connectionString ?? $"Data Source={Path.Combine(AppContext.BaseDirectory, "Data", "insights.db")}";
            if (_connString.Contains(":memory:", StringComparison.OrdinalIgnoreCase))
            {
                _sharedConn = new SqliteConnection(_connString);
            }
        }

        public class Evaluation
        {
            public long Id { get; set; }
            public DateTime Ts { get; set; }
            public string Fen { get; set; } = string.Empty;
            public int Depth { get; set; }
            public int? ScoreCp { get; set; }
            public int? ScoreMate { get; set; }
            public int? Nps { get; set; }
            public string? Pv { get; set; }
            public string Score => ScoreMate.HasValue ? $"M{ScoreMate}" : ScoreCp?.ToString() ?? string.Empty;
        }

        private async Task<SqliteConnection> GetOpenConnectionAsync()
        {
            if (_sharedConn != null)
            {
                if (_sharedConn.State != ConnectionState.Open)
                    await _sharedConn.OpenAsync();
                return _sharedConn;
            }
            var conn = new SqliteConnection(_connString);
            await conn.OpenAsync();
            return conn;
        }

        private void ReturnConnection(SqliteConnection conn)
        {
            if (_sharedConn == null)
                conn.Dispose();
        }

        private async Task InitAsync(SqliteConnection conn)
        {
            if (_initialized) return;
            try
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS evaluations(\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        ts TEXT,\n                        fen TEXT,\n                        depth INT,\n                        score_cp INT NULL,\n                        score_mate INT NULL,\n                        nps INT NULL,\n                        pv TEXT NULL)";
                await cmd.ExecuteNonQueryAsync();
                _initialized = true;
            }
            catch (Exception ex)
            {
                LogOnce(ex);
            }
        }

        public async Task AppendAsync(string fen, int depth, int? scoreCp, int? scoreMate, int? nps, string pv)
        {
            try
            {
                var conn = await GetOpenConnectionAsync();
                try
                {
                    await InitAsync(conn);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO evaluations(ts, fen, depth, score_cp, score_mate, nps, pv) VALUES ($ts,$fen,$depth,$scp,$sm,$nps,$pv)";
                    cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
                    cmd.Parameters.AddWithValue("$fen", fen);
                    cmd.Parameters.AddWithValue("$depth", depth);
                    cmd.Parameters.AddWithValue("$scp", (object?)scoreCp ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("$sm", (object?)scoreMate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("$nps", (object?)nps ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("$pv", (object?)pv ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
                finally { ReturnConnection(conn); }
            }
            catch (Exception ex)
            {
                LogOnce(ex);
            }
        }

        public async Task<List<Evaluation>> GetLatestAsync(int count)
        {
            var list = new List<Evaluation>();
            try
            {
                var conn = await GetOpenConnectionAsync();
                try
                {
                    await InitAsync(conn);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT id, ts, fen, depth, score_cp, score_mate, nps, pv FROM evaluations ORDER BY id DESC LIMIT $cnt";
                    cmd.Parameters.AddWithValue("$cnt", count);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Evaluation
                        {
                            Id = reader.GetInt64(0),
                            Ts = DateTime.Parse(reader.GetString(1)),
                            Fen = reader.GetString(2),
                            Depth = reader.GetInt32(3),
                            ScoreCp = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                            ScoreMate = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            Nps = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                            Pv = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }
                finally { ReturnConnection(conn); }
            }
            catch (Exception ex)
            {
                LogOnce(ex);
            }
            return list;
        }

        private void LogOnce(Exception ex)
        {
            if (_loggedError) return;
            _loggedError = true;
            try { Console.Error.WriteLine($"Insights disabled: {ex.Message}"); } catch { }
        }
    }
}
