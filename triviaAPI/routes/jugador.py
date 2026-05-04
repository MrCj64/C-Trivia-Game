from fastapi import APIRouter, Response, status
from sqlalchemy import func, select
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

@jugador.get("/jugador/existe", tags=["jugadores"])
def verificar_Jugador(nombreJugador: str):
    with engine.connect() as conn:
        login_result = conn.execute(select(func.count()).select_from(jugadores).
                                    where(jugadores.c.nombreJugador == nombreJugador)).scalar()
        return login_result > 0

@jugador.post("/jugador", tags=["jugadores"])
def create_jugador(j: Jugador):
    with engine.connect() as conn:
        
        if verificar_Jugador(j.nombreJugador) > 0: return { "message" : "Existe"}
        
        max_id = conn.execute(
            select(func.ifnull(func.max(jugadores.c.idJugador), 0))
        ).scalar()
        
        nuevo_id = max_id + 1

        result = conn.execute(jugadores.insert().values(
            idJugador=nuevo_id,
            nombreJugador=j.nombreJugador,
            password=j.password
        ))
        conn.commit()

        return result.lastrowid

@jugador.get("/jugador/login", tags=["jugadores"])
def login_jugador(nombreJugador: str, password:str):
    with engine.connect() as conn:
        login_result = conn.execute(select(func.count()).select_from(jugadores).
                                    where(jugadores.c.nombreJugador == nombreJugador).
                                    where(jugadores.c.password == password)).scalar()
        return {"existe": login_result > 0}

@jugador.get("/jugador/buscar", tags=["jugadores"])
def get_jugador_by_nombre(nombreJugador : str):
    with engine.connect() as conn:
        result = conn.execute(jugadores.select().where(jugadores.c.nombreJugador == nombreJugador)) .mappings().first()   
    if result :
        return {"idJugador" : result["idJugador"]}
    else :
        return {"idJugador": None}

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
