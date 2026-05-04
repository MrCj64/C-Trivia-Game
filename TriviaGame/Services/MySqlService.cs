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
        private const string base_url = "http://127.0.0.1:8000";

        private string myConnectionString = "Server=127.0.0.1;" +
                                            "Database=tiviagamebd;" +
                                            "User ID =root;" +
                                            "Password=123;";
        MySqlConnection myConnection;
        public MySqlConnection GetConnection()
        {
            myConnection = new MySqlConnection(myConnectionString);
            return myConnection;
        }

        public  HttpClient getClient() { return client; }
        public string getBaseUrl() {  return base_url; }
    }
}
