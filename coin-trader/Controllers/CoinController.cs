using coin_trader.Models;
using coin_trader.Models.DTO;
using coin_trader.Repositories;
using coin_trader.Services;
using Microsoft.AspNetCore.Mvc;

namespace coin_trader.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CoinController : ControllerBase
    {
        private readonly ICoinService _coinService;

        public CoinController(ICoinService coinService)
        {
            _coinService = coinService;
        }

        [HttpGet("id/{coinId}")]
        public async Task<IActionResult> GetCoinByIdAsync([FromRoute] int coinId)
        {
            var coin = await _coinService.GetCoinByIdAsync(coinId);
            if (coin == null)
                return NotFound();
            return Ok(coin);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetCoinByNameAsync([FromRoute] string name)
        {
            var coin = await _coinService.GetCoinByNameAsync(name);
            if (coin == null)
                return NotFound();
            return Ok(coin);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCoinAsync([FromBody] Coin coin)
        {
            Console.WriteLine("coin name: " + coin.CoinName);
            var createdCoin = await _coinService.CreateCoinAsync(coin);
            return Ok(createdCoin);
        }

        [HttpGet("list")]
        public async Task<IActionResult> TestCoinList()
        {
            await _coinService.GetCoinListAsync();
            return Ok();
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseCoin([FromBody] PurchaseCoinDTO purchaseCoinDTO)
        {
            CoinWallet coinWallet = await _coinService.PurchaseCoinAsync(purchaseCoinDTO);

            if (coinWallet == null)
                return BadRequest();

            return Ok();
        }

        [HttpPost("sell")]
        public async Task<IActionResult> SellCoin([FromBody] SellCoinDTO sellCoinDTO)
        {
            CoinWallet coinWallet = await _coinService.SellCoinAsync(sellCoinDTO);
            if (coinWallet == null)
                return BadRequest();
            return Ok();
        }
    }
}
