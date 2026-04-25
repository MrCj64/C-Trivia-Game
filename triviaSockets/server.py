import socket

def run_server():
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    server_ip = "10.103.151.206"
    port = 3306
    
    server.bind((server_ip, port))
    server.listen(0)
    
    print(f"Escuchando en la direccion {server_ip}, en el puerto: {port}")
    
    client_socket, client_address = server.accept()
    print(f"Se establecio la conexion con:{client_address[0]}:{client_address[1]}")
    
    while True:
        request = client_socket.recv(1024)
        request = request.decode("utf-8")
        
        if request.lower() == "close":
            client_socket.send("closed".encode("utf-8"))
            break
        
        print(f"Peticion recibida : {request}") 
        
        response = "accepted".encode("utf-8")
        client_socket.send(response)
        
    client_socket.close()
    print("Conexion terminada")
    
    server.close()
    
run_server()