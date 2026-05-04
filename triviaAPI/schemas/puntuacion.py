from pydantic import BaseModel
from typing import Optional

class Puntuacion(BaseModel):
        puntuacionTotal : int
        IdJugador : Optional[int] = None
        IdCategoria : Optional[int] = None