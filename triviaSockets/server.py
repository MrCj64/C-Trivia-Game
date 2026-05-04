import socket
import threading
import json
from datetime import datetime


salas = {}
salas_lock = threading.Lock()
client_sockets = {}

class Room:
    def __init__(self, room_id, category):
        self.room_id = room_id
        self.category = category
        self.players = []
        self.max_players = 4
        self.created_at = datetime.now()
        self.is_active = True
        self.game_started = False

    def add_player(self, player_info):
        if len(self.players) < self.max_players:
            self.players.append(player_info)
            return True
        return False

    def remove_player(self, player_name):
        self.players = [p for p in self.players if p['name'] != player_name]

    def get_player_count(self):
        return len(self.players)

    def is_full(self):
        return len(self.players) >= self.max_players

    def to_dict(self):
        return {
            'room_id': self.room_id,
            'category': self.category,
            'players': self.players,
            'player_count': len(self.players),
            'is_full': self.is_full()
        }

def handle_client(client_socket, addr):
    player_name = None
    room_id = None

    try:
        while True:
            request = client_socket.recv(1024).decode("utf-8")

            if not request:
                break

            request_data = json.loads(request)
            action = request_data.get("action")

            if action == "join_room":
                room_id = request_data.get("room_id")
                category = request_data.get("category")
                player_name = request_data.get("player_name")
                avatar_id = request_data.get("avatar", 0)

                with salas_lock:
                    if room_id not in salas:
                        salas[room_id] = Room(room_id, category)

                    room = salas[room_id]

                    if room.add_player({
                        'name': player_name,
                        'ip': addr[0],
                        'port': addr[1],
                        'avatar': avatar_id,
                        'joined_at': datetime.now().isoformat()
                    }):
                        client_sockets[player_name] = client_socket

                        response = {
                            'status': 'success',
                            'message': f'{player_name} se ha unido a la sala',
                            'room_info': room.to_dict()
                        }
                        print(f"[{datetime.now()}] {player_name} se unió a sala {room_id}. Total jugadores: {room.get_player_count()}")

                        broadcast_room_status(room, room_id)

                        if room.is_full():
                            broadcast_game_start(room, room_id)
                    else:
                        response = {
                            'status': 'error',
                            'message': 'Sala llena'
                        }
                        print(f"[{datetime.now()}] {player_name} intentó unirse a sala llena {room_id}")

                client_socket.send(json.dumps(response).encode("utf-8"))

    except json.JSONDecodeError as e:
        print(f"[{datetime.now()}] Error JSON de {addr}: {e}")
    except Exception as e:
        print(f"[{datetime.now()}] Error manejando cliente {addr}: {e}")
    finally:
        if room_id and player_name:
            with salas_lock:
                if room_id in salas:
                    salas[room_id].remove_player(player_name)
                    if salas[room_id].get_player_count() == 0:
                        del salas[room_id]

            if player_name in client_sockets:
                del client_sockets[player_name]

        client_socket.close()
        print(f"[{datetime.now()}] Conexión cerrada con {addr[0]}:{addr[1]}")

def broadcast_room_status(room, room_id):
    message = {
        'action': 'room_status',
        'room_info': room.to_dict()
    }
    message_json = json.dumps(message).encode("utf-8")
    for player in room.players:
        player_name = player['name']
        if player_name in client_sockets:
            try:
                client_sockets[player_name].send(message_json)
            except Exception as e:
                pass

def broadcast_game_start(room, room_id):
    """Envía mensaje de inicio de juego a todos los jugadores de la sala"""
    print(f"[{datetime.now()}] broadcast_game_start() llamado para sala {room_id}")
    print(f"[{datetime.now()}] Jugadores en sala: {len(room.players)}")
    print(f"[{datetime.now()}] client_sockets disponibles: {list(client_sockets.keys())}")

    game_start_message = {
        'action': 'game_start',
        'room_id': room_id,
        'players': room.players,
        'player_count': room.get_player_count(),
        'category': room.category
    }

    message_json = json.dumps(game_start_message).encode("utf-8")

    for player in room.players:
        player_name = player['name']
        print(f"[{datetime.now()}] Buscando socket para jugador: {player_name}")
        if player_name in client_sockets:
            try:
                client_sockets[player_name].send(message_json)
                print(f"[{datetime.now()}] Mensaje de inicio enviado a {player_name}")
            except Exception as e:
                print(f"[{datetime.now()}] Error enviando mensaje a {player_name}: {e}")
        else:
            print(f"[{datetime.now()}] Socket no encontrado para {player_name}")

    room.game_started = True
    print(f"[{datetime.now()}] Juego iniciado en sala {room_id} con {room.get_player_count()} jugadores")

def run_server():
    server_ip = "10.103.150.76"
    port = 50000

    try:
        server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        server.bind((server_ip, port))
        server.listen(5)

        print(f"[{datetime.now()}] Servidor escuchando en puerto {port}")

        while True:
            client_socket, addr = server.accept()
            print(f"[{datetime.now()}] Nueva conexión de {addr[0]}:{addr[1]}")

            thread = threading.Thread(target=handle_client, args=(client_socket, addr,), daemon=True)
            thread.start()

    except Exception as e:
        print(f"[{datetime.now()}] Error en servidor: {e}")
    finally:
        server.close()
        print(f"[{datetime.now()}] Servidor cerrado")

if __name__ == "__main__":
    run_server()