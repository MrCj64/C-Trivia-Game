from fastapi import APIRouter, Response, status
from config.db import engine
from models.tables import jugadores
from schemas.jugador import Jugador
from starlette.status import HTTP_204_NO_CONTENT
jugador = APIRouter()

@jugador.get("/jugador", response_model=list[Jugador], tags=["jugadores"])
def get_jugadores():
    with engine.connect() as conn:
        select_jugador_results = conn.execute(jugadores.select()).mappings().all()
        return select_jugador_results

@jugador.post("/jugador", response_model=Jugador, tags=["jugadores"])
def create_jugadores(j: Jugador):
    with engine.connect() as conn:
        insert_jugador_results = conn.execute(jugadores.insert().values(
            nombreJugador = j.nombreJugador,
            password = j.password))
        conn.commit()

        nuevo_jugador = conn.execute(jugadores.select().where(jugadores.c.idJugador == insert_jugador_results.lastrowid)).mappings().first()
        return nuevo_jugador

@jugador.get("/jugador/{id}", response_model=Jugador, tags=["jugadores"])
def get_jugadores_by_id(id:int):
    with engine.connect() as conn:
        busqueda_id_results = conn.execute(jugadores.select().where(jugadores.c.idJugador == id)).mappings().first()
        return busqueda_id_results
    
@jugador.delete("/jugador/{id}", status_code=status.HTTP_204_NO_CONTENT, tags=["jugadores"])
def detele_jugadores(id: int):
    with engine.connect() as conn:
        delete_jugadores_result = conn.execute(jugadores.delete().where(jugadores.c.idJugador == id))
        conn.commit()
        return Response(status_code= HTTP_204_NO_CONTENT)

@jugador.put("/jugador/{id}", response_model=Jugador, tags=["jugadores"])
def update_jugadores(id:int, j:Jugador):
    with engine.connect() as conn:
        update_jugadores_result = conn.execute(j.update().values(nombre = j.nombreJugador, password=j.password).where(jugadores.c.idJugador == id)).mappings().first()
        return "updated"