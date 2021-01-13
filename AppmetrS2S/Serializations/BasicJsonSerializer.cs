using Newtonsoft.Json;

namespace AppmetrS2S.Serializations
{
	public class BasicJsonSerializer : IJsonSerializer
	{
		private static readonly JsonSerializerSettings _serializer = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Objects
		};
		public string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, _serializer);
		}

		public T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, _serializer);
		}
	}
}
