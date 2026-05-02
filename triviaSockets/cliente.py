import socket


def run_client():
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    server_ip = "192.168.0.226"
    server_port = 3306
    client.connect((server_ip, server_port))

    try:
        while True:
            msg = input("Enter message: ")
            client.send(msg.encode("utf-8")[:1024])

            response = client.recv(1024)
            response = response.decode("utf-8")

            if response.lower() == "closed":
                break

            print(f"Received: {response}")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        client.close()
        print("Connection to server closed")


run_client()

