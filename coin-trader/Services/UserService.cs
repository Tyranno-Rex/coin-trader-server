
using coin_trader.Repositories;
using coin_trader.Models;
using coin_trader.Models.DTO;

namespace coin_trader.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;        
        private readonly IHttpClientFactory _httpClientFactory;

        public UserService(IUserRepository userRepository, IHttpClientFactory httpClientFactory)
        {
            _userRepository = userRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<User> GetUserByNameAsync(string name)
        {
            return await _userRepository.GetUserByNameAsync(name);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<CoinWallet> CreateWalletAsync(CoinWallet coinWallet)
        {
            return await _userRepository.CreateWalletAsync(coinWallet);
        }

        public async Task<CoinWallet> GetCoinWalletByUserIdAsync(int userId)
        {
            return await _userRepository.GetCoinWalletByUserIdAsync(userId);
        }

        public async Task<CoinWallet> DepositToWalletAsync(DepositWalletDTO depositWalletDTO)
        {
            var user = await _userRepository.GetUserByIdAsync(depositWalletDTO.UserId);
            var coinWallet = await _userRepository.GetCoinWalletByUserIdAsync(depositWalletDTO.UserId);
            if (coinWallet == null || user == null)
                return null;

            // 유저의 cash가 충분한지 확인
            if (user.UserCash < depositWalletDTO.DepositAmount)
                return null;
            // 유저의 cash에서 충전할 금액을 뺌
            user.UserCash -= depositWalletDTO.DepositAmount;
            // 코인 지갑에 충전할 금액을 더함
            coinWallet.CoinWalletCash += depositWalletDTO.DepositAmount;

            // DB에 반영
            var updatedUser = await _userRepository.UpdateUserAsync(user);
            var updatedCoinWallet = await _userRepository.UpdateCoinWalletAsync(coinWallet);

            return coinWallet;
        }
    
        public async Task<CoinWallet> WithdrawFromWalletAsync(WithdrawWalletDTO withdrawWalletDTO)
        {
            var user = await _userRepository.GetUserByIdAsync(withdrawWalletDTO.UserId);
            var coinWallet = await _userRepository.GetCoinWalletByUserIdAsync(withdrawWalletDTO.UserId);
            if (coinWallet == null || user == null)
                return null;
            
            // 코인 지갑에 충전할 금액이 충분한지 확인
            if (coinWallet.CoinWalletCash < withdrawWalletDTO.WithdrawAmount)
                return null;
            // 코인 지갑에서 출금할 금액을 뺌
            coinWallet.CoinWalletCash -= withdrawWalletDTO.WithdrawAmount;
            // 유저의 cash에 출금할 금액을 더함
            user.UserCash += withdrawWalletDTO.WithdrawAmount;

            // DB에 반영
            var updatedUser = await _userRepository.UpdateUserAsync(user);
            var updatedCoinWallet = await _userRepository.UpdateCoinWalletAsync(coinWallet);
            return coinWallet;
        }
    }
}
