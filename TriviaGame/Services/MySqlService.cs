using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using Mysqlx.Cursor;

namespace TriviaGame.Services
{
    //Clase para declarar unicamente la estrucura del servicio de mysql
    internal class MySqlService
    {
        private string myConnectionString = "Server=127.0.0.1;" +
                                            "Database=TriviaGameBD;" +
                                            "User ID =root;" +
                                            "Password=123;";
        MySqlConnection myConnection;
        public MySqlConnection GetConnection()
        {
            myConnection = new MySqlConnection(myConnectionString);
            return myConnection;
        }
    }
}
