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
using System.Text.RegularExpressions;

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


            // MessageIdentification v="ANO20221115131504715"/> // From Ano.xml
            // DocumentIdentification v="ACKID354554"/> // From Ack.xml

            var identification = GetIdentificationFromXml(requestBodyAsStr, "DocumentIdentification") ??
                GetIdentificationFromXml(requestBodyAsStr, "MessageIdentification");

            if (string.IsNullOrEmpty(identification))
            {
                identification = "Did not find identification in data";
            }

            var sqlString = "INSERT INTO [Bulk].[Mavir_RawReplies]([Identification],[Data],[LastUpdatedUtc]) VALUES('{0}', '{1}', GETUTCDATE())";
            var insertStatement = string.Format(sqlString, identification, requestBodyAsStr);

            var azureDbHandler = new AzureDbHandler(fileLogger);
            string sqlConnection = "Server=tcp:tradinganalytics.database.windows.net;Database=7_tradesupporttest;User ID=All_ExceuteWriteLogin;Password=cn5QuJhsePLkPwKyR6zY;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;";
            azureDbHandler.ExecuteSql(insertStatement, sqlConnection);

            azureDbHandler.ExecuteStoredProcedure("[Mavir].[ImportBulkReplyData]", sqlConnection);
            azureDbHandler.ExecuteStoredProcedure("[Mavir].[CreateRawReply]", sqlConnection);


            //return new OkObjectResult(requestBody == string.Empty ? "Post request was null" : requestBody);
            return new OkObjectResult(requestBodyAsStr);

        }
        private static string GetIdentificationFromXml(string ackXml, string identificationKey)
        {
            string result = null;
            try
            {
                var regexMatch = Regex.Match(ackXml, $"{identificationKey}.*");
                if (regexMatch.Success)
                {

                    string key = regexMatch.Groups[0].Value;
                    if (!string.IsNullOrEmpty(key))
                    {
                        var valueRegex = Regex.Match(key, "(\".*\")/>");
                        if (valueRegex.Success)
                        {

                            result = valueRegex.Groups[0].Value;
                            result = result.Replace("\"", string.Empty);
                            var tempResult = result;

                            var firstIndeOf = tempResult.IndexOf(">");
                            if (firstIndeOf == -1)
                            {
                                return null;
                            }
                            result = tempResult.Substring(0, firstIndeOf - 1);
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                
            }
            return result;
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
