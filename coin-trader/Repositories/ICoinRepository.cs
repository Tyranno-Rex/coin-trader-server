using System.Threading.Tasks;
using coin_trader.Models;

namespace coin_trader.Repositories
{
    public interface ICoinRepository
    {
        Task<Coin> GetCoinByIdAsync(int coinId);
        Task<Coin> GetCoinByNameAsync(string name);
        Task<Coin> CreateCoinAsync(Coin coin);
        Task<Boolean> DeleteCoinAsync(int coinId);
    }
}
