using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WX.Api.Abstractions;
using WX.Api.Models;

namespace WX.Api.Controllers
{
    [Route("api/answers")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IResourceService _resourceService;
        private readonly ITrolleyCalculator _trolleyCalculator;
        private readonly ISettings _settings;

        public MainController(ISettings settings, IResourceService resourceService, ITrolleyCalculator trolleyCalculator)
        {
            _settings = settings;
            _resourceService = resourceService;
            _trolleyCalculator = trolleyCalculator;
        }

        [HttpPost]
        [Route("trolleytotal")]
        public IActionResult GetTrolleyTotal([FromBody] TrolleyRequest request)
        {
            var total = _trolleyCalculator.GetTrolleyTotal(request);
            return Ok(total);
        }

        [HttpGet]
        [Route("user")]
        public IActionResult GetUser()
        {
            return Ok(new { _settings.Name, _settings.Token });
        }

        [HttpGet]
        [Route("products")]
        public async Task<IActionResult> ListProducts([FromQuery] SortOption sortOption)
        {
            var products = await _resourceService.ListProducts(sortOption);
            return Ok(products);
        }
    }
}