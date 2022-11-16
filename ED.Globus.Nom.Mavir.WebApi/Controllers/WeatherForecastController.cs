using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ED.Globus.Nom.Mavir.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        [HttpPost]
        [Consumes("application/xml", "application/json")]
        [Route("mekToPartner")]
        public string Get([FromBody] string something)
        {
            
            
            return "Replied: " + something;
        }
    }

    public class DealInsertXmlFormatter : XmlSerializerInputFormatter
    {
        private const string ContentType = "application/xml";
        public DealInsertXmlFormatter(MvcOptions options) : base(options)
        {
            SupportedMediaTypes.Add(ContentType);
        }
        public override bool CanRead(InputFormatterContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;
            return contentType.StartsWith(ContentType);
        }
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            StreamReader reader = new StreamReader(context.HttpContext.Request.Body);
            string text = await reader.ReadToEndAsync();
            return await InputFormatterResult.SuccessAsync(text);
        }
    }
}
