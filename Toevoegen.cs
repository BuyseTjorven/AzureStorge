using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;
using MCT.Functions;

namespace MCT.Functions
{
    public static class Toevoegen
    {
        [FunctionName("Toevoegen")]
        public static async Task<IActionResult> Add(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            Guid regId = System.Guid.NewGuid();
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            Persoon persoon = JsonConvert.DeserializeObject<Persoon>(body);
            Persoon result = new Persoon();
            result.RegistrationId = regId;
            result.LastName = persoon.LastName;
            result.FirstName = persoon.FirstName;
            result.EMail = persoon.EMail;
            result.Zipcode = persoon.Zipcode;
            result.Age = persoon.Age;
            result.IsFirstTimer = persoon.IsFirstTimer;
            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "INSERT INTO Persoon (RegistrationID, LastName, FistName, EMail, Zipcode, Age, IsFirstTimer)  VALUES(@RegistrationID, @LastName, @FistName, @EMail, @Zipcode, @Age, @IsFirstTimer)";
                    command.Parameters.AddWithValue("@RegistrationID", result.RegistrationId);
                    command.Parameters.AddWithValue("@LastName", result.LastName);
                    command.Parameters.AddWithValue("@FistName", result.FirstName);
                    command.Parameters.AddWithValue("@EMail", result.EMail);
                    command.Parameters.AddWithValue("@Zipcode", result.Zipcode);
                    command.Parameters.AddWithValue("@Age", result.Age);
                    command.Parameters.AddWithValue("@IsFirstTimer", result.IsFirstTimer);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return new OkObjectResult(result);
        }

        [FunctionName("Lezen")]
        public static async Task<IActionResult> Read(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="v1/registrations")] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            List<Persoon> registraties = new List<Persoon>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT * FROM Persoon";
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var persoon = new Persoon();
                        persoon.RegistrationId =Guid.Parse(reader["RegistrationId"].ToString());
                        persoon.LastName=reader["LastName"].ToString();
                        persoon.FirstName=reader["FistName"].ToString();
                        persoon.EMail = reader["Email"].ToString();
                        persoon.Zipcode = reader["Zipcode"].ToString();
                        persoon.Age =Convert.ToInt32(reader["Age"]);
                        persoon.IsFirstTimer = Convert.ToBoolean(reader["IsFirstTimer"]);
                        registraties.Add(persoon);
                    }
                }
            }
            return new OkObjectResult(registraties);
        }
    }
}
