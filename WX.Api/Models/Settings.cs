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

        public string Url => _configuration.GetSection(nameof(Url)).Get<string>();
        public string Name => _configuration.GetSection(nameof(Name)).Get<string>();
        public string Token => _configuration.GetSection(nameof(Token)).Get<string>();
    }
}