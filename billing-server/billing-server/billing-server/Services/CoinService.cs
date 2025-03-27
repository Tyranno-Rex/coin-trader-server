
using coin_trader.Models;
using coin_trader.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;

using coin_trader.Models.DTO;
using System.Formats.Asn1;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace coin_trader.Services
{
    public class CoinService : ICoinService
    {
        private readonly ICoinRepository _coinRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public CoinService(ICoinRepository coinRepository, IHttpClientFactory httpClientFactory, IUserRepository userRepository)
        {
            _coinRepository = coinRepository;
            _httpClientFactory = httpClientFactory;
            _userRepository = userRepository;
        }

        public async Task<Coin> GetCoinByIdAsync(int coinId)
        {
            return await _coinRepository.GetCoinByIdAsync(coinId);
        }

        public async Task<Coin> GetCoinByNameAsync(string name)
        {
            return await _coinRepository.GetCoinByNameAsync(name);
        }

        public async Task<CreateCoinDTO> CreateCoinAsync(Coin coin)
        {
            var client = _httpClientFactory.CreateClient("CoinGecko");

            string coinName = coin.CoinName;
            string currency = "usd";
            string requestUri = $"simple/price?ids={coinName}&vs_currencies={currency}";

            HttpResponseMessage response = await client.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("CoinGecko API 호출에 실패했습니다.");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var priceData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(jsonResponse);

            if (priceData != null && priceData.ContainsKey(coinName) && priceData[coinName].ContainsKey(currency))
            {
                decimal price = (decimal)priceData[coinName][currency];
                return new CreateCoinDTO(coin.CoinId, coin.CoinName, coin.CoinAmount, price, coin.CoinWalletId);
            }

            throw new Exception("CoinGecko API 호출 결과가 올바르지 않습니다.");
        }

        public async Task<decimal> GetCoinValueByNameAsync(string name)
        {
            var client = _httpClientFactory.CreateClient("CoinGecko");
            string currency = "usd";
            string requestUri = $"simple/price?ids={name}&vs_currencies={currency}";
            HttpResponseMessage response = await client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("CoinGecko API 호출에 실패했습니다.");
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var priceData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(jsonResponse);
            if (priceData != null && priceData.ContainsKey(name) && priceData[name].ContainsKey(currency))
            {
                decimal price = (decimal)priceData[name][currency];
                return price;
            }
            throw new Exception("CoinGecko API 호출 결과가 올바르지 않습니다.");
        }

        public async Task GetCoinListAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CoinGecko");
                string requestUri = "coins/list";

                HttpResponseMessage response = await client.GetAsync(requestUri);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"CoinGecko API 호출 실패. Status code: {response.StatusCode}");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("jsonResponse: " + jsonResponse);
                var coinDataList = JsonSerializer.Deserialize<List<CoinData>>(jsonResponse);

                if (coinDataList != null && coinDataList.Any()) // ✅ 데이터가 있는지 확인
                {
                    
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "CoinData.csv"); // ✅ 절대 경로 지정

                    using (var writer = new StreamWriter(filePath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(coinDataList);
                        writer.Flush(); // ✅ 즉시 파일에 기록
                    }

                    Console.WriteLine($"✅ CSV 저장 완료: {filePath}");
                }
                else
                {
                    Console.WriteLine("⚠️ 저장할 데이터가 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 오류 발생: {ex.Message}");
            }
        }

        public async Task<CoinWallet> PurchaseCoinAsync(PurchaseCoinDTO purchaseCoinDTO)
        {
            CoinWallet coinWallet = await _userRepository.GetCoinWalletByUserIdAsync(purchaseCoinDTO.UserId);
            if (coinWallet == null)
            {
                throw new Exception("지갑이 존재하지 않습니다.");
            }

            decimal coinValue = await GetCoinValueByNameAsync(purchaseCoinDTO.CoinName);
            decimal totalPrice = coinValue * purchaseCoinDTO.Amount;
            if (coinWallet.CoinWalletCash < totalPrice)
            {
                throw new Exception("잔액이 부족합니다.");
            }

            coinWallet.CoinWalletCash -= totalPrice;
            coinWallet.Coins.Add(new Coin
            {
                CoinName = purchaseCoinDTO.CoinName,
                CoinAmount = purchaseCoinDTO.Amount,
                CoinPrice = coinValue
            });

            await _userRepository.UpdateCoinWalletAsync(coinWallet);
            await _coinRepository.CreateCoinTransactionAsync(new CoinTransaction
            {
                UserId = purchaseCoinDTO.UserId,
                CoinName = purchaseCoinDTO.CoinName,
                Type = "purchase",
                Amount = purchaseCoinDTO.Amount,
                Price = coinValue,
                CreatedAt = DateTime.Now
            });

            return coinWallet;
        }

        public async Task<CoinWallet> SellCoinAsync(SellCoinDTO sellCoinDTO)
        {
            CoinWallet coinWallet = await _userRepository.GetCoinWalletByUserIdAsync(sellCoinDTO.UserId);
            if (coinWallet == null)
            {
                throw new Exception("지갑이 존재하지 않습니다.");
            }
            List<Coin> coins = coinWallet.Coins;
            Coin coin = coins.FirstOrDefault(c => c.CoinName == sellCoinDTO.CoinName);

            if (coin == null)
            {
                throw new Exception("해당 코인을 보유하고 있지 않습니다.");
            }
            
            decimal coinHaveAmount = coin.CoinAmount;
            decimal coinSellAmount = sellCoinDTO.Amount;

            if (coinHaveAmount < coinSellAmount)
            {
                throw new Exception("보유한 코인의 양이 부족합니다.");
            }

            // 전달 받은 amount는 코인의 가격이 아닌 판매할 코인의 양
            decimal coinValue = await GetCoinValueByNameAsync(sellCoinDTO.CoinName);
            decimal totalPrice = coinValue * coinSellAmount;


            coinWallet.CoinWalletCash += totalPrice;
            coin.CoinAmount -= coinSellAmount;

            if (coin.CoinAmount == 0)
            {
                coinWallet.Coins.Remove(coin);
                await _coinRepository.DeleteCoinAsync(coin.CoinId);
            }

            await _userRepository.UpdateCoinWalletAsync(coinWallet);
            await _coinRepository.CreateCoinTransactionAsync(new CoinTransaction
            {
                UserId = sellCoinDTO.UserId,
                CoinName = sellCoinDTO.CoinName,
                Type = "sell",
                Amount = coinSellAmount,
                Price = coinValue,
                CreatedAt = DateTime.Now
            });
            return coinWallet;
        }
    }
}
