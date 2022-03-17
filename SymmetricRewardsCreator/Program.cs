using Microsoft.Extensions.Configuration;
using SymmetricRewardsCreator;
using SymmetricRewardsCreator.Subgraph;
using System.Data.SqlClient;
using System.Timers;

namespace Symmetric.CorrectionNS
{
    public class ResponseType
    {
        public PoolType? Pool { get; set; }
    }

    public class PoolType
    {
        public string? id { get; set; }
    }

    public class Wallet
    {
        public string? Address { get; set; }

        public decimal Rewards { get; set; }

        public Network network { get; set; }
    }

    class Program
    {
        // TODO: Move this out to a config file
        public static decimal SYMM_Daily_Rewards { get { return (decimal)15.20833333333333; } }  // 365 SYMM tokens per day

        private static Tokens? balTokenSet { get; set; }

        private static IConfigurationRoot? Config { get; set; }

        public static void Main(string[] args)
        {
            Config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

            // TODO: Move this out to a config file
            var aTimer = new System.Timers.Timer(60 * 60 * 1000); //one hour in milliseconds (add * 60)
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

            aTimer.Start();
            while (true)
            {
            }
        }

        private static async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            checked // for overflow
            {
                // Get all pools from Subgraph
                var rewardPoolsCelo = await Pools.GetAllPools(Network.Celo);
                var rewardPoolsGnosis = await Pools.GetAllPools(Network.Gnosis);

                if (rewardPoolsCelo != null &&
                    rewardPoolsGnosis != null)
                {
                    decimal celoTVL = 0;
                    decimal gnosisTVL = 0;

                    // Calculate TVL for each network
                    if (rewardPoolsCelo.Pools != null)
                    {
                        foreach (var pool in rewardPoolsCelo.Pools)
                        {
                            celoTVL += pool.liquidity;
                        }
                    }

                    if (rewardPoolsGnosis.Pools != null)
                    {
                        foreach (var pool in rewardPoolsGnosis.Pools)
                        {
                            gnosisTVL += pool.liquidity;
                        }
                    }

                    // Get latest token prices for all tokens from Subgraph
                    var tokenPricesCelo = await TokenPrices.GetAllTokenPrices(Network.Celo);
                    var tokenPricesGnosis = await TokenPrices.GetAllTokenPrices(Network.Gnosis);

                    List<Wallet> users = new();

                    decimal totalAdjustedLiquidity = 0;
                    decimal totalliquidity = 0;

                    if (rewardPoolsCelo.Pools != null)
                    {
                        // Loop through each pool, calculate the adjusted pool liquidity and record it
                        foreach (var pool in rewardPoolsCelo.Pools)
                        {
                            pool.adjustedLiquidity = CalculateAdjustedLiquidity(pool, Network.Celo);
                            totalAdjustedLiquidity += pool.adjustedLiquidity;
                            totalliquidity += pool.liquidity;
                        }
                    }

                    if (rewardPoolsGnosis.Pools != null)
                    {
                        foreach (var pool in rewardPoolsGnosis.Pools)
                        {
                            pool.adjustedLiquidity = CalculateAdjustedLiquidity(pool, Network.Gnosis);
                            totalAdjustedLiquidity += pool.adjustedLiquidity;
                            totalliquidity += pool.liquidity;
                        }
                    }

                    if (rewardPoolsCelo.Pools != null)
                    {
                        foreach (var pool in rewardPoolsCelo.Pools)
                        {
                            pool.adjustedPoolLiquidityPercent = pool.adjustedLiquidity / totalAdjustedLiquidity;
                            pool.SYMM_Daily_Rewards = SYMM_Daily_Rewards * pool.adjustedPoolLiquidityPercent;

                            if (pool.shares != null)
                            {
                                foreach (var wallet in pool.shares)
                                {
                                    if (wallet.userAddress != null)
                                    {
                                        var thisWallet = users.FirstOrDefault(x => x.Address == wallet.userAddress.id && x.network == Network.Celo);
                                        if (thisWallet != null)
                                        {
                                            thisWallet.Rewards += (wallet.balance / pool.totalShares) * pool.SYMM_Daily_Rewards;
                                        }
                                        else
                                        {
                                            Wallet userWallet = new();
                                            userWallet.Address = wallet.userAddress != null ? wallet.userAddress.id : "0x0";
                                            userWallet.network = Network.Celo;
                                            userWallet.Rewards = (wallet.balance / pool.totalShares) * pool.SYMM_Daily_Rewards;
                                            users.Add(userWallet);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (rewardPoolsGnosis.Pools != null)
                    {
                        foreach (var pool in rewardPoolsGnosis.Pools)
                        {
                            pool.adjustedPoolLiquidityPercent = pool.adjustedLiquidity / totalAdjustedLiquidity;
                            pool.SYMM_Daily_Rewards = SYMM_Daily_Rewards * pool.adjustedPoolLiquidityPercent;

                            if (pool.shares != null)
                            {
                                foreach (var wallet in pool.shares)
                                {
                                    if (wallet.userAddress != null)
                                    {
                                        var thisWallet = users.FirstOrDefault(x => x.Address == wallet.userAddress.id && x.network == Network.Gnosis);
                                        if (thisWallet != null)
                                        {
                                            thisWallet.Rewards += (wallet.balance / pool.totalShares) * pool.SYMM_Daily_Rewards;
                                        }
                                        else
                                        {
                                            Wallet userWallet = new();
                                            userWallet.Address = wallet.userAddress != null ? wallet.userAddress.id : "0x0";
                                            userWallet.network = Network.Gnosis;
                                            userWallet.Rewards = (wallet.balance / pool.totalShares) * pool.SYMM_Daily_Rewards;
                                            users.Add(userWallet);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    RecordRewards(users);
                }
            }
        }

        private static void RecordRewards(List<Wallet> users)
        {
            SqlConnectionStringBuilder builder = new();

            builder.DataSource = Config.GetSection("RewardsDatabase:DataSource").Value;
            builder.UserID = Config.GetSection("RewardsDatabase:UserID").Value;
            builder.Password = Config.GetSection("RewardsDatabase:Password").Value;
            builder.InitialCatalog = Config.GetSection("RewardsDatabase:InitialCatalog").Value;

            using (SqlConnection connection = new(builder.ConnectionString))
            {
                connection.Open();

                foreach (Wallet user in users)
                {
                    if (user.Rewards > 0)
                    {
                        int chainId = 1;

                        switch (user.network)
                        {
                            case Network.Celo:
                                chainId = 42220;
                                break;
                            case Network.Gnosis:
                            default:
                                chainId = 100;
                                break;
                        }
                        String sql = string.Format("Set DateFormat YMD INSERT INTO dbo.Rewards (ChainId, Address, Amount, RewardDate) VALUES ({0}, '{1}', {2}, '{3}')", chainId, user.Address, user.Rewards, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        using (SqlCommand commandThree = new(sql, connection))
                        {
                            commandThree.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static decimal CalculateAdjustedLiquidity(PoolsType pool, Network network)
        {
            if (pool.tokens != null)
            {
                checked // for overflow
                {
                    int chainId = 1;
                    switch (network)
                    {
                        case Network.Celo:
                            chainId = 42220;
                            break;
                        case Network.Gnosis:
                        default:
                            chainId = 100;
                            break;
                    }

                    List<double> weights = new();

                    foreach (var token in pool.tokens)
                    {
                        weights.Add(token.denormWeight / pool.totalWeight);
                    }

                    var feeFactor = GetFeeFactor(pool.swapFee);
                    var ratioFactor = ComputeRatioFactor(pool, weights, chainId, 2);
                    var wrapFactor = ComputeWrapFactor(pool, weights, chainId);

                    var thisCombinedAdjustmentFactor = feeFactor
                        * ratioFactor
                        * wrapFactor;

                    var thisAdjustedPoolLiquidity = pool.liquidity * (decimal)thisCombinedAdjustmentFactor;

                    return thisAdjustedPoolLiquidity;
                }
            }
            else
            {
                return 0;
            }
        }

        private static double GetFeeFactor(double swapFee)
        {
            checked // for overflow
            {
                var FEE_FACTOR_K = 0.25;

                var exp = Math.Pow((FEE_FACTOR_K * swapFee * 100), 2);
                return Math.Exp(-exp);
            }
        }

        private static double ComputeRatioFactor(PoolsType pool, List<double> weights, int chainId, double balMultiplier)
        {
            checked // for overflow
            {
                double brfSum = 0;
                double pairWeightSum = 0;

                var N = weights.Count;
                for (var j = 0; j < N; j++)
                {
                    if (weights[j] == 0)
                        continue;

                    for (var k = j + 1; k < N; k++)
                    {
                        var pairWeight = weights[j] * weights[k];
                        var normalizedWeight1 = weights[j] / weights[j] + weights[k];
                        var normalizedWeight2 = weights[k] / weights[j] + weights[k];

                        if (pool.tokens != null &&
                            pool.tokens[j].address != null &&
                            pool.tokens[k].address != null)
                        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            var stakingBoostOfPair = GetStakingBoostOfPair(
                              chainId,
                              balMultiplier,
                              pool.tokens[j].address.ToLower(),
                              weights[j],
                              pool.tokens[k].address.ToLower(),
                              weights[k]
                            );
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                            // stretches factor for equal weighted pairs to 1
                            double ratioFactorOfPair = 4
                              * normalizedWeight1
                              * normalizedWeight2
                              * pairWeight;

                            var brfOfPair = stakingBoostOfPair * ratioFactorOfPair;

                            brfSum = brfSum + brfOfPair;
                            pairWeightSum = pairWeightSum + pairWeight;
                        }
                    }
                }

                return brfSum / pairWeightSum;
            }
        }

        private static double GetStakingBoostOfPair(
          int chainId,
          double balMultiplier,
          string token1,
          double weight1,
          string token2,
          double weight2
        )
        {
            checked // for overflow
            {
                balTokenSet = new Tokens();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (
                  balTokenSet.BAL_TOKEN[chainId] != null &&
                  token1 == balTokenSet.BAL_TOKEN[chainId].ToString().ToLower() &&
                  balTokenSet.UncappedTokens.FirstOrDefault(b => b.NetworkId == chainId).Tokens.Contains(token2)
                )
                {
                    return balMultiplier
                      * weight1
                      + weight2
                      / (weight1 + weight2);
                }
                else if (
                balTokenSet.BAL_TOKEN[chainId] != null &&
                token2 == balTokenSet.BAL_TOKEN[chainId].ToString().ToLower() &&
                balTokenSet.UncappedTokens.FirstOrDefault(b => b.NetworkId == chainId).Tokens.Contains(token1)
              )
                {
                    return weight1
                      + (balMultiplier * weight2)
                      / (weight1 + weight2);
                }
                else
                {
                    return 1;
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        private static double ComputeWrapFactor(PoolsType pool, List<double> weights, int chainId)
        {
            checked // for overflow
            {
                double wrapFactorSum = 0;
                double pairWeightSum = 0;
                int N = weights.Count;

                for (var x = 0; x < N; x++)
                {
                    if (weights[x] != 0)
                    {
                        for (var y = x + 1; y < N; y++)
                        {
                            var pairWeight = weights[x] * weights[y];

                            if (pool.tokens != null &&
                                pool.tokens[x] != null)
                            {
                                double wrapFactorPair = GetWrapFactorOfPair(
                                  pool.tokens[x],
                                  pool.tokens[y],
                                  chainId
                                );
                                wrapFactorSum += (wrapFactorPair * pairWeight);
                                pairWeightSum += pairWeight;
                            }
                        }
                    }
                }

                return wrapFactorSum / pairWeightSum;
            }
        }

        private static double GetWrapFactorOfPair(TokenType tokenA, TokenType tokenB, int chainId)
        {
            checked // for overflow
            {
                double WRAP_FACTOR_HARD = 0.1;
                double WRAP_FACTOR_SOFT = 0.2;

                bool foundTokenA = false;
                bool foundTokenB = false;
                bool wrap_factor_hard = false;

                if (balTokenSet != null)
                {
                    var thisTokenSet = balTokenSet.EquivalentSets.FirstOrDefault(balTokenSet => balTokenSet.NetworkId == chainId);

                    if (thisTokenSet != null &&
                        thisTokenSet.TokenSet != null)
                    {
                        foreach (var set in thisTokenSet.TokenSet)
                        {
                            if (tokenA.address != null &&
                                set.Tokens != null &&
                                set.Tokens.Contains(tokenA.address.ToLower()))
                            {
                                foundTokenA = true;
                            }

                            if (tokenB.address != null &&
                                set.Tokens != null &&
                                set.Tokens.Contains(tokenB.address.ToLower()))
                            {
                                foundTokenB = true;
                            }

                            if (set.Tokens != null &&
                                tokenA.address != null &&
                                tokenB.address != null &&
                                set.Tokens.Contains(tokenA.address) && set.Tokens.Contains(tokenB.address.ToLower()))
                            {
                                wrap_factor_hard = true;
                            }
                        }
                    }
                    if (wrap_factor_hard)
                        return WRAP_FACTOR_HARD;

                    if (foundTokenA && foundTokenB)
                        return WRAP_FACTOR_SOFT;
                }

                return 1.0;
            }
        }
    }
}