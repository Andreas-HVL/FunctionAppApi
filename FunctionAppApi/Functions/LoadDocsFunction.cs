using FunctionAppApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FunctionAppApi.Functions
{
    public class LoadDocsFunction
    {
        private readonly Container _container;
        public LoadDocsFunction(CosmosClient cosmosClient)
        {
            _container = cosmosClient.GetContainer("PokemonDb", "Pokemons");
        }
        [Function("ServeDocsHomepage")]
        public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "docs")]
        HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var docsPath = Path.Combine(Environment.CurrentDirectory, "docs.html");
            var html = await File.ReadAllTextAsync(docsPath);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await response.WriteStringAsync(html);
            return response;
        }
    }
}
