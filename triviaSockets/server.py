import socket
import threading
import json
import urllib.request
import urllib.error
import random
from datetime import datetime
import time

BASE_API_URL = "http://192.168.100.28:8000"

salas = {}
salas_lock = threading.Lock()
client_sockets = {}

class Room:
    def __init__(self, room_id, category):
        self.room_id = room_id
        self.category = category
        self.players = []
        self.max_players = 5
        self.created_at = datetime.now()
        self.timer_start = time.time()
        self.is_active = True
        self.game_started = False
        self.start_timer = None
        self.questions = []
        self.final_scores = {}
        self.finished_players = set()

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

def get_or_create_player(player_name):
    jugadores = api_get("/jugador") or []
    
    # FIX: campo correcto es "nombreJugador" no "nomJugador"
    for j in jugadores:
        if j.get("nombreJugador", "").strip() == player_name.strip():
            return j
    
    # FIX: la API requiere "nombreJugador" y "password"
    nuevo_jugador = {"nombreJugador": player_name, "password": ""}
    try:
        json_data = json.dumps(nuevo_jugador).encode("utf-8")
        req = urllib.request.Request(
            f"{BASE_API_URL}/jugador",
            data=json_data,
            headers={'Content-Type': 'application/json'}
        )
        with urllib.request.urlopen(req, timeout=5) as response:
            if response.status == 200:
                creado = json.loads(response.read().decode("utf-8"))
                print(f"[{datetime.now()}] Nuevo jugador registrado: {creado}")
                # La API devuelve el lastrowid, no el objeto completo.
                # Necesitamos buscarlo para obtener el idJugador real.
                jugador_nuevo = api_get(f"/jugador/buscar?nombreJugador={player_name}")
                if jugador_nuevo:
                    return {"idJugador": jugador_nuevo["idJugador"], "nombreJugador": player_name}
    except Exception as e:
        print(f"[{datetime.now()}] Error al registrar jugador: {e}")
    return None

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
                
                player_id = request_data.get("player_id", 0)
                
                player_data = get_or_create_player(player_name)

                if not player_data:
                    response = {'status': 'error','message':'Error validando jugador'}
                    client_socket.send(json.dumps(response).encode("utf-8"))
                    continue

                player_id = player_data.get("idJugador") or 0   
                

                print(f"[{datetime.now()}] Solicitud join_room: player='{player_name}', ID={player_id}, category='{category}', avatar={avatar_id}")

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
                        client_socket.send(json.dumps(response).encode("utf-8"))
                        continue

                    if room.add_player({
                        'id': player_id,
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

                client_socket.send(json.dumps(response).encode("utf-8"))

            elif action == "start_game":
                room_id = request_data.get("room_id")
                with salas_lock:
                    if room_id in salas and not salas[room_id].game_started:
                        broadcast_game_start(salas[room_id], room_id)
                continue

            elif action == "close" or action == "leave_room":
                break
            
            elif action == "game_finished":
                score = request_data.get("score", 0)
                category_id = request_data.get("category_id")
                
                with salas_lock:
                    player_room = None
                    for r_id, room in salas.items():
                        if any(p['name'] == player_name for p in room.players):
                            player_room = room
                            room_id = r_id
                            break
                    
                    if player_room:
                        player_room.final_scores[player_name] = score
                        player_room.finished_players.add(player_name)
                        
                        player_info = next((p for p in player_room.players if p['name'] == player_name), None)
                        if player_info and 'id' in player_info:
                            save_score_to_db(player_info['id'], category_id, score)
                        
                        if len(player_room.finished_players) == len(player_room.players):
                            broadcast_game_over(player_room, room_id)
                continue

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
                    else:
                        broadcast_room_status(salas[room_id], room_id)
            if player_name in client_sockets:
                del client_sockets[player_name]
        client_socket.close()

def api_get(endpoint):
    try:
        with urllib.request.urlopen(f"{BASE_API_URL}{endpoint}", timeout=5) as response:
            if response.status != 200:
                return None
            return json.loads(response.read().decode("utf-8"))
    except Exception:
        return None

def save_score_to_db(player_id, category_id, score):
    try:
        data = {"IdJugador": player_id, "IdCategoria": category_id, "puntuacionTotal": score}
        json_data = json.dumps(data).encode("utf-8")
        req = urllib.request.Request(f"{BASE_API_URL}/puntuacion", data=json_data, headers={'Content-Type': 'application/json'})
        with urllib.request.urlopen(req, timeout=5):
            pass
    except Exception:
        pass

def broadcast_game_over(room, room_id):
    final_scores = []
    for player in room.players:
        player_name = player['name']
        score = room.final_scores.get(player_name, 0)
        avatar = player.get('avatar', 1)
        final_scores.append({'name': player_name, 'score': score, 'avatar': avatar})
    final_scores.sort(key=lambda x: x['score'], reverse=True)
    msg = json.dumps({'action': 'game_over', 'final_scores': final_scores, 'room_id': room_id}).encode("utf-8")
    for player in room.players:
        if player['name'] in client_sockets:
            try: client_sockets[player['name']].send(msg)
            except Exception: pass

def fetch_questions(category):
    questions = api_get(f"/pregunta/{category}") or []
    if not isinstance(questions, list): return []
    random.shuffle(questions)
    selected = []
    for q in questions:
        q_id = q.get("idPregunta")
        if q_id is None: continue
        answers = api_get(f"/respuesta/{q_id}") or []
        if not answers: continue
        random.shuffle(answers)
        selected.append({
            "idPregunta": q_id,
            "nomPregunta": q.get("nomPregunta"),
            "puntuacionPregunta": q.get("puntuacionPregunta", 0),
            "tipoPregunta": q.get("tipoPregunta", "TEXT"),
            "answers": [{
                "idRespuesta": a.get("idRespuesta"),
                "textRespuesta": a.get("textRespuesta"),
                "rutaRespuesta": a.get("rutaRespuesta"),
                "tipoRespuesta": a.get("tipoRespuesta"),
                "EsCorrecta": bool(a.get("EsCorrecta", False))
            } for a in answers]
        })
    return selected

def start_room_timer(room, room_id):
    def timeout_start():
        with salas_lock:
            if room_id in salas and not salas[room_id].game_started:
                broadcast_game_start(salas[room_id], room_id)
    room.start_timer = threading.Timer(20.0, timeout_start)
    room.start_timer.daemon = True
    room.start_timer.start()

def broadcast_room_status(room, room_id):
    msg = json.dumps({'action': 'room_status', 'room_info': room.to_dict(), 'server_time': time.time(), 'timer_start': room.timer_start}).encode("utf-8")
    for p in room.players:
        if p['name'] in client_sockets:
            try: client_sockets[p['name']].send(msg)
            except Exception: pass

def broadcast_game_start(room, room_id):
    if room.game_started: return
    room.game_started = True
    room.questions = fetch_questions(room.category)
    msg = json.dumps({'action': 'game_start', 'room_id': room_id, 'players': room.players, 'player_count': room.get_player_count(), 'category': room.category, 'server_time': time.time(), 'countdown': 20, 'questions': room.questions}).encode("utf-8")
    for p in room.players:
        if p['name'] in client_sockets:
            try: client_sockets[p['name']].send(msg)
            except Exception: pass

def run_server():
    server_ip = "192.168.100.28"
    port = 50000
    try:
        server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        server.bind((server_ip, port))
        server.listen(5)
        print(f"[{datetime.now()}] Servidor escuchando en puerto {port}")
        while True:
            c_sock, addr = server.accept()
            threading.Thread(target=handle_client, args=(c_sock, addr), daemon=True).start()
    except Exception as e:
        print(f"Error en servidor: {e}")

if __name__ == "__main__":
    run_server()