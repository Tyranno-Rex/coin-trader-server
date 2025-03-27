using coin_trader.Models;
using Microsoft.EntityFrameworkCore;

namespace coin_trader.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly coin_trader_context _context;

        public UserRepository(coin_trader_context context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User> GetUserByNameAsync(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == name);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<CoinWallet> CreateWalletAsync(CoinWallet coinWallet)
        {
            _context.CoinWallets.Add(coinWallet);
            await _context.SaveChangesAsync();
            return coinWallet;
        }

        public async Task<CoinWallet> GetCoinWalletByUserIdAsync(int userId)
        {
            CoinWallet? coinWallet = await _context.CoinWallets
                    .Include(cw => cw.Coins) // Coins 관계 데이터 로드
                    .FirstOrDefaultAsync(cw => cw.UserId == userId);
            return coinWallet;
        }//return await _context.CoinWallets.FirstOrDefaultAsync(cw => cw.UserId == userId);

        public async Task<CoinWallet> UpdateCoinWalletAsync(CoinWallet coinWallet)
        {
            _context.CoinWallets.Update(coinWallet);
            await _context.SaveChangesAsync();
            return coinWallet;
        }
    }
}
