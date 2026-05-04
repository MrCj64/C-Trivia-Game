from fastapi import APIRouter, Response, status
from config.db import engine
from models.tables import puntuaciones
from schemas.puntuacion import Puntuacion

puntuacion = APIRouter()

@puntuacion.get("/puntuacion", response_model=list[Puntuacion], tags = ["puntuaciones"])
def get_puntuaciones():
    with engine.connect() as conn:
        select_puntuaciones_results = conn.execute(puntuaciones.select()).mappings().all()
        return select_puntuaciones_results
    
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

@puntuacion.post("/puntuacion", response_model=Puntuacion, tags=["puntuaciones"])
def create_puntuacion(p:Puntuacion):
    with engine.connect() as conn:
        insert_puntuacion_jugador_result = conn.execute(puntuaciones.insert().values(
            puntuacionTotal = p.puntuacionTotal,
            IdJugador = p.IdJugador,
            IdCategoria = p.IdCategoria
        ))
        conn.commit()
        return conn.execute(puntuaciones.select().where(puntuaciones.c.IdCategoria == p.idJugador)
                        .where(puntuaciones.c.IdJugador == p.idCategoria)).mappings().first() 