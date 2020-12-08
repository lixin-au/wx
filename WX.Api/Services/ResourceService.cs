using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WX.Api.Abstractions;
using WX.Api.Models;

namespace WX.Api.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISerializer _serializer;
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(
            IHttpClientFactory httpClientFactory,
            ISerializer serializer,
            ILogger<ResourceService> logger
            )
        {
            _httpClientFactory = httpClientFactory;
            _serializer = serializer;
            _logger = logger;

            _logger.LogInformation($"{nameof(ResourceService)} started.");
        }

        public async Task<IEnumerable<Product>> ListProducts(SortOption sortOption)
        {
            var products = await Task.FromResult(
                _serializer.Deserialize<List<Product>>("[{\"name\":\"Test Product A\",\"price\":99.99,\"quantity\":0},{\"name\":\"Test Product B\",\"price\":101.99,\"quantity\":0},{\"name\":\"Test Product C\",\"price\":10.99,\"quantity\":0},{\"name\":\"Test Product D\",\"price\":5,\"quantity\":0},{\"name\":\"Test Product F\",\"price\":999999999999,\"quantity\":0}]").AsQueryable());

            if (sortOption == SortOption.Recommended)
            {
                var productLookups = products.ToDictionary(x => x.Name, x => x);
                var histories = _serializer.Deserialize<List<ShopperHistory>>("[{\"customerId\":123,\"products\":[{\"name\":\"Test Product A\",\"price\":99.99,\"quantity\":3},{\"name\":\"Test Product B\",\"price\":101.99,\"quantity\":1},{\"name\":\"Test Product F\",\"price\":999999999999,\"quantity\":1}]},{\"customerId\":23,\"products\":[{\"name\":\"Test Product A\",\"price\":99.99,\"quantity\":2},{\"name\":\"Test Product B\",\"price\":101.99,\"quantity\":3},{\"name\":\"Test Product F\",\"price\":999999999999,\"quantity\":1}]},{\"customerId\":23,\"products\":[{\"name\":\"Test Product C\",\"price\":10.99,\"quantity\":2},{\"name\":\"Test Product F\",\"price\":999999999999,\"quantity\":2}]},{\"customerId\":23,\"products\":[{\"name\":\"Test Product A\",\"price\":99.99,\"quantity\":1},{\"name\":\"Test Product B\",\"price\":101.99,\"quantity\":1},{\"name\":\"Test Product C\",\"price\":10.99,\"quantity\":1}]}]");
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
                SortOption.Descending => products.OrderByDescending(x => x.Price),
                _ => products,
            };
        }

        public decimal GetTrolleyTotal(TrolleyRequest request)
        {
            var productLookups = request.Products.ToDictionary(x => x.Name, x => x);
            var specialLookups = new Dictionary<string, Dictionary<int, decimal>>();
            foreach (var special in request.Specials)
            {
                foreach (var quantityOnSpecial in special.Quantities)
                {
                    if (!specialLookups.TryGetValue(quantityOnSpecial.Name, out var quantityLookups))
                    {
                        quantityLookups = new Dictionary<int, decimal>();
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
                    var orderedSpecials = specials.OrderBy(x => remainingItemCount % x.Key).ThenByDescending(x => x.Key);
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

        private async Task<TData> Send<TRequest, TData>
            (HttpMethod method, string uri, TRequest request = null)
            where TRequest : class
        {
            var client = _httpClientFactory.CreateClient();

            StringContent content = null;
            if (request != null)
            {
                var json = _serializer.Serialize(request);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(uri),
                Content = content,
            };

            var response = await client.SendAsync(httpRequestMessage);

            var dataJson = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var data = _serializer.Deserialize<TData>(dataJson);

            return data;
        }
    }
}