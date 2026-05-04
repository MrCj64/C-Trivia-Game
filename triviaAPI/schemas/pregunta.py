from pydantic import BaseModel
from typing import Optional

class Pregunta(BaseModel):
        idPregunta: Optional[int] = None
        puntuacionPregunta: int
        nomPregunta: str
        idCategoria: Optional[int] = None