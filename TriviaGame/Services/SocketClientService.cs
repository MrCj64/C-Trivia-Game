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

        public SocketClientService(string serverIp = "10.103.150.76", int serverPort = 50000)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _client = new TcpClient();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
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
                System.Diagnostics.Debug.WriteLine($"Conectado exitosamente a {_serverIp}:{_serverPort}");
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                _client?.Dispose();
                _client = new TcpClient();
                System.Diagnostics.Debug.WriteLine($"Error de conexión: {ex.Message}");
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
                            System.Diagnostics.Debug.WriteLine($"[ListenToMessagesAsync] Acción recibida: {action}");

                            if (action == "game_start")
                            {
                                System.Diagnostics.Debug.WriteLine($"[ListenToMessagesAsync] Evento game_start recibido!");
                                var eventArgs = new GameStartEventArgs();
                                OnGameStarted?.Invoke(this, eventArgs);
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

                System.Diagnostics.Debug.WriteLine($"Solicitud de unirse a sala enviada: {jsonRequest}");

                var timeoutTask = Task.Delay(5000);
                var completedTask = await Task.WhenAny(_joinResponseTcs.Task, timeoutTask);

                if (completedTask == _joinResponseTcs.Task)
                {
                    string response = await _joinResponseTcs.Task;
                    System.Diagnostics.Debug.WriteLine($"Respuesta recibida: {response}");
                    return response;
                }

                System.Diagnostics.Debug.WriteLine("Timeout esperando respuesta del servidor");
                return "{\"status\":\"error\",\"message\":\"Timeout - No se recibió respuesta del servidor\"}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en JoinRoomAsync: {ex.Message}");
                return "{\"status\":\"error\",\"message\":\"" + ex.Message + "\"}";
            }
        }

        public async Task<bool> StartGameAsync(string roomId)
        {
            if (!_isConnected || _stream == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: No hay conexión activa para iniciar juego");
                return false;
            }

            try
            {
                var request = new { action = "start_game", room_id = roomId };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();

                System.Diagnostics.Debug.WriteLine($"Solicitud de inicio de juego enviada para sala: {roomId}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al iniciar juego: {ex.Message}");
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
            try
            {
                _isConnected = false;
                _cancellationTokenSource.Cancel();

                var request = new { action = "close" };
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();

                if (_listenerTask != null)
                {
                    try
                    {
                        await _listenerTask;
                    }
                    catch { }
                }

                _stream?.Dispose();
                _client?.Dispose();
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

    public class RoomInfo
    {
        public string RoomId { get; set; }
        public string Category { get; set; }
        public int PlayerCount { get; set; }
        public bool IsFull { get; set; }
        public List<string> Players { get; set; } = new List<string>();
    }
}