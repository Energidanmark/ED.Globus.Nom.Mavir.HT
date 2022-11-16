using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ED.Globus.Nom.Mavir.HT
{
    public static class Function1
    {
        [FunctionName("MekToPartner")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            var fileLogger = new FileLogger();

            log.LogDebug("C# HTTP trigger function processed a request1.");

            string requestBodyAsStr = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogDebug("We got a request from Mavir");

            var str = "INSERT INTO [Bulk].[RawMavirReplies]([Data],[LastUpdatedUtc]) VALUES('{0}', GETUTCDATE())";
            var insertStatement = string.Format(str, requestBodyAsStr);

            var azureDbHandler = new AzureDbHandler(fileLogger);
            string sqlConnection = "Server=tcp:tradinganalytics.database.windows.net;Database=7_tradesupporttest;User ID=All_ExceuteWriteLogin;Password=cn5QuJhsePLkPwKyR6zY;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;";
            azureDbHandler.ExecuteSql(insertStatement, sqlConnection);
            //return new OkObjectResult(requestBody == string.Empty ? "Post request was null" : requestBody);
            return new OkObjectResult(requestBodyAsStr);

        }
    }

    public class FileLogger 
    {

        public void Debug(string message)
        {
            var logMessage = $"[{DateTime.Now}] : " + message;
            if (Directory.Exists("C:\\home"))
            {
                var today = DateTime.Today.ToString("yyyyMMdd");
                File.AppendAllText($"C:\\home\\{today}_mavirLog.txt", logMessage + Environment.NewLine);
            }
        }
    }
}
