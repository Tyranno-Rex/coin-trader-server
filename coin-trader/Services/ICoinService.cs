
using coin_trader.Models;
using coin_trader.Repositories;
using coin_trader.Models.DTO;

using Microsoft.AspNetCore.Mvc;

using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;



namespace coin_trader.Services
{
    public interface ICoinService
    {
        Task<Coin> GetCoinByIdAsync(int coinId);
        Task<Coin> GetCoinByNameAsync(string name);
        Task<CreateCoinDTO> CreateCoinAsync(Coin coin);
        Task GetCoinListAsync();
        Task<CoinWallet> PurchaseCoinAsync(PurchaseCoinDTO purchaseCoinDTO);
        Task<CoinWallet> SellCoinAsync(SellCoinDTO sellCoinDTO);
    }
}
