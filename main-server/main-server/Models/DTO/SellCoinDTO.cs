namespace coin_trader.Models.DTO
{
    public class SellCoinDTO
    {
        public int UserId { get; set; }
        public string CoinName { get; set; }
        public decimal Amount { get; set; }
    }
}
