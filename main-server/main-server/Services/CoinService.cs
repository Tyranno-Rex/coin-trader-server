﻿
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
using System.Text;

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
            // Billing Server와 통신해서 처리하는 방식으로 수정함.

            var client = _httpClientFactory.CreateClient("BillingServer");
            string requestUri = "purchase";
            string json = JsonSerializer.Serialize(purchaseCoinDTO);

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("코인 구매에 실패했습니다.");
            }

            CoinWallet coinWallet = await _userRepository.GetCoinWalletByUserIdAsync(purchaseCoinDTO.UserId);
            return coinWallet;
        }

        public async Task<CoinWallet> SellCoinAsync(SellCoinDTO sellCoinDTO)
        {
            var client = _httpClientFactory.CreateClient("BillingServer");
            string requestUri = "sell";
            string json = JsonSerializer.Serialize(sellCoinDTO);

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("코인 판매에 실패했습니다.");
            }

            CoinWallet coinWallet = await _userRepository.GetCoinWalletByUserIdAsync(sellCoinDTO.UserId);
            return coinWallet;
        }
    }
}
