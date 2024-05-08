using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using LegacyApp.Enums;
using LegacyApp.Models;

namespace LegacyApp.Repositories
{
    public class ClientRepository
    {
        private readonly string _connectionString;
        
        public ClientRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["appDatabase"].ConnectionString;
        }
        
        public Client GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "uspGetClientById"
                };

                var parametr = new SqlParameter("@clientId", SqlDbType.Int) { Value = id };
                command.Parameters.Add(parametr);
                
                connection.Open();
                var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                
                if (reader.Read())
                {
                    return new Client
                    {
                        Id = reader.GetInt32("ClientId"),
                        Name = reader["Name"].ToString(),
                        ClientStatus = (ClientStatus) int.Parse(reader["ClientStatus"].ToString())
                    };
                }
            }

            return null;
        }
    }
}