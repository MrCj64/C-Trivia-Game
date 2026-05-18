using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TriviaGame.Services
{
    public class SocketClientService
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _serverIp;
        private readonly int _serverPort;
        private bool _isConnected = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listenerTask;

        private TaskCompletionSource<string> _joinResponseTcs;

        public event EventHandler<string> OnMessageReceived;
        public event EventHandler<string> OnConnectionStatusChanged;
        public event EventHandler<GameStartEventArgs> OnGameStarted;
        public event EventHandler<GameOverEventArgs> OnGameOver;

        public SocketClientService(string serverIp = "192.168.100.28", int serverPort = 50000)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _client = new TcpClient();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> ConnectAsync()
        {
            if (_isConnected && _client != null && _client.Connected)
            {
                OnConnectionStatusChanged?.Invoke(this, "Ya existe una conexión activa");
                return true;
            }

            try
            {
                _client = new TcpClient();
                var connectTask = _client.ConnectAsync(_serverIp, _serverPort);
                var timeoutTask = Task.Delay(5000);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    _client?.Dispose();
                    _client = new TcpClient();
                    OnConnectionStatusChanged?.Invoke(this, "Error al conectar: Timeout");
                    return false;
                }

                _stream = _client.GetStream();
                _isConnected = true;
                _cancellationTokenSource = new CancellationTokenSource();
                _listenerTask = Task.Run(() => ListenToMessagesAsync(_cancellationTokenSource.Token));
                OnConnectionStatusChanged?.Invoke(this, "Conectado al servidor");

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                _client?.Dispose();
                _client = new TcpClient();
                OnConnectionStatusChanged?.Invoke(this, $"Error al conectar: {ex.Message}");
                return false;
            }
        }

        private async Task ListenToMessagesAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];
            while (_isConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    try
                    {
                        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                        if (response != null && response.ContainsKey("status") && _joinResponseTcs != null)
                        {
                            _joinResponseTcs.TrySetResult(jsonResponse);
                        }

                        OnMessageReceived?.Invoke(this, jsonResponse);

                        if (response != null && response.ContainsKey("action"))
                        {
                            string action = response["action"].ToString();
                            if (action == "game_start")
                            {
                                OnGameStarted?.Invoke(this, new GameStartEventArgs());
                            }
                            else if (action == "game_over")
                            {
                                var finalScores = new List<(string, int, int)>();
                                if (response.ContainsKey("final_scores"))
                                {
                                    var scoresArray = response["final_scores"] as JsonElement?;
                                    if (scoresArray.HasValue && scoresArray.Value.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var scoreElement in scoresArray.Value.EnumerateArray())
                                        {
                                            string name = scoreElement.GetProperty("name").GetString() ?? "";
                                            int score = scoreElement.GetProperty("score").GetInt32();
                                            int avatar = scoreElement.GetProperty("avatar").GetInt32();
                                            finalScores.Add((name, score, avatar));
                                        }
                                    }
                                }
                                OnGameOver?.Invoke(this, new GameOverEventArgs { FinalScores = finalScores });
                            }
                        }
                    }
                    catch
                    {
                        OnMessageReceived?.Invoke(this, jsonResponse);
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        public async Task<string> JoinRoomAsync(string roomId, string category, string playerName, int avatarId)
        {
            if (!_isConnected || _stream == null)
            {
                return "{\"status\":\"error\",\"message\":\"No hay conexión activa con el servidor\"}";
            }

            _joinResponseTcs = new TaskCompletionSource<string>();

            try
            {
                var request = new { action = "join_room", room_id = roomId, category = category, player_name = playerName, avatar = avatarId };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();

                var timeoutTask = Task.Delay(5000);
                var completedTask = await Task.WhenAny(_joinResponseTcs.Task, timeoutTask);

                if (completedTask == _joinResponseTcs.Task)
                {
                    return await _joinResponseTcs.Task;
                }

                return "{\"status\":\"error\",\"message\":\"Timeout - No se recibió respuesta del servidor\"}";
            }
            catch (Exception ex)
            {
                return "{\"status\":\"error\",\"message\":\"" + ex.Message + "\"}";
            }
        }

        public async Task<bool> StartGameAsync(string roomId)
        {
            if (!_isConnected || _stream == null) return false;
            try
            {
                var request = new { action = "start_game", room_id = roomId };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendGameFinishedAsync(int score, int categoryId)
        {
            if (!_isConnected || _stream == null) return false;
            try
            {
                var request = new { action = "game_finished", score = score, category_id = categoryId };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LeaveRoomAsync(string roomId)
        {
            try
            {
                var request = new { action = "leave_room", room_id = roomId };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                OnConnectionStatusChanged?.Invoke(this, $"Error al salir de la sala: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            if (!_isConnected && _client == null) return true;

            try
            {
                _isConnected = false;
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                if (_stream != null && _client != null && _client.Connected)
                {
                    try
                    {
                        var request = new { action = "close" };
                        string jsonRequest = JsonSerializer.Serialize(request);
                        byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                        await _stream.WriteAsync(data, 0, data.Length);
                        await _stream.FlushAsync();
                    }
                    catch { }
                }

                if (_listenerTask != null && !_listenerTask.IsCompleted)
                {
                    await Task.WhenAny(_listenerTask, Task.Delay(2000));
                }

                _stream?.Close();
                _stream?.Dispose();
                _stream = null;

                if (_client != null)
                {
                    if (_client.Connected) _client.Close();
                    _client.Dispose();
                    _client = null;
                }

                OnConnectionStatusChanged?.Invoke(this, "Desconectado del servidor");
                return true;
            }
            catch (Exception ex)
            {
                OnConnectionStatusChanged?.Invoke(this, $"Error al desconectar: {ex.Message}");
                return false;
            }
        }

        public bool IsConnected => _isConnected;
    }

    public class GameStartEventArgs : EventArgs
    {
        public string RoomId { get; set; }
        public List<string> Players { get; set; }
        public int PlayerCount { get; set; }
    }

    public class GameOverEventArgs : EventArgs
    {
        public List<(string name, int score, int avatar)> FinalScores { get; set; }
    }

    public class RoomInfo
    {
        public string RoomId { get; set; }
        public string Category { get; set; }
        public int PlayerCount { get; set; }
        public bool IsFull { get; set; }
        public List<string> Players { get; set; } = new List<string>();
    }
}