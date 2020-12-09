using Microsoft.Extensions.Configuration;
using WX.Api.Abstractions;

namespace WX.Api.Models
{
    public class Settings : ISettings
    {
        private readonly IConfiguration _configuration;

        public Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Name => _configuration.GetSection(nameof(Name)).Get<string>();
        public string ProductsUri => _configuration.GetSection(nameof(ProductsUri)).Get<string>();
        public string ShopperHistoryUri => _configuration.GetSection(nameof(ShopperHistoryUri)).Get<string>();
        public string Token => _configuration.GetSection(nameof(Token)).Get<string>();
    }
}