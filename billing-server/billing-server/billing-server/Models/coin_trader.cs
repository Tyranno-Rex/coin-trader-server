using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace coin_trader.Models
{
    public class coin_trader_context : DbContext
    {
        public coin_trader_context(DbContextOptions<coin_trader_context> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CoinTransaction> CoinTransactions { get; set; }
        public DbSet<CoinWallet> CoinWallets { get; set; }
        public DbSet<Coin> Coins { get; set; }
    }

    public class User
    {
        [Key] // 기본 키 설정
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 자동 생성 설정
        public int UserId { get; set; }

        [Required]
        public required string UserName { get; set; }

        [Required]
        public decimal UserCash { get; set; }

        // 1:1 관계 (User -> CoinWallet)
        public CoinWallet? CoinWallet { get; set; }
    }

    public class CoinTransaction
    {
        [Key] // 기본 키 설정
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CoinTransactionId { get; set; }

        // 외래 키 설정
        [ForeignKey("User")]
        public int UserId { get; set; }

        public User? User { get; set; } // Navigation Property

        [Required]
        public required string CoinName { get; set; }

        [Required]
        public required string Type { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }

    public class CoinWallet
    {
        [Key] // 기본 키 설정
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CoinWalletId { get; set; }

        [Required]
        public required string CoinWalletPassword { get; set; }

        // 외래 키 설정
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public decimal CoinWalletCash { get; set; }

        // 1:1 관계 (CoinWallet -> User)
        [JsonIgnore]
        public User? User { get; set; } // Navigation Property

        // 1:N 관계 (CoinWallet -> Coin)
        public List<Coin> Coins { get; set; } = new List<Coin>();
    }

    public class Coin
    {
        [Key] // 기본 키 설정
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CoinId { get; set; }

        [Required]
        public required string CoinName { get; set; }

        [Required]
        public decimal CoinAmount { get; set; }

        public decimal CoinPrice { get; set; } = 0;

        // 외래 키 설정
        [ForeignKey(nameof(CoinWallet))]
        public int CoinWalletId { get; set; }

        public CoinWallet? CoinWallet { get; set; }
    }
}
