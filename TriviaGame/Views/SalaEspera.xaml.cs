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

        private SocketClientService _socketClient;
        private string _roomId;
        private string _category;
        private string _playerName;
        private int _avatarId = 0;
        private bool _socketConnected = false;
        private bool _gameStarting = false;
        private Func<string, Task> _onGameStartCallback;
        private DateTime _tiempoInicio = DateTime.MinValue;
        private double _desviacionReloj = 0;

        public SalaEspera(string categoria = "General", string nombreJugador = "", Func<string, Task> onGameStart = null, SocketClientService socketClient = null)
        {
            InitializeComponent();

            _category = categoria;
            _playerName = string.IsNullOrWhiteSpace(nombreJugador) ? "Jugador" : nombreJugador;
            _roomId = $"room_{categoria}";
            _onGameStartCallback = onGameStart;
            _socketClient = socketClient ?? new SocketClientService();

            Player1.Visibility = Visibility.Hidden;
            Player2.Visibility = Visibility.Hidden;
            Player3.Visibility = Visibility.Hidden;
            Player4.Visibility = Visibility.Hidden;

            Loaded += SalaEspera_Loaded;
        }

        private async void SalaEspera_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SalaEspera_Loaded;

            await ConectarYUnirseASalaAsync();
        }

        private void IniciarTemporizador(DateTime tiempoServidor = default)
        {
            if (tiempoServidor != DateTime.MinValue)
            {
                _tiempoInicio = tiempoServidor;
                _desviacionReloj = (DateTime.UtcNow - tiempoServidor).TotalMilliseconds;
            }
            else
            {
                _tiempoInicio = DateTime.UtcNow;
                _desviacionReloj = 0;
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            ActualizarTiempoRestante();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ActualizarTiempoRestante();
        }

        private void ActualizarTiempoRestante()
        {
            if (_tiempoInicio == DateTime.MinValue) return;

            double tiempoTranscurrido = (DateTime.UtcNow - _tiempoInicio).TotalSeconds;
            int nuevoTiempo = (int)(20 - tiempoTranscurrido);

            if (nuevoTiempo != _tiempoRestante)
            {
                _tiempoRestante = nuevoTiempo;
                if (Timer != null)
                    Timer.Content = Math.Max(0, _tiempoRestante).ToString();
            }

            if (_tiempoRestante <= 0)
            {
                _timer?.Stop();
                if (!_gameStarting)
                {
                    IniciarJuego();
                }
            }
        }

        private async Task ConectarYUnirseASalaAsync()
        {
            if (_socketConnected)
            {
                System.Console.WriteLine("Ya existe una conexión activa, ignorando nueva solicitud");
                return;
            }

            try
            {
                System.Console.WriteLine($"Intentando conectar al servidor como '{_playerName}' en categoría '{_category}'");

                if (await _socketClient.ConnectAsync())
                {
                    _socketClient.OnMessageReceived += SocketClient_OnMessageReceived;

                    _avatarId = _random.Next(1, 5);

                    System.Console.WriteLine($"Enviando solicitud para unirse a sala: room={_roomId}, player={_playerName}, avatar={_avatarId}");

                    string response = await _socketClient.JoinRoomAsync(_roomId, _category, _playerName, _avatarId);

                    System.Console.WriteLine($"Respuesta del servidor: {response}");

                    var responseObj = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    if (responseObj != null && responseObj.ContainsKey("status"))
                    {
                        if (responseObj["status"].ToString() == "success")
                        {
                            _socketConnected = true;

                            DateTime tiempoServidor = DateTime.UtcNow;
                            if (responseObj.ContainsKey("timer_start"))
                            {
                                if (double.TryParse(responseObj["timer_start"].ToString(), out double timerStartUnix))
                                {
                                    tiempoServidor = UnixTimeStampToDateTime(timerStartUnix);
                                }
                            }

                            Dispatcher.Invoke(() => IniciarTemporizador(tiempoServidor));
                        }
                        else
                        {
                            System.Console.WriteLine($"Error al unirse a la sala: {responseObj.GetValueOrDefault("message", "Error desconocido")}");
                        }
                    }
                }
                else
                {
                    System.Console.WriteLine("No se pudo conectar al servidor");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error en ConectarYUnirseASalaAsync: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SocketClient_OnMessageReceived(object sender, string message)
        {
            try
            {

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<Dictionary<string, object>>(message, options);

                if (response != null && response.ContainsKey("action"))
                {
                    string action = response["action"].ToString();

                    if (action == "room_status")
                    {
                        ActualizarEstadoSala(message);
                    }
                    else if (action == "game_start")
                    {
                        string questionsJson = null;
                        if (response.TryGetValue("questions", out var questions))
                        {
                            questionsJson = questions.ToString();
                        }

                        if (!_gameStarting)
                        {
                            IniciarJuego(questionsJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void IniciarJuego(string questionsJson = null)
        {
            if (_gameStarting) return;

            _gameStarting = true;
            _timer?.Stop();

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await (_onGameStartCallback?.Invoke(questionsJson) ?? Task.CompletedTask);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            });
        }

        private void ActualizarEstadoSala(string message)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using (var doc = JsonDocument.Parse(message))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("timer_start", out JsonElement timerStartElement))
                    {
                        if (double.TryParse(timerStartElement.GetRawText(), out double timerStartUnix))
                        {
                            DateTime tiempoServidor = UnixTimeStampToDateTime(timerStartUnix);
                            if (_tiempoInicio == DateTime.MinValue || _tiempoInicio > tiempoServidor)
                            {
                                _tiempoInicio = tiempoServidor;
                            }
                        }
                    }

                    if (root.TryGetProperty("server_time", out JsonElement serverTimeElement))
                    {
                        if (double.TryParse(serverTimeElement.GetRawText(), out double serverTimeUnix))
                        {
                            DateTime tiempoServidor = UnixTimeStampToDateTime(serverTimeUnix);
                            if (_tiempoInicio == DateTime.MinValue)
                            {
                                _tiempoInicio = tiempoServidor;
                            }
                        }
                    }

                    if (root.TryGetProperty("room_info", out JsonElement roomInfoElement))
                    {
                        if (roomInfoElement.TryGetProperty("players", out JsonElement playersElement))
                        {
                            var players = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(
                                playersElement.GetRawText(), options);

                            if (players != null)
                            {
                                _jugadoresConectados = players.Count;

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
                System.Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
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

            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error mostrando jugador {indice}: {ex.Message}");
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
                System.Console.WriteLine($"Error ocultando jugador {indice}: {ex.Message}");
            }
        }

        private ImageBrush CargarAvatar(int numeroImagen)
        {
            ImageBrush pincel = new ImageBrush();
            string rutaFisica = string.Empty;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                rutaFisica = System.IO.Path.Combine(baseDir, $"Views/Avatar/{numeroImagen}.png");

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(rutaFisica, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                pincel.ImageSource = bitmap;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fallo al cargar avatar {numeroImagen}: {ex.Message}");
            }

            return pincel;
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
    }
}