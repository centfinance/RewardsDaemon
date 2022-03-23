using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace SymmetricRewardsCreator.Subgraph;

public class SymmetricTokenPrices
{
    public List<TokenPricesType>? TokenPrices { get; set; }
}

public class TokenPricesType
{
    public string? id { get; set; }

    public string? symbol { get; set; }

    public double price { get; set; }

}

public class TokenPrices
{
    /// <summary>
    /// Get a collection of all the token prices for the current target network
    /// </summary>
    /// <param name="targetNetwork">The target network to retrieve pricing from</param>
    /// <returns>A collection of token prices</returns>
    public static async Task<SymmetricTokenPrices> GetAllTokenPrices(Network targetNetwork)
    {
        // Load subgraph data
        GraphQLHttpClient graphQLClient;

        switch (targetNetwork)
        {
            case Network.Celo:
                graphQLClient = new GraphQLHttpClient("https://api.thegraph.com/subgraphs/name/centfinance/symmetricv1celo", new NewtonsoftJsonSerializer());
                break;
            case Network.Gnosis:
            default:
                graphQLClient = new GraphQLHttpClient("https://api.thegraph.com/subgraphs/name/centfinance/symmetricv1gnosis", new NewtonsoftJsonSerializer());
                break;
        }

        var tokenPriceRequest = new GraphQLRequest
        {
            Query = @"
                {
                     tokenPrices {
                        id
                        symbol
                        price
                    }
                }"
        };

        var graphQLResponse = await graphQLClient.SendQueryAsync<SymmetricTokenPrices>(tokenPriceRequest);
        return graphQLResponse.Data;
    }
}
