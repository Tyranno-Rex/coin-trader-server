using coin_trader.Models;
using Microsoft.EntityFrameworkCore;

namespace coin_trader.Repositories
{
    public class CoinRepository : ICoinRepository
    {
        private readonly coin_trader_context _context;

        public CoinRepository(coin_trader_context context)
        {
            _context = context;
        }

        public async Task<Coin> CreateCoinAsync(Coin coin)
        {
            _context.Coins.Add(coin);
            await _context.SaveChangesAsync();
            return coin;
        }

        public async Task<Coin> GetCoinByIdAsync(int coinId)
        {
            return await _context.Coins.FindAsync(coinId);
        }

        public Task<Coin> GetCoinByNameAsync(string name)
        {
            return _context.Coins.FirstOrDefaultAsync(c => c.CoinName == name);
        }

        public async Task<bool> DeleteCoinAsync(int coinId)
        {
            Coin coin = await _context.Coins.FindAsync(coinId);
            if (coin == null)
                return false;
            _context.Coins.Remove(coin);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
