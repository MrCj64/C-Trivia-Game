import socket
import threading
import json
from datetime import datetime
import time

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
        self.timer_start = time.time()
        self.is_active = True
        self.game_started = False
        self.start_timer = None

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
                category = request_data.get("category")
                base_room_id = f"room_{category}"

                player_name = request_data.get("player_name", "").strip()
                avatar_id = request_data.get("avatar", 0)

                if not player_name:
                    response = {
                        'status': 'error',
                        'message': 'Nombre de jugador vacío o inválido'
                    }
                    print(f"[{datetime.now()}] Rechazado: nombre de jugador vacío desde {addr}")
                    client_socket.send(json.dumps(response).encode("utf-8"))
                    continue

                print(f"[{datetime.now()}] Solicitud join_room: player='{player_name}', category='{category}', avatar={avatar_id}")

                with salas_lock:
                    room_id = base_room_id
                    room_number = 1

                    while room_id in salas and (salas[room_id].is_full() or salas[room_id].game_started):
                        room_number += 1
                        room_id = f"{base_room_id}_{room_number}"

                    if room_id not in salas:
                        salas[room_id] = Room(room_id, category)
                        print(f"[{datetime.now()}] Nueva sala creada: {room_id} para categoría {category}")

                    room = salas[room_id]

                    if any(p['name'] == player_name for p in room.players):
                        response = {
                            'status': 'error',
                            'message': f'{player_name} ya está en esta sala'
                        }
                        print(f"[{datetime.now()}] {player_name} ya está en sala {room_id}, rechazando conexión duplicada")
                        client_socket.send(json.dumps(response).encode("utf-8"))
                        continue

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
                            'room_info': room.to_dict(),
                            'room_id': room_id,
                            'timer_start': datetime.now().isoformat()
                        }
                        print(f"[{datetime.now()}] ✓ {player_name} (avatar {avatar_id}) se unió a sala {room_id}. Total jugadores: {room.get_player_count()}/{room.max_players}")
                        print(f"[{datetime.now()}] Jugadores en {room_id}: {[p['name'] for p in room.players]}")

                        broadcast_room_status(room, room_id)

                        if room.get_player_count() == 1 and not room.game_started:
                            start_room_timer(room, room_id)

                        if room.is_full() and not room.game_started:
                            if room.start_timer:
                                room.start_timer.cancel()
                            broadcast_game_start(room, room_id)
                    else:
                        response = {
                            'status': 'error',
                            'message': 'Sala llena'
                        }
                        print(f"[{datetime.now()}] {player_name} intentó unirse a sala llena {room_id}")

                client_socket.send(json.dumps(response).encode("utf-8"))

            elif action == "close" or action == "leave_room":
                print(f"[{datetime.now()}] Cliente {addr} ({player_name}) solicita {action}")
                break
            
            else:
                print(f"[{datetime.now()}] Acción desconocida '{action}' de {addr}")

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
                        if salas[room_id].start_timer:
                            salas[room_id].start_timer.cancel()
                        del salas[room_id]
                        print(f"[{datetime.now()}] Sala {room_id} eliminada (vacía)")
                    else:
                        broadcast_room_status(salas[room_id], room_id)

            if player_name in client_sockets:
                del client_sockets[player_name]

        client_socket.close()
        print(f"[{datetime.now()}] Conexión cerrada con {addr[0]}:{addr[1]}")

def start_room_timer(room, room_id):
    """Inicia un timer de 20 segundos para iniciar el juego automáticamente"""
    def timeout_start():
        with salas_lock:
            if room_id in salas and not salas[room_id].game_started:
                print(f"[{datetime.now()}] Timer expirado para sala {room_id}. Iniciando juego con {salas[room_id].get_player_count()} jugadores")
                broadcast_game_start(salas[room_id], room_id)
    
    room.start_timer = threading.Timer(20.0, timeout_start)
    room.start_timer.daemon = True
    room.start_timer.start()
    print(f"[{datetime.now()}] Timer de 20 segundos iniciado para sala {room_id}")

def broadcast_room_status(room, room_id):
    """Envía actualización del estado de la sala a todos los jugadores"""
    message = {
        'action': 'room_status',
        'room_info': room.to_dict(),
        'server_time': time.time(),
        'timer_start': room.timer_start
    }
    message_json = json.dumps(message).encode("utf-8")

    for player in room.players:
        player_name = player['name']
        if player_name in client_sockets:
            try:
                client_sockets[player_name].send(message_json)
            except Exception as e:
                print(f"[{datetime.now()}] Error enviando room_status a {player_name}: {e}")

def broadcast_game_start(room, room_id):
    """Envía mensaje de inicio de juego sincronizado a todos los jugadores"""
    if room.game_started:
        return

    room.game_started = True

    print(f"[{datetime.now()}] broadcast_game_start() llamado para sala {room_id}")
    print(f"[{datetime.now()}] Jugadores en sala: {len(room.players)}")

    server_time = time.time()

    game_start_message = {
        'action': 'game_start',
        'room_id': room_id,
        'players': room.players,
        'player_count': room.get_player_count(),
        'category': room.category,
        'server_time': server_time,
        'countdown': 20
    }

    message_json = json.dumps(game_start_message).encode("utf-8")

    for player in room.players:
        player_name = player['name']
        if player_name in client_sockets:
            try:
                client_sockets[player_name].send(message_json)
                print(f"[{datetime.now()}] Mensaje de inicio enviado a {player_name}")
            except Exception as e:
                print(f"[{datetime.now()}] Error enviando mensaje a {player_name}: {e}")
        else:
            print(f"[{datetime.now()}] Socket no encontrado para {player_name}")

    print(f"[{datetime.now()}] Juego iniciado en sala {room_id} con {room.get_player_count()} jugadores")

def run_server():
    server_ip = "192.168.0.226"
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