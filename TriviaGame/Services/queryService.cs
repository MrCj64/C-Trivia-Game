using MySql.Data.MySqlClient;
using TriviaGame.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TriviaGame.Services
{
    //Clase para implementar la logica de las consultas a la base de datos
    internal class queryService
    {
        private MySqlService dataB;
        private MySqlConnection conn;
        public queryService()
        {
            dataB = new MySqlService();
            conn = dataB.GetConnection();
             conn.Open();
        }

        public List<Dictionary<string, object>> GetPreguntas(int idCategoria)
        {
            List<Dictionary<string, object>> listaPreguntas = new List<Dictionary<string, object>>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM pregunta WHERE idCategoria = @idCategoria", conn);
            cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> pregunta = new Dictionary<string, object>
                {
                    { "idPregunta",             reader.GetInt32("idPregunta"        )},
                    { "puntuacionPregunta",     reader.GetInt32("puntuacionPregunta")},
                    { "nomPregunta",            reader.GetString("nomPregunta"      )},
                    { "idCategoria",            reader.GetInt32("idCategoria"       )}
                };
                listaPreguntas.Add(pregunta);
            }
            reader.Close();
            return listaPreguntas;
        }

        public List<Dictionary<string, object>> GetRespuestas(int idPregunta)
        {
            List<Dictionary<string, object>> listaRespuestas = new List<Dictionary<string, object>>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM respuesta WHERE idPregunta = @idPregunta", conn);
            cmd.Parameters.AddWithValue("@idPregunta", idPregunta);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> respuesta = new Dictionary<string, object>
                {
                    { "idRespuesta",            reader.GetInt32("idRespuesta"   )},
                    { "idPregunta",             reader.GetInt32("idPregunta"    )},
                    { "textRespuesta",          reader.IsDBNull(reader.GetOrdinal("textRespuesta")) ? null : reader.GetString("textRespuesta")},
                    { "EsCorrecta",             reader.GetBoolean("EsCorrecta"  )},
                    { "tipoRespuesta",          reader.IsDBNull(reader.GetOrdinal("tipoRespuesta")) ? null : reader.GetString("tipoRespuesta")},
                    { "rutaRespuesta",          reader.IsDBNull(reader.GetOrdinal("rutaRespuesta")) ? null : reader.GetString("rutaRespuesta")},
                };
                listaRespuestas.Add(respuesta);
            }
            reader.Close();
            return listaRespuestas;
        }

        public List<Dictionary<string, object>> GetJugadores()
        {
            List<Dictionary<string, object>> listaJugadores = new List<Dictionary<string, object>>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM jugador", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> jugadores = new Dictionary<string, object>
                {
                    { "idJugador",                reader.GetInt32("idJugador"        )},
                    { "nombreJugador",            reader.GetString("nombreJugador"    )},
                    { "password",                 reader.GetString("password"        )}
                };

                listaJugadores.Add(jugadores);
            }
            reader.Close();
            return listaJugadores;
        }

        public bool insertaJugador(string nombreJugador, string password)
        {
            string checkSql = "SELECT COUNT(*) FROM jugador WHERE NombreJugador = @nombreJugador";
            MySqlCommand checkCmd = new MySqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@nombreJugador", nombreJugador);
            long existe = (long)checkCmd.ExecuteScalar();

            if (existe > 0)
                return false;

            string insertSql = @"INSERT INTO jugador (idJugador, nombreJugador, password)
                                  VALUES (
                                    (SELECT IFNULL(MAX(j.idJugador), 0) + 1 FROM jugador j),
                                    @nombreJugador,
                                    @password
                                  )";
            MySqlCommand cmd = new MySqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@nombreJugador", nombreJugador);
            cmd.Parameters.AddWithValue("@password", password);

            int filas = cmd.ExecuteNonQuery();
            return filas > 0;
        }

        public void insertaPuntuacion(int idJugador, int idCategoria, int puntuacion)
        {
            MySqlCommand cmd = new MySqlCommand(
                @"INSERT INTO puntuacion (IdJugador, IdCategoria, puntuacionTotal) 
          VALUES (@id, @idCategoria, @puntuacion) 
          ON DUPLICATE KEY UPDATE puntuacionTotal = puntuacionTotal + @puntuacion", conn);
            cmd.Parameters.AddWithValue("@id", idJugador);
            cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
            cmd.Parameters.AddWithValue("@puntuacion", puntuacion);
            cmd.ExecuteNonQuery();
        }

        public bool LoginJugador(string nombreJugador, string password)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;

            cmd.CommandText = "SELECT COUNT(*) FROM jugador WHERE nombreJugador=@nombre AND password=@pass";

            cmd.Parameters.AddWithValue("@nombre", nombreJugador);
            cmd.Parameters.AddWithValue("@pass", password);

            int count = Convert.ToInt32(cmd.ExecuteScalar());

            return count > 0;
        }
        public string GetNombreCategoria(int idCategoria)
        {
            MySqlCommand cmd = new MySqlCommand(
                "SELECT NombreCategoria FROM categoria WHERE idCategoria = @id", conn);
            cmd.Parameters.AddWithValue("@id", idCategoria);
            object result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : $"Categoría {idCategoria}";
        }
        public int GetIdJugador(string nombreJugador)
        {
            MySqlCommand cmd = new MySqlCommand(
                "SELECT idJugador FROM jugador WHERE nombreJugador = @nombre", conn);
            cmd.Parameters.AddWithValue("@nombre", nombreJugador);
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public List<Dictionary<string, object>> GetPuntuaciones()
        {
            List<Dictionary<string, object>> lista = new List<Dictionary<string, object>>();
            MySqlCommand cmd = new MySqlCommand(
                @"SELECT j.nombreJugador, c.NombreCategoria, p.puntuacionTotal
          FROM puntuacion p
          INNER JOIN jugador j ON j.idJugador = p.IdJugador
          INNER JOIN categoria c ON c.idCategoria = p.IdCategoria
          ORDER BY p.puntuacionTotal DESC", conn);

            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Dictionary<string, object>
        {
            { "nombreJugador",   reader.GetString("nombreJugador")   },
            { "NombreCategoria", reader.GetString("NombreCategoria") },
            { "puntuacionTotal", reader.GetInt32("puntuacionTotal")  }
        });
            }
            reader.Close();
            return lista;
        }

    }

}
