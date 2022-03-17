using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace SymmetricRewardsCreator.Subgraph
{
    public class PoolShares
    {
        public class SymmetricPoolShares
        {
            public List<ShareType>? poolShares { get; set; }
        }

        public class ShareType
        {
            public userAddressType? userAddress { get; set; }

            public decimal balance { get; set; }
        }

        /// <summary>
        /// Get a collection of all the pool shares for a specific pool
        /// </summary>
        /// <param name="targetNetwork">The target network for the current pool</param>
        /// <param name="poolId">The pool address</param>
        /// <returns>A list of shares from the current pool</returns>
        public static async Task<List<ShareType>> GetPoolShares(Network targetNetwork, string poolId)
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

            var poolSharesRequest = new GraphQLRequest
            {
                Query = @"
                query poolSharesQuery($queryPoolId: ID!) {
                    poolShares(first: 1000, where: {balance_gt: 0, poolId: $queryPoolId  }) {
                    id
                    userAddress {
                      id
                    }
                    poolId {
                      id
                    }
                    balance
                    }
                }",
                Variables = new
                {
                    queryPoolId = poolId.ToLower()
                }
            };
            var graphQLResponse = await graphQLClient.SendQueryAsync<SymmetricPoolShares>(poolSharesRequest);

            return graphQLResponse.Data.poolShares != null ? graphQLResponse.Data.poolShares : new List<ShareType>();
        }
    }
}
