using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

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

        public List<Dictionary<string, object>> GetPreguntas()
        {
            List<Dictionary<string,object>> listaPreguntas = new List<Dictionary<string, object>>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM pregunta", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary <string, object> preguntas = new Dictionary<string, object>
                {
                    { "idPregunta",             reader.GetInt32("idPregunta"        )},
                    { "puntuacionPregunta",     reader.GetInt32("puntuacionPregunta")},
                    { "nomPregunta",            reader.GetString("nomPregunta"      )},
                    { "idCategoria",            reader.GetInt32("idCategoria"       )}
                };

            listaPreguntas.Add(preguntas);
          
            }
            reader.Close();
            return listaPreguntas;
        }

        public List<Dictionary<string, object>> GetRespuestas()
        {
            List<Dictionary<string, object>> ListaRespuestas = new List<Dictionary<string, object>>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM respuesta", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> respuestas = new Dictionary<string, object>
                {
                    { "idRespuesta",            reader.GetInt32("idRespuesta"   )},
                    { "idPregunta",             reader.GetInt32("idPregunta"    )},
                    { "textRespuesta",          reader.GetString("textRespuesta")},
                    { "EsCorrecta",             reader.GetBoolean("EsCorrecta"  )},
                    { "tipoRespuesta",          reader.GetString("tipoRespuesta")},
                    { "rutaRespuesta",          reader.GetString("rutaRespuesta")},
                };

                ListaRespuestas.Add(respuestas);
            }
            reader.Close();
            return ListaRespuestas;
        }

        public List<Dictionary<string, object>> GetJugadores()
        {
            List<Dictionary<string, object>> listaJugadores = new List<Dictionary<string, object>>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM respuesta", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> jugadores = new Dictionary<string, object>
                {
                    { "idJugador",                reader.GetInt32("idJugador"        )},
                    { "nombreJugador",            reader.GetInt32("nombreJugador"    )},
                    { "password",                 reader.GetString("password"        )}
                };

                listaJugadores.Add(jugadores);
            }
            reader.Close();
            return listaJugadores;
        }

        public void insertaJugador(string nombreJugador, string password)
        {
            MySqlCommand cmd = new MySqlCommand();
            
            cmd.CommandText = "INSERT INTO jugadores (idJugador, NombreJugador, password) VALUES ('{@nuevoId, @nombreJugador, @password}')";
            cmd.Parameters.AddWithValue("@nombre", nombreJugador);
            cmd.Parameters.AddWithValue("@password", password);

            cmd.ExecuteNonQuery();
        }

        public void insertaPuntuacion(int idJugador, int puntuacion)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "INSERT INTO jugador (idJugador, puntuacionTotal) VALUES (@id, @puntuacion) ON DUPLICATE KEY puntuacionTotal = puntuacionTotal + @puntuacion";
            cmd.Parameters.AddWithValue("@id", idJugador);
            cmd.Parameters.AddWithValue("@puntuacion", puntuacion);

            cmd.ExecuteNonQuery();
        }
    }
}
