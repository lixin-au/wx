namespace WX.Api.Abstractions
{
    public interface ISerializer
    {
        T Deserialize<T>(string json);

        string Serialize(object inpput);
    }
}