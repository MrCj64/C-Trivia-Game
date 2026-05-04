from fastapi import APIRouter, Response, status
from config.db import engine
from models.tables import categorias
from schemas.categoria import Categoria

categoria = APIRouter()

@categoria.get("/categoria", response_model=list[Categoria], tags=["categorias"])
def get_categorias():
    with engine.connect() as conn:
        select_categorias_results = conn.execute(categorias.select()).mappings().all()
        return select_categorias_results
    
@categoria.get("/categoria/{idCategoria}", response_model=Categoria, tags=["categorias"])
def get_categorias_by_id(idCategoria: int):
    with engine.connect() as conn:
        busqueda_categorias_id_results= conn.execute(categorias.select().where(categorias.c.idCategoria == idCategoria)).mappings().first()
        return busqueda_categorias_id_results
