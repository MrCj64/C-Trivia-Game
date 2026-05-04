from fastapi import APIRouter, Response, status
from config.db import engine
from models.tables import preguntas
from schemas.pregunta import Pregunta

pregunta = APIRouter()

@pregunta.get("/pregunta", response_model=list[Pregunta], tags=["preguntas"])
def get_pregunta():
    with engine.connect() as conn:
        select_pregunta_results = conn.execute(preguntas.select()).mappings().all()
        return select_pregunta_results
    
@pregunta.get("/pregunta/{idCategory}", response_model=list[Pregunta], tags=["preguntas"])
def get_pregunta_by_id(idCategory: int):
    with engine.connect() as conn:
        busqueda_pregunta_by_id = conn.execute(preguntas.select().where(preguntas.c.idCategoria == idCategory)).mappings().all()
        return busqueda_pregunta_by_id