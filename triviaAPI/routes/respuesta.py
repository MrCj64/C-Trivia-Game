from fastapi import APIRouter, Response, status
from config.db import engine
from models.tables import respuestas
from schemas.respuesta import Respuesta

respuesta = APIRouter()
@respuesta.get("/respuesta", response_model=list[Respuesta], tags=["respuestas"])
def get_respuestas():
    with engine.connect() as conn:
        select_respuestas_results = conn.execute(respuestas.select()).mappings().all()
        return select_respuestas_results
    
@respuesta.get("/respuesta/{idPregunta}", response_model=list[Respuesta], tags=["respuestas"])
def get_respuestas_by_categoryID(idPregunta : int):
    with engine.connect() as conn:
        busqueda_respuestas_idPregunta = conn.execute(respuestas.select().where(respuestas.c.idPregunta == idPregunta)).mappings().all()
        return busqueda_respuestas_idPregunta
    
@respuesta.get("/respuesta/{idPregunta}/{EsCorrecta}", response_model=Respuesta, tags=["respuestas"])
def get_respuestas_by_categoryID_esCorrecta(idPregunta : int, EsCorrecta: bool):
    with engine.connect() as conn:
        busqueda_respuestas_idPregunta_esCorrecta = conn.execute(respuestas.select()
                                                             .where(respuestas.c.idPregunta == idPregunta)
                                                             .where(respuestas.c.EsCorrecta == EsCorrecta)).mappings().first()
    return busqueda_respuestas_idPregunta_esCorrecta