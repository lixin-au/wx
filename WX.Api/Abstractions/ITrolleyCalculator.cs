using WX.Api.Models;

namespace WX.Api.Abstractions
{
    public interface ITrolleyCalculator
    {
        decimal GetTrolleyTotal(TrolleyRequest request);
    }
}