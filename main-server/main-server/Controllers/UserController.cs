using coin_trader.Models;
using coin_trader.Repositories;
using coin_trader.Services;
using coin_trader.Models.DTO;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;




namespace coin_trader.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("id/{userId}")]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("name/{name}")] 
        public async Task<IActionResult> GetUserByNameAsync([FromRoute] string name)
        {
            var user = await _userService.GetUserByNameAsync(name);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost("create")] 
        public async Task<IActionResult> CreateUserAsync([FromBody] User user)
        {
            var createdUser = await _userService.CreateUserAsync(user);
            return Ok(createdUser);
        }

        [HttpPost("wallet/create")]
        public async Task<IActionResult> CreateWalletAsync([FromBody] CreateCoinWalletDTO createCoinWalletDTO)
        {
            var coinWallet = new CoinWallet
            {
                UserId = createCoinWalletDTO.UserId,
                CoinWalletPassword = createCoinWalletDTO.CoinWalletPassword,
            };

            var createdCoinWallet = await _userService.CreateWalletAsync(coinWallet);
            if (createdCoinWallet == null)
                return BadRequest();

            return Ok();
        }

        [HttpPost("wallet/deposit")]
        public async Task<IActionResult> DepositToWalletAsync([FromBody] DepositWalletDTO depositWalletDTO)
        {
            var deposited_coinWallet = await _userService.DepositToWalletAsync(depositWalletDTO);

            return Ok(deposited_coinWallet);

        }

        [HttpPost("wallet/withdraw")]
        public async Task<IActionResult> WithdrawFromWalletAsync([FromBody] WithdrawWalletDTO withdrawWalletDTO)
        {
            var withdrawn_coinWallet = await _userService.WithdrawFromWalletAsync(withdrawWalletDTO);
            return Ok(withdrawn_coinWallet);
        }
    }
}
