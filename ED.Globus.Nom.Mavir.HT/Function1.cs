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
          
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogDebug(requestBody);
            if (string.IsNullOrEmpty(requestBody))
            {
                fileLogger.Debug("request was empty when read to end");
            }
            else
            {
                fileLogger.Debug(requestBody);
            }

            //return new OkObjectResult(requestBody == string.Empty ? "Post request was null" : requestBody);
            return new OkObjectResult(requestBody);

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
