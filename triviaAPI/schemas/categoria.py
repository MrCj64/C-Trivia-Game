from pydantic import BaseModel
from typing import Optional

class Categoria(BaseModel):
        idCategoria: Optional[int] = None
        NombreCategoria: str