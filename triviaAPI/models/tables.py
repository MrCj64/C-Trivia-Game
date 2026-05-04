from sqlalchemy import Table, Column
from sqlalchemy.sql.sqltypes import Integer, VARCHAR, BOOLEAN
from config.db import meta

jugadores = Table("jugador", meta, 
                Column("idJugador", Integer, primary_key=True),
                Column("nombreJugador", VARCHAR(45)),
                Column("password", VARCHAR(45))
            )

categorias = Table("categoria", meta, 
                Column("idCategoria", Integer, primary_key=True),
                Column("NombreCategoria", VARCHAR(45))
                )

partidas = Table("partida", meta, 
                Column("idJugador", Integer, primary_key=True),
                Column("idPregunta", Integer, primary_key=True),
                Column("idRespuesta", Integer)
                )

preguntas = Table("pregunta", meta, 
                  Column("idPregunta", Integer, primary_key=True),
                  Column("puntuacionPregunta", Integer),
                  Column("nomPregunta", VARCHAR(150)),
                  Column("idCategoria", Integer)
                  )

puntuaciones = Table("puntuacion", meta, 
                     Column("puntuacionTotal", Integer),
                     Column("IdJugador", Integer, primary_key=True),
                     Column("IdCategoria", Integer, primary_key=True)
                     )

respuestas = Table("respuesta", meta, 
                   Column("idRespuesta", Integer, primary_key=True),
                   Column("idPregunta", Integer),
                   Column("textRespuesta", VARCHAR(200)),
                   Column("EsCorrecta", BOOLEAN),
                   Column("tipoRespuesta", VARCHAR(45)),
                   Column("rutaRespuesta", VARCHAR(200))
                   )