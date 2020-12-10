using System.Collections.Generic;
using System.Threading.Tasks;
using WX.Api.Models;

namespace WX.Api.Abstractions
{
    public interface IResourceService
    {
        Task<IEnumerable<Product>> ListProducts(SortOption sortOption);
    }
}