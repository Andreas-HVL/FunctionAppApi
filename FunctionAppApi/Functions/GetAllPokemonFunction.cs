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
    public class GetAllPokemonFunction
    {
        private readonly Container _container;

        public GetAllPokemonFunction(CosmosClient cosmosClient)
        {
            _container = cosmosClient.GetContainer("PokemonDb", "Pokemons");
        }
        [Function("GetAllPokemon")]
        public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "pokemon")]
        HttpRequestData req)
        {
            List<Pokemon> pokemon = [];

            try
            {
                using var iterator = _container.GetItemQueryIterator<Pokemon>("SELECT * FROM c");
                while (iterator.HasMoreResults)
                {
                    var feedResponse = await iterator.ReadNextAsync();
                    pokemon.AddRange(feedResponse.Resource);
                }
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(pokemon);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error retrieving pokemon: {ex.Message}");
                return errorResponse;
            }
        }
    }
}
