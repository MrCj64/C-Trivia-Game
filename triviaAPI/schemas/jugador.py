from pydantic import BaseModel
from typing import Optional

class Jugador(BaseModel):
        idJugador: Optional[int] = None
        nombreJugador: str
        password: str