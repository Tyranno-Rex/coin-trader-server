
using coin_trader.Models;
using coin_trader.Repositories;
using coin_trader.Models.DTO;

using Microsoft.AspNetCore.Mvc;

using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;



namespace coin_trader.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int coinId);
        Task<User> GetUserByNameAsync(string name);
        Task<User> CreateUserAsync(User user);
        Task<CoinWallet> CreateWalletAsync(CoinWallet coinWallet);
        Task<CoinWallet> GetCoinWalletByUserIdAsync(int userId);
        Task<CoinWallet> DepositToWalletAsync(DepositWalletDTO depositWalletDTO);
        Task<CoinWallet> WithdrawFromWalletAsync(WithdrawWalletDTO withdrawWalletDTO);
    }
}
