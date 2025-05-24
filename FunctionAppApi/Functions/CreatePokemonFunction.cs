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
    public class CreatePokemonFunction
    {
        private readonly Container _container;

        public CreatePokemonFunction(CosmosClient cosmosClient)
        {
            _container = cosmosClient.GetContainer("PokemonDb", "Pokemons");
        }

        [Function("CreatePokemon")]
        public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "pokemon")]
        HttpRequestData req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var requestBody = JsonSerializer.Deserialize<CreatePokemonDto>(body);

                if (requestBody is null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Missing or invalid 'text' field.");
                    return badRequest;
                }
                var pokemon = new Pokemon
                {
                    Id = requestBody.Id,
                    Name = requestBody.Name,
                    Type = requestBody.Type
                };

                var itemResponse = await _container.CreateItemAsync(pokemon);
                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(itemResponse.Resource);
                return response;
            }
            catch (CosmosException ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Cosmos DB error: {ex.Message}");
                return errorResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Unexpected error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}
