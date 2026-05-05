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
        private string _serverIp;
        private int _serverPort;
        private string _playerName;
        private string _roomId;
        private bool _isConnected = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listenerTask;

        private TaskCompletionSource<string> _joinResponseTcs;

        public event EventHandler<string> OnMessageReceived;
        public event EventHandler<string> OnConnectionStatusChanged;
        public event EventHandler<GameStartEventArgs> OnGameStarted;
        public event EventHandler<GameOverEventArgs> OnGameOver;

        public SocketClientService(string serverIp = "192.168.0.226", int serverPort = 50000)
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

            if (_client != null && _client.Connected)
            {
                await DisconnectAsync();
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
                                var eventArgs = new GameStartEventArgs();
                                OnGameStarted?.Invoke(this, eventArgs);
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
                                var eventArgs = new GameOverEventArgs { FinalScores = finalScores };
                                OnGameOver?.Invoke(this, eventArgs);
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
                    string response = await _joinResponseTcs.Task;
                    return response;
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
            if (!_isConnected || _stream == null)
            {
                return false;
            }

            try
            {
                var request = new { action = "start_game", room_id = roomId };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendGameFinishedAsync(int score, int categoryId)
        {
            if (!_isConnected || _stream == null)
            {
                return false;
            }

            try
            {
                var request = new { action = "game_finished", score = score, category_id = categoryId };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();

                return true;
            }
            catch (Exception ex)
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

        /// <summary>
        /// Desconecta del servidor de forma segura y espera a que el listener se cierre
        /// </summary>
        public async Task<bool> DisconnectAsync()
        {
            System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Iniciando desconexión...");

            // Si ya estaba desconectado, retornar inmediatamente
            if (!_isConnected && _client == null)
            {
                System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Ya estaba desconectado");
                return true;
            }

            try
            {
                // PASO 1: Marcar como desconectado PRIMERO para evitar condiciones de carrera
                _isConnected = false;
                System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Marcado como desconectado");

                // PASO 2: Cancelar el listener token para que se detenga la lectura
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Token de cancelación enviado");
                }

                // PASO 3: Intentar enviar mensaje de cierre al servidor
                if (_stream != null && _client != null && _client.Connected)
                {
                    try
                    {
                        var request = new { action = "close" };
                        string jsonRequest = JsonSerializer.Serialize(request);
                        byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                        await _stream.WriteAsync(data, 0, data.Length);
                        await _stream.FlushAsync();
                        System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Mensaje de cierre enviado al servidor");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DisconnectAsync] No se pudo enviar mensaje de cierre: {ex.Message}");
                        // Continuar con desconexión aunque falle el envío
                    }
                }

                // PASO 4: Esperar a que el listener termine (con timeout)
                if (_listenerTask != null && !_listenerTask.IsCompleted)
                {
                    try
                    {
                        var completedTask = await Task.WhenAny(_listenerTask, Task.Delay(2000));
                        if (completedTask == _listenerTask)
                        {
                            System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Listener terminó exitosamente");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Timeout esperando listener (2 segundos)");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DisconnectAsync] Error esperando listener: {ex.Message}");
                    }
                }

                // PASO 5: Cerrar stream
                if (_stream != null)
                {
                    try
                    {
                        _stream.Close();
                        _stream.Dispose();
                        _stream = null;
                        System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Stream cerrado");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DisconnectAsync] Error cerrando stream: {ex.Message}");
                    }
                }

                // PASO 6: Cerrar cliente TCP
                if (_client != null)
                {
                    try
                    {
                        if (_client.Connected)
                        {
                            _client.Close();
                        }
                        _client.Dispose();
                        _client = null;
                        System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Cliente TCP cerrado");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DisconnectAsync] Error cerrando cliente: {ex.Message}");
                    }
                }

                OnConnectionStatusChanged?.Invoke(this, "Desconectado del servidor");
                System.Diagnostics.Debug.WriteLine("[DisconnectAsync] Desconexión completada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DisconnectAsync] Error durante desconexión: {ex.Message}");
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