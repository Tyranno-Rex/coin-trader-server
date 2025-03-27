using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

using coin_trader.Models;
using coin_trader.Repositories;
using coin_trader.Services;

namespace coin_trader
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // appsettings.json 또는 환경변수에 저장된 CoinGecko API 키 사용 (예: "CoinGecko:ApiKey")
            string coinGeckoApiKey = Configuration["CoinGecko:ApiKey"];
            if (string.IsNullOrEmpty(coinGeckoApiKey))
            {
                throw new System.Exception("CoinGecko API 키가 설정되지 않았습니다.");
            }
            // HttpClient 등록 (필요한 경우 API 키를 헤더에 포함)
            services.AddHttpClient("CoinGecko", client =>
            {
                client.BaseAddress = new System.Uri("https://api.coingecko.com/api/v3/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                // 만약 CoinGecko API에서 API 키를 필요로 한다면 헤더에 추가 (헤더명은 실제 API 문서 확인 필요)
                if (!string.IsNullOrEmpty(coinGeckoApiKey))
                {
                    client.DefaultRequestHeaders.Add("X-CoinGecko-Api-Key", coinGeckoApiKey);
                }
            });

            // Billing Server와 통신하기 위한 HttpClient 등록
            services.AddHttpClient("BillingServer", client =>
            {
                client.BaseAddress = new System.Uri("https://localhost:35771/api/v1/coin/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddDbContext<coin_trader_context>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new System.Version(8, 0, 40))));

            // Repository 등록
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICoinRepository, CoinRepository>();

            // Service 등록
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICoinService, CoinService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddRazorPages();
        }
    
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }
            else
            {
                _ = app.UseExceptionHandler("/Error");
                _ = app.UseHsts();
            }
            _ = app.UseHttpsRedirection();
            _ = app.UseStaticFiles();
            _ = app.UseCookiePolicy();
            _ = app.UseRouting();
            _ = app.UseAuthorization();
            _ = app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // API 컨트롤러 자동 매핑
            });
        }

    }
}
