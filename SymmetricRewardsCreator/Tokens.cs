using System.Collections;

namespace SymmetricRewardsCreator
{
    public class TokenSet
    {
        public int NetworkId { get; set; }

        public List<string>? Tokens { get; set; }
    }

    public class TokenEquivalentSet
    {
        public int NetworkId { get; set; }

        public List<TokenSet>? TokenSet { get; set; }
    }


    public class Tokens
    {
        public Hashtable BAL_TOKEN { get; set; }

        public List<TokenSet> UncappedTokens { get; set; }

        public List<TokenEquivalentSet> EquivalentSets { get; set; }

        // TODO: Move this out to a config file
        public Tokens()
        {
            BAL_TOKEN = new();
            BAL_TOKEN.Add(1, "0x57dB3FfCa78dBbE0eFa0EC745D55f62aa0Cbd345");
            BAL_TOKEN.Add(42, "0xfB66054D9C7b357b3134Dc47eD54EddAcc012f44");
            BAL_TOKEN.Add(100, "0xC45b3C1c24d5F54E7a2cF288ac668c74Dd507a84");
            BAL_TOKEN.Add(42220, "0x7c64ad5f9804458b8c9f93f7300c15d55956ac2a");

            UncappedTokens = new List<TokenSet>();

            TokenSet ts = new();

            ts = new();
            ts.Tokens = new();
            ts.NetworkId = 100;
            ts.Tokens.Add("0x6a023ccd1ff6f2045c3309768ead9e68f978f6e1".ToLower()); // WETH
            ts.Tokens.Add("0xc45b3c1c24d5f54e7a2cf288ac668c74dd507a84".ToLower()); // SYMM
            ts.Tokens.Add("0x44fa8e6f47987339850636f88629646662444217".ToLower()); // DAI
            ts.Tokens.Add("0xddafbb505ad214d7b80b1f830fccc89b60fb7a83".ToLower()); // USDC
            ts.Tokens.Add("0x8e5bbbb09ed1ebde8674cda39a0c169401db4252".ToLower()); // WBTC
            ts.Tokens.Add("0xe91d153e0b41518a2ce8dd3d7944fa863463a97d".ToLower()); // WXDAI
            ts.Tokens.Add("0x2bf2ba13735160624a0feae98f6ac8f70885ea61".ToLower()); // FRACTION
            ts.Tokens.Add("0x3a97704a1b25f08aa230ae53b352e2e72ef52843".ToLower()); // AGVE
            ts.Tokens.Add("0x4ecaba5870353805a9f068101a40e0f32ed605c6".ToLower()); // USDT
            ts.Tokens.Add("0x63e62989d9eb2d37dfdb1f93a22f063635b07d51".ToLower()); // MIVA
            ts.Tokens.Add("0x6b0f8a3fb7cb257ad7c72ada469ba1d3c19c5094".ToLower()); // RXDAI
            ts.Tokens.Add("0x71850b7e9ee3f13ab46d67167341e4bdc905eef9".ToLower()); // HNY
            ts.Tokens.Add("0xb7d311e2eb55f2f68a9440da38e7989210b9a05e".ToLower()); // STAKE
            ts.Tokens.Add("0xd51e1ddd116fff9a71c1b8feeb58113afa2b4d93".ToLower()); // AMIS
            ts.Tokens.Add("0xd87fcb23da48d4d9b70c6f39b46debb5d993ad19".ToLower()); // HBTC
            ts.Tokens.Add("0x22bd2a732b39dace37ae7e8f50a186f3d9702e87".ToLower()); // yDAI+yUSDC+yUSDT+yTUSD
            UncappedTokens.Add(ts);

            ts = new();
            ts.Tokens = new();
            ts.NetworkId = 42220;
            ts.Tokens.Add("0x2def4285787d58a2f811af24755a8150622f4361".ToLower()); // cETH
            ts.Tokens.Add("0x471ece3750da237f93b8e339c536989b8978a438".ToLower()); // CELO
            ts.Tokens.Add("0xd8763cba276a3738e6de85b4b3bf5fded6d6ca73".ToLower()); // cEUR
            ts.Tokens.Add("0x765de816845861e75a25fca122bb6898b8b1282a".ToLower()); // cUSD
            ts.Tokens.Add("0xd629eb00deced2a080b7ec630ef6ac117e614f1b".ToLower()); // cBTC
            ts.Tokens.Add("0xa8d0e6799ff3fd19c6459bf02689ae09c4d78ba7".ToLower()); // mcEUR
            ts.Tokens.Add("0x64defa3544c695db8c535d289d843a189aa26b98".ToLower()); // mcUSD
            ts.Tokens.Add("0x00be915b9dcf56a3cbe739d9b9c202ca692409ec".ToLower()); // UBE
            ts.Tokens.Add("0x1a8dbe5958c597a744ba51763abebd3355996c3e".ToLower()); // rCELO
            ts.Tokens.Add("0x7037f7296b2fc7908de7b57a89efaa8319f0c500".ToLower()); // mCELO
            ts.Tokens.Add("0x7c64ad5f9804458b8c9f93f7300c15d55956ac2a".ToLower()); // SYMM
            UncappedTokens.Add(ts);

            EquivalentSets = new();

            TokenEquivalentSet newTokenEquivalentSet = new();
            newTokenEquivalentSet.NetworkId = 100;
            newTokenEquivalentSet.TokenSet = new List<TokenSet>();

            ts = new();
            ts.Tokens = new();
            ts.NetworkId = 100;
            ts.Tokens.Add("0xDDAfbb505ad214D7b80b1f830fcCc89B60fb7A83".ToLower()); // USDC
            ts.Tokens.Add("0xe91D153E0b41518A2Ce8Dd3D7944Fa863463a97d".ToLower()); // WXDAI
            newTokenEquivalentSet.TokenSet.Add(ts);
            EquivalentSets.Add(newTokenEquivalentSet);

            newTokenEquivalentSet = new();
            newTokenEquivalentSet.NetworkId = 42220;
            newTokenEquivalentSet.TokenSet = new List<TokenSet>();

            ts = new();
            ts.Tokens = new();
            ts.NetworkId = 42220;
            ts.Tokens.Add("0xD629eb00dEced2a080B7EC630eF6aC117e614f1b".ToLower()); // cBTC
            ts.Tokens.Add("0xBe50a3013A1c94768A1ABb78c3cB79AB28fc1aCE".ToLower()); // WBTC
            newTokenEquivalentSet.TokenSet.Add(ts);

            ts = new();
            ts.Tokens = new();
            ts.NetworkId = 42220;
            ts.Tokens.Add("0x2DEf4285787d58a2f811AF24755A8150622f4361".ToLower()); // cETH
            ts.Tokens.Add("0xE919F65739c26a42616b7b8eedC6b5524d1e3aC4".ToLower()); // WETH
            newTokenEquivalentSet.TokenSet.Add(ts);

            EquivalentSets.Add(newTokenEquivalentSet);
        }
    }
}
