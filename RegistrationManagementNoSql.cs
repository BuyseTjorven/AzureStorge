using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;

namespace MCT.Functions
{
    public static class RegistrationManagementNoSql
    {
        [FunctionName("AddRegistrationNoSql")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            string tableUrl = Environment.GetEnvironmentVariable("TableUrl");
            string accountName = Environment.GetEnvironmentVariable("AccountName");
            string accountKey = Environment.GetEnvironmentVariable("AccountKey");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Persoon newPersoon = JsonConvert.DeserializeObject<Persoon>(requestBody);

            newPersoon.RegistrationId = Guid.NewGuid();

            string partionKey = newPersoon.Zipcode;
            string rowKey = newPersoon.RegistrationId.ToString();

            var tableclient = new TableClient(new Uri(tableUrl), "storageregistration", new TableSharedKeyCredential(accountName, accountKey));

            var registrationEntity = new TableEntity(partionKey, rowKey){
                {"LastName", newPersoon.LastName},
                {"FirstName", newPersoon.FirstName},
                {"Email", newPersoon.EMail},
                {"Zipcode", newPersoon.Zipcode},
                {"Age", newPersoon.Age.ToString()},
                {"IsFirtTimer", newPersoon.IsFirstTimer.ToString()},

            };

            await tableclient.AddEntityAsync(registrationEntity);
            return new OkObjectResult(registrationEntity);
        }
    }
}
