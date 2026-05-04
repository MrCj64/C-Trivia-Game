using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Shapes;
using TriviaGame.Services;
using System.Threading.Tasks;

namespace TriviaGame.Views
{
    public partial class SalaEspera : UserControl
    {
        DispatcherTimer _timer;
        int _tiempoRestante = 20;
        int _jugadoresConectados = 0;
        Random _random = new Random();
        List<int> _avataresUsados = new List<int>();

        private SocketClientService _socketClient = new SocketClientService();
        private string _roomId;
        private string _category;
        private string _playerName;
        private int _avatarId = 0;
        private bool _socketConnected = false;
        private bool _gameStarting = false;
        private Func<Task> _onGameStartCallback;

        public SalaEspera(string categoria = "General", string nombreJugador = "", Func<Task> onGameStart = null)
        {
            InitializeComponent();

            _category = categoria;
            _playerName = nombreJugador;
            _roomId = Guid.NewGuid().ToString().Substring(0, 8);
            _onGameStartCallback = onGameStart;

            Player1.Visibility = Visibility.Hidden;
            Player2.Visibility = Visibility.Hidden;
            Player3.Visibility = Visibility.Hidden;
            Player4.Visibility = Visibility.Hidden;

            // Conectar socket y unirse a la sala
            _ = ConectarYUnirseASalaAsync();

            IniciarTemporizador();
        }

        private void IniciarTemporizador()
        {
            System.Diagnostics.Debug.WriteLine($"[SalaEspera] Iniciando temporizador. Tiempo inicial: {_tiempoRestante} segundos");
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            // Mostrar el valor inicial inmediatamente
            if (Timer != null)
                Timer.Content = _tiempoRestante.ToString();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_tiempoRestante > 0)
            {
                _tiempoRestante--;
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Timer: {_tiempoRestante} segundos restantes");
                if (Timer != null)
                    Timer.Content = _tiempoRestante.ToString();
            }
            else
            {
                _timer.Stop();
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Timer llegó a 0");

                if (!_gameStarting)
                {
                    _gameStarting = true;
                    System.Diagnostics.Debug.WriteLine($"[SalaEspera] Timer llegó a 0, invocando callback");

                    // Invocar el callback de forma asíncrona sin bloquear el dispatcher
                    Dispatcher.InvokeAsync(async () =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalaEspera] Ejecutando callback del timer");
                            await (_onGameStartCallback?.Invoke() ?? Task.CompletedTask);
                            System.Diagnostics.Debug.WriteLine($"[SalaEspera] Callback del timer completado");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error en callback: {ex.Message}\n{ex.StackTrace}");
                        }
                    });
                }
            }
        }

        private async Task ConectarYUnirseASalaAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Iniciando conexión al servidor...");

                // Conectar al servidor
                if (await _socketClient.ConnectAsync())
                {
                    System.Diagnostics.Debug.WriteLine($"[SalaEspera] Conectado al servidor. Uniéndose a sala...");
                    _socketClient.OnMessageReceived += SocketClient_OnMessageReceived;
                    _socketClient.OnGameStarted += SocketClient_OnGameStarted;

                    // Seleccionar un avatar aleatorio
                    _avatarId = _random.Next(1, 5);

                    // Unirse a la sala
                    string response = await _socketClient.JoinRoomAsync(_roomId, _category, _playerName, _avatarId);
                    System.Diagnostics.Debug.WriteLine($"[SalaEspera] Respuesta de unirse a sala: {response}");

                    _socketConnected = true;
                    MostrarJugador(0, _playerName, _avatarId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error: No se pudo conectar al servidor");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error en ConectarYUnirseASalaAsync: {ex.Message}");
            }
        }

        private void SocketClient_OnMessageReceived(object sender, string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Mensaje recibido: {message}");

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(message, options);

                if (response != null && response.ContainsKey("action"))
                {
                    string action = response["action"].ToString();

                    if (action == "room_status")
                    {
                        System.Diagnostics.Debug.WriteLine($"[SalaEspera] room_status recibido");
                        ActualizarEstadoSala(message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error procesando mensaje: {ex.Message}");
            }
        }

        private void SocketClient_OnGameStarted(object sender, GameStartEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[SalaEspera] Evento game_started recibido");
            Dispatcher.InvokeAsync(async () =>
            {
                if (!_gameStarting)
                {
                    _gameStarting = true;
                    _timer.Stop();
                    System.Diagnostics.Debug.WriteLine($"[SalaEspera] Socket game_started, invocando callback");
                    try
                    {
                        await (_onGameStartCallback?.Invoke() ?? Task.CompletedTask);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error en callback de game_started: {ex.Message}");
                    }
                }
            });
        }

        private void ActualizarEstadoSala(string message)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using (var doc = System.Text.Json.JsonDocument.Parse(message))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("room_info", out JsonElement roomInfoElement))
                    {
                        if (roomInfoElement.TryGetProperty("players", out JsonElement playersElement))
                        {
                            var players = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(
                                playersElement.GetRawText(), options);

                            if (players != null)
                            {
                                _jugadoresConectados = players.Count;
                                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Actualizando UI con {_jugadoresConectados} jugadores");

                                Dispatcher.Invoke(() =>
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        OcultarJugador(i);
                                    }

                                    for (int i = 0; i < players.Count && i < 4; i++)
                                    {
                                        var player = players[i];
                                        string playerName = player.ContainsKey("name") ? player["name"].ToString() : $"Jugador{i + 1}";
                                        int avatarId = 1;

                                        if (player.ContainsKey("avatar"))
                                        {
                                            if (int.TryParse(player["avatar"].ToString(), out int parsedAvatar))
                                            {
                                                avatarId = parsedAvatar;
                                            }
                                        }

                                        MostrarJugador(i, playerName, avatarId);
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error actualizando estado de sala: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void MostrarJugador(int indice, string nombreJugador, int numeroAvatar)
        {
            try
            {
                StackPanel playerPanel;
                Ellipse avatarEllipse;
                TextBlock textBlock;

                switch (indice)
                {
                    case 0:
                        playerPanel = Player1;
                        avatarEllipse = Avatar1;
                        textBlock = TxtUsuario1;
                        break;
                    case 1:
                        playerPanel = Player2;
                        avatarEllipse = Avatar2;
                        textBlock = TxtUsuario2;
                        break;
                    case 2:
                        playerPanel = Player3;
                        avatarEllipse = Avatar3;
                        textBlock = TxtUsuario3;
                        break;
                    case 3:
                        playerPanel = Player4;
                        avatarEllipse = Avatar4;
                        textBlock = TxtUsuario4;
                        break;
                    default:
                        return;
                }

                playerPanel.Visibility = Visibility.Visible;
                avatarEllipse.Fill = CargarAvatar(numeroAvatar);
                textBlock.Text = nombreJugador;

                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Jugador {indice} mostrado: {nombreJugador} con avatar {numeroAvatar}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error mostrando jugador {indice}: {ex.Message}");
            }
        }

        private void OcultarJugador(int indice)
        {
            try
            {
                StackPanel playerPanel;

                switch (indice)
                {
                    case 0:
                        playerPanel = Player1;
                        break;
                    case 1:
                        playerPanel = Player2;
                        break;
                    case 2:
                        playerPanel = Player3;
                        break;
                    case 3:
                        playerPanel = Player4;
                        break;
                    default:
                        return;
                }

                playerPanel.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalaEspera] Error ocultando jugador {indice}: {ex.Message}");
            }
        }

        private ImageBrush CargarAvatar(int numeroImagen)
        {
            ImageBrush pincel = new ImageBrush();
            string rutaFisica = string.Empty;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                rutaFisica = System.IO.Path.Combine(baseDir, $"Views/Imagenes/Avatares/avatar{numeroImagen}.png");

                if (!System.IO.File.Exists(rutaFisica))
                {
                    string rutaAlternativa = System.IO.Path.Combine(baseDir, $"../..//Views/Imagenes/Avatares/avatar{numeroImagen}.png");
                    if (System.IO.File.Exists(rutaAlternativa))
                    {
                        rutaFisica = rutaAlternativa;
                    }
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(rutaFisica, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                pincel.ImageSource = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fallo al cargar avatar {numeroImagen}: {ex.Message}");
            }

            return pincel;
        }
    }
}