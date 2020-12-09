using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WX.Api.Abstractions;
using WX.Api.Models;

namespace WX.Api.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISerializer _serializer;
        private readonly ISettings _settings;
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(
            IHttpClientFactory httpClientFactory,
            ISerializer serializer,
            ISettings settings,
            ILogger<ResourceService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _serializer = serializer;
            _settings = settings;
            _logger = logger;
        }

        public decimal GetTrolleyTotal(TrolleyRequest request)
        {
            _logger.LogInformation("=====================");
            _logger.LogInformation(_serializer.Serialize(request));

            var productLookups = request.Products.ToDictionary(x => x.Name, x => x);
            var specialLookups = new Dictionary<string, Dictionary<decimal, decimal>>();
            foreach (var special in request.Specials)
            {
                foreach (var quantityOnSpecial in special.Quantities)
                {
                    if (!specialLookups.TryGetValue(quantityOnSpecial.Name, out var quantityLookups))
                    {
                        quantityLookups = new Dictionary<decimal, decimal>();
                        specialLookups.Add(quantityOnSpecial.Name, quantityLookups);
                    }

                    quantityLookups.Add(quantityOnSpecial.Quantity, special.Total);
                }
            }

            var total = 0M;
            foreach (var quantity in request.Quantities)
            {
                if (!productLookups.TryGetValue(quantity.Name, out var product))
                {
                    continue;
                }

                var remainingItemCount = quantity.Quantity;
                if (specialLookups.TryGetValue(quantity.Name, out var specials))
                {
                    var orderedSpecials = specials
                        .Where(x => x.Key > 0)
                        .OrderBy(x => remainingItemCount % x.Key)
                        .ThenByDescending(x => x.Key);
                    foreach (var special in orderedSpecials)
                    {
                        if (remainingItemCount < special.Key)
                        {
                            continue;
                        }

                        var specialGroupCount = remainingItemCount / special.Key;

                        total += specialGroupCount * special.Value;
                        remainingItemCount -= specialGroupCount * special.Key;
                    }
                }

                total += product.Price * remainingItemCount;
            }
            return total;
        }

        public async Task<IEnumerable<Product>> ListProducts(SortOption sortOption)
        {
            var products = await LoadProductsFromResourceApi();

            if (sortOption == SortOption.Recommended)
            {
                var productLookups = products.ToDictionary(x => x.Name, x => x);

                var histories = await LoadShopperHistoriesResourceApi();

                foreach (var history in histories)
                {
                    foreach (var purchasedProduct in history.Products)
                    {
                        if (productLookups.TryGetValue(purchasedProduct.Name, out var product))
                        {
                            product.Quantity++;
                        }
                    }
                }

                return productLookups.Values.OrderByDescending(x => x.Quantity);
            }

            return sortOption switch
            {
                SortOption.Low => products.OrderBy(x => x.Price),
                SortOption.High => products.OrderByDescending(x => x.Price),
                SortOption.Ascending => products.OrderBy(x => x.Name),
                SortOption.Descending => products.OrderByDescending(x => x.Name),
                _ => products,
            };
        }

        private async Task<TData> Get<TData>(string uri)
        {
            uri = $"{uri}?token={_settings.Token}";

            var client = _httpClientFactory.CreateClient();

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
            };

            var response = await client.SendAsync(httpRequestMessage);

            var dataJson = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var data = _serializer.Deserialize<TData>(dataJson);

            return data;
        }

        private async Task<List<Product>> LoadProductsFromResourceApi()
        {
            var products = await Get<List<Product>>(_settings.ProductsUri);
            return products;
        }

        private async Task<List<ShopperHistory>> LoadShopperHistoriesResourceApi()
        {
            var histories = await Get<List<ShopperHistory>>(_settings.ShopperHistoryUri);
            return histories;
        }
    }
}