import socket
import threading

def handle_client(client_socket, addr):
    try:
        while True:
          request = client_socket.recv(1024).decode("utf-8") 
          if request.lower() == "close":
              client_socket.send("closed".encode("utf-8"))
              break
          print(f"Received: {request}")
          
          response = "Accepted"
          client_socket.send(response.encode("utf-8"))
          
    except Exception as e:
        print(f"Error when hanlding client: {e}")
    finally:
        client_socket.close()
        print(f"Connection to client ({addr[0]}:{addr[1]}) closed")
        
def run_server():
    
    server_ip = "192.168.0.226"
    port = 3306
    
    try:
        server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server.bind((server_ip, port))
        server.listen()
        
        print(f"Escuchando en la direccion {server_ip}, en el puerto: {port}")
        
        while True:
            client_socket, addr = server.accept()
            
            print(f"Se establecio la conexion con:{addr[0]}:{addr[1]}")
            
            thread = threading.Thread(target=handle_client, args=(client_socket, addr,))
            thread.start()
            
    except Exception as e:
        print(f"Error: {e}")
    finally:
        server.close()
 
run_server()      