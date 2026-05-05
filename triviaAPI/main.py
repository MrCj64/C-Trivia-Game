from fastapi import FastAPI
from routes.jugador import jugador
from routes.categoria import categoria
from routes.pregunta import pregunta
from routes.puntuacion import puntuacion
from routes.respuesta import respuesta


app = FastAPI(
    title="TriviaBD",
    description="API para el juego de Trivia",
)

app.include_router(jugador)
app.include_router(categoria)
app.include_router(pregunta)
app.include_router(puntuacion)
app.include_router(respuesta)

@app.get("/")
def root():
    return {"message": "Bienvenido a TriviaBD API"}

if __name__ == "__main__":
        import uvicorn
        uvicorn.run(app, host="0.0.0.0", port = 8000) #Ahora escucha de todos los lados posibles