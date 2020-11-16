namespace Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using Parser;

    public class ProxyController : ControllerBase
    {
        private readonly ILogger<ProxyController> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public ProxyController(ILogger<ProxyController> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }


        [HttpPost("/proxy")]
        public async Task<dynamic> Proxy([FromBody] Request request)
        {
            dynamic parsedRequest;
            if (request.Params.Length > 1)
            {
                parsedRequest = new ExpandoObject() as IDictionary<string, dynamic>;
                foreach (var property in request.Params)
                {
                    var evaluatedRes = PropertyParser.ParsePropertyValue(property);
                    if (!string.IsNullOrWhiteSpace(property.Name))
                    {
                        parsedRequest.Add(property.Name, evaluatedRes);
                    }
                    else
                    {
                        throw new ArgumentException("invalid request, undefined name");
                    }
                }
            }
            else
            {
                parsedRequest = PropertyParser.ParsePropertyValue(request.Params[0]);
            }

            var reqBody = JsonSerializer.Serialize(parsedRequest);
            logger.LogInformation($"Got request: {JsonSerializer.Serialize(request)},{Environment.NewLine}Converted body to {reqBody}");
            
            var client = httpClientFactory.CreateClient("requester");
            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri(request.Url),
            };

            foreach (var header in request.Headers.Where(h => !h.Name.ToLower().Equals("content-type")))
            {
                httpRequest.Headers.Add(header.Name, header.Value);
            }

            var contentType = request.Headers.FirstOrDefault(h => h.Name.ToLower().Equals("content-type"))?.Value ?? "application/json"; 
            httpRequest.Content = new StringContent(reqBody,
                Encoding.UTF8,
                contentType);

            logger.LogInformation($"Calling {request.Url} with: {httpRequest}");
            var res = await client.SendAsync(httpRequest);
            logger.LogInformation($"received {JsonSerializer.Serialize(res)}");
            return res;
        }
    }
}