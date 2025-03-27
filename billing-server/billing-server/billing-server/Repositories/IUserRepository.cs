using System.Threading.Tasks;
using coin_trader.Models;

namespace coin_trader.Repositories
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByNameAsync(string name);
        Task<User> UpdateUserAsync(User user);

        Task<CoinWallet> CreateWalletAsync(CoinWallet coinWallet);
        Task<CoinWallet> GetCoinWalletByUserIdAsync(int userId);
        Task<CoinWallet> UpdateCoinWalletAsync(CoinWallet coinWallet);
    }
}
