using System.Linq;
using WX.Api.Abstractions;
using WX.Api.Models;

namespace WX.Api.Services
{
    public class TrolleyCalculator : ITrolleyCalculator
    {
        private readonly ISerializer _serializer;

        public TrolleyCalculator(ISerializer serializer)

        {
            _serializer = serializer;
        }

        public decimal GetTrolleyTotal(TrolleyRequest request)
        {
            var productLookups = request.Products.ToDictionary(x => x.Name, x => x);
            var specials = request.Specials.OrderByDescending(x => x.Total);
            var trolleyItemLookups = request.Quantities.ToDictionary(x => x.Name, x => x);

            var total = 0M;
            foreach (var special in specials)
            {
                var canApply = true;
                while (canApply)
                {
                    foreach (var productInSpecial in special.Quantities)
                    {
                        if (!trolleyItemLookups.TryGetValue(productInSpecial.Name, out var item))
                        {
                            continue;
                        }

                        canApply = canApply && (productInSpecial.Quantity <= item.Quantity);
                        if (!canApply)
                        {
                            break;
                        }
                    }

                    if (!canApply)
                    {
                        break;
                    }

                    foreach (var productInSpecial in special.Quantities)
                    {
                        if (!trolleyItemLookups.TryGetValue(productInSpecial.Name, out var item))
                        {
                            continue;
                        }

                        item.Quantity -= productInSpecial.Quantity;
                    }
                    total += special.Total;
                }
            }

            foreach (var itemEntry in trolleyItemLookups)
            {
                if (productLookups.TryGetValue(itemEntry.Value.Name, out var product))
                {
                    total += itemEntry.Value.Quantity * product.Price;
                }
            }

            return total;
        }
    }
}