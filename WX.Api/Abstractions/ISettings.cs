namespace WX.Api.Abstractions
{
    public interface ISettings
    {
        string Name { get; }
        string Token { get; }
        string ProductsUri { get; }
        string ShopperHistoryUri { get; }
    }
}