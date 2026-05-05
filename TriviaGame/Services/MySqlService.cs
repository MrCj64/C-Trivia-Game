using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using MySql.Data.MySqlClient;
using Mysqlx.Cursor;


namespace TriviaGame.Services
{
    //Clase para declarar unicamente la estrucura de la conexión a la API
    internal class MySqlService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string base_url = "http://10.103.158.219:8000";

        public HttpClient getClient() { return client; }
        public string getBaseUrl() { return base_url; }
    }
}
