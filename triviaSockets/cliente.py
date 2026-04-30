import socket

def run_client():
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    server_ip = "10.103.151.206"
    port = 3306
    
    client.connect((server_ip,port))
    
    while True:
        msg = input("Enter message: ")
        
        client.send(msg.encode("utf-8")[:1024])
        
        response = client.recv(1024)
        response = response.decode("utf-8")
        
        if response.lower() == "closed":
            break
        
        print(f"Peticion recibida : {response}") 
        
        response = "accepted".encode("utf-8")
        
    client.close()
    print("Conexion terminada")
    
run_client()
