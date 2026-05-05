using MySql.Data.MySqlClient;
using TriviaGame.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Windows;

namespace TriviaGame.Services
{
    //Clase para implementar la logica de las consultas a la base de datos
    internal class queryService
    {
        private MySqlService dataAPIconn;
        private readonly HttpClient client;
        private readonly string base_url;
        public queryService()
        {
            dataAPIconn = new MySqlService();
            client = dataAPIconn.getClient();
            base_url = dataAPIconn.getBaseUrl();
        }

        public async Task<List<Dictionary<string, object>>> GetPreguntas(int idCategoria)
        {
            try
            {
                List<Dictionary<string, object>> listaPreguntas;
                HttpResponseMessage response = await client.GetAsync($"{base_url}/pregunta/{idCategoria}");
                response.EnsureSuccessStatusCode();
                string preguntasJson = await response.Content.ReadAsStringAsync();

                listaPreguntas = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(preguntasJson);
                return listaPreguntas;
            }
            catch (Exception e)
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

        public async Task<bool> insertaJugador(string nombreJugador, string password)
        {
            try
            {
                string checkSQL;
                string insercionJugador;
                var parametros = new
                {
                    nombreJugador = nombreJugador,
                    password = password
                };

                string jsonString = JsonConvert.SerializeObject(parametros);
                var httpContent = new StringContent(jsonString.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.GetAsync($"{base_url}/jugador/existe?nombreJugador={nombreJugador}");
                response.EnsureSuccessStatusCode();
                checkSQL = await response.Content.ReadAsStringAsync();
                if (int.Parse(checkSQL) > 0) return false; ;


                response = await client.PostAsync($"{base_url}/jugador", httpContent);
                string insertJugador = await response.Content.ReadAsStringAsync();
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task insertaPuntuacion(int idJugador, int idCategoria, int puntuacion)
        {

            var nuevaPuntuacion = new
            {
                IdJugador = idJugador,
                idCategoria = idCategoria,
                puntuacionTotal = puntuacion
            };

            try
            {
                string json = JsonConvert.SerializeObject(nuevaPuntuacion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"{base_url}/puntuacion", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                MessageBox.Show("e");
            }


        }

        public async Task<bool> LoginJugador(string nombreJugador, string password)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(
                    $"{base_url}/jugador/login?nombreJugador={nombreJugador}&password={password}"
                );
                response.EnsureSuccessStatusCode();
                string respuestaJson = await response.Content.ReadAsStringAsync();

                var resultado = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuestaJson);
                return Convert.ToBoolean(resultado["existe"]);
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public async Task<int> GetIdJugador(string nombreJugador)
        {
            try
            {
                int idJugador;
                HttpResponseMessage response = await client.GetAsync($"{base_url}/jugador/buscar?nombreJugador={nombreJugador}");
                response.EnsureSuccessStatusCode();
                string jugadorId = await response.Content.ReadAsStringAsync();

                idJugador = JsonConvert.DeserializeObject<int>(jugadorId);
                return idJugador;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetPuntuaciones()
        {
            try
            {
                List<Dictionary<string, object>> listaPuntuaciones;
                HttpResponseMessage response = await client.GetAsync($"{base_url}/puntuacion/ranking");
                response.EnsureSuccessStatusCode();
                string puntuacionesJSon = await response.Content.ReadAsStringAsync();

                listaPuntuaciones = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(puntuacionesJSon);
                return listaPuntuaciones;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
