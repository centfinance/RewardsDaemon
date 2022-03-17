using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using static SymmetricRewardsCreator.Subgraph.PoolShares;

namespace SymmetricRewardsCreator.Subgraph
{
    // TODO: Move this out to a config file
    public enum Network
    {
        Celo,
        Gnosis
    }

    public class SymmetricPools
    {
        public List<PoolsType>? Pools { get; set; }
    }

    public class PoolsType
    {
        public string? id { get; set; }

        public bool publicSwap { get; set; }

        public double swapFee { get; set; }

        public string? controller { get; set; }

        public string? createTime { get; set; }

        public decimal totalShares { get; set; }

        public string? symbol { get; set; }

        public string? name { get; set; }

        public bool crp { get; set; }

        public bool finalized { get; set; }

        public decimal liquidity { get; set; }

        public decimal adjustedLiquidity { get; set; }

        public decimal adjustedPoolLiquidityPercent { get; set; }

        public double totalWeight { get; set; }

        public List<string>? tokensList { get; set; }

        public List<TokenType>? tokens { get; set; }

        public List<ShareType>? shares { get; set; }

        public decimal SYMM_Daily_Rewards { get; set; }

    }

    public class userAddressType
    {
        public string? id { get; set; }
    }

    public class TokenType
    {
        public string? id { get; set; }

        public string? symbol { get; set; }

        public string? name { get; set; }

        public int decimals { get; set; }

        public string? address { get; set; }

        public double balance { get; set; }

        public double denormWeight { get; set; }
    }

    public class Pools
    {
        public static async Task<SymmetricPools> GetAllPools(Network targetNetwork)
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

            var poolsRequest = new GraphQLRequest
            {
                Query = @"
                       {
                            pools (where: {liquidity_gt: 0} ) {
                               id
                               publicSwap
                               swapFee
                               controller
                               createTime
                               tokensList
                               totalShares
                               symbol
                               name
                               crp
                               finalized
                               liquidity
                               totalWeight
                               shares (first: 1000,where: {balance_gt: 0}, orderBy: balance, orderDirection: desc) {
                                   userAddress {
                                               id
                                   }
                                   balance
                               }
                               tokens {
                                   id
                                   symbol
                                   name
                                   decimals
                                   address
                                   balance
                                   denormWeight
                               }
                           }
                       }"
            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<SymmetricPools>(poolsRequest);
            return graphQLResponse.Data;
        }
    }
}
