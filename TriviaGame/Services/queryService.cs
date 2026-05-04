using MySql.Data.MySqlClient;
using TriviaGame.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace TriviaGame.Services
{
    //Clase para implementar la logica de las consultas a la base de datos
    internal class queryService
    {
        private MySqlService dataAPIconn;
        private MySqlService dataB;
        private MySqlConnection conn;
        private readonly HttpClient client;
        private readonly string base_url;
        public queryService()
        {
            dataAPIconn = new MySqlService();
            client = dataAPIconn.getClient();
            base_url = dataAPIconn.getBaseUrl();
            dataB = new MySqlService();
            conn = dataB.GetConnection();
            conn.Open();
        }

        public async Task<List<Dictionary<string, object>>> GetPreguntas(int idCategoria)
        {
            try { 
                List<Dictionary<string, object>> listaPreguntas;
                HttpResponseMessage response = await client.GetAsync($"{base_url}/pregunta/{idCategoria}");
                response.EnsureSuccessStatusCode();
                string preguntasJson = await response.Content.ReadAsStringAsync();

                listaPreguntas = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(preguntasJson);
                return listaPreguntas;
            }
            catch(Exception e)
            {
                return null;
            }

        } 

        public async Task<List<Dictionary<string, object>>> GetRespuestas(int idPregunta)
        {
            try
            {
                List<Dictionary<string, object>> listaRespuestas;
                HttpResponseMessage response = await client.GetAsync($"{base_url}/respuesta/{idPregunta}");
                response.EnsureSuccessStatusCode();
                string respuestasJson = await response.Content.ReadAsStringAsync();

                listaRespuestas = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(respuestasJson);
                return listaRespuestas;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetJugadores()
        {
            try
            {
                List<Dictionary<string, object>> listaJugadores;
                HttpResponseMessage response = await client.GetAsync($"{base_url}/jugador");
                response.EnsureSuccessStatusCode();
                string jugadoresJson = await response.Content.ReadAsStringAsync();

                listaJugadores = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jugadoresJson);
                return listaJugadores;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<string> GetNombreCategoria(int idCategoria)
        {
            try
            {
                string categorias;
                HttpResponseMessage response = await client.GetAsync($"{base_url}/pregunta/{idCategoria}");
                response.EnsureSuccessStatusCode();
                string categoriasJson = await response.Content.ReadAsStringAsync();

                categorias = JsonConvert.DeserializeObject<string>(categoriasJson);
                return categorias;
            }
            catch (Exception e)
            {
                return null;
            }
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
