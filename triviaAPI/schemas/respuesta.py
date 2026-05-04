from pydantic import BaseModel
from typing import Optional

class Respuesta(BaseModel):
        idRespuesta : Optional[int] = None
        idPregunta : Optional[int] = None
        textRespuesta : str
        EsCorrecta : bool
        tipoRespuesta : str
        rutaRespuesta : Optional[str] = None