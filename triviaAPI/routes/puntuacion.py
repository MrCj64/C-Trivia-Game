from fastapi import APIRouter, Response, status
from config.db import engine
from models.tables import puntuaciones, jugadores, categorias
from sqlalchemy import select
from schemas.puntuacion import Puntuacion, PuntuacionRanking
from sqlalchemy.dialects.mysql import insert as mysql_insert

puntuacion = APIRouter()

@puntuacion.get("/puntuacion/ranking", response_model=list[PuntuacionRanking], tags=["puntuaciones"])
def get_puntuaciones():
    with engine.connect() as conn:
        return conn.execute(
            select(
                jugadores.c.nombreJugador,
                categorias.c.NombreCategoria,
                puntuaciones.c.puntuacionTotal
            )
            .select_from(puntuaciones)
            .join(jugadores, jugadores.c.idJugador == puntuaciones.c.IdJugador)
            .join(categorias, categorias.c.idCategoria == puntuaciones.c.IdCategoria)
            .order_by(puntuaciones.c.puntuacionTotal.desc())
        ).mappings().all()


@puntuacion.get("/puntuacion/{idCategory}", response_model=list[Puntuacion], tags = ["puntuaciones"])
def get_puntuaciones_by_categoryID(idCategory : int):
    with engine.connect() as conn:
        busqueda_puntuaciones_byCategory = conn.execute(puntuaciones.select().where(puntuaciones.c.IdCategoria == idCategory)).mappings().all()
        return busqueda_puntuaciones_byCategory
    
@puntuacion.get("/puntuacion/{idCategory}/{idJugador}", response_model=Puntuacion, tags = ["puntuaciones"])
def get_puntuaciones_by_category_jugador(idCategory: int, idJugador: int):
    with engine.connect() as conn:
        busqueda_puntuaciones_categoryJugador = conn.execute(puntuaciones.select()
                                                             .where(puntuaciones.c.IdCategoria == idCategory)
                                                             .where(puntuaciones.c.idJugador == idJugador)).mappings().all()
    return busqueda_puntuaciones_categoryJugador

@puntuacion.post("/puntuacion", tags=["puntuaciones"])
def create_puntuacion(p:Puntuacion):
    with engine.connect() as conn:
        create_puntuacion_query = mysql_insert(puntuaciones).values(
            IdJugador = p.IdJugador,
            IdCategoria = p.IdCategoria,
            puntuacionTotal = p.puntuacionTotal
        ).on_duplicate_key_update(puntuacionTotal=puntuaciones.c.puntuacionTotal + p.puntuacionTotal)

        conn.execute(create_puntuacion_query)
        conn.commit()
        resultado = conn.execute(puntuaciones.select().where(puntuaciones.c.IdJugador == p.IdJugador)
                                 .where(puntuaciones.c.IdCategoria == p.IdCategoria)).mappings().first()
        
        return resultado
        