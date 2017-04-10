namespace AppmetrS2S.Serializations
{
    public interface IJsonSerializer
    {
        string Serialize(object obj);

        T Deserialize<T>(string json);
    }
}