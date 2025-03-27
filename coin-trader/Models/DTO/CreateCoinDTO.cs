namespace coin_trader.Models.DTO
{
    public class CreateCoinDTO
    {
        int CoinId { get; set; }
        string CoinName { get; set; }
        decimal CoinAmount { get; set; }
        decimal CoinPrice { get; set; }
        int CoinWalletId { get; set; }

        public CreateCoinDTO(int coinId, string coinName, decimal coinAmount, decimal coinPrice, int coinWalletId)
        {
            CoinId = coinId;
            CoinName = coinName;
            CoinAmount = coinAmount;
            CoinPrice = coinPrice;
            CoinWalletId = coinWalletId;
        }
    }

    public class CoinData
    {
        public string id { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }

        public CoinData(string id, string symbol, string name)
        {
            this.id = id;
            this.symbol = symbol;
            this.name = name;
        }
    }

}
