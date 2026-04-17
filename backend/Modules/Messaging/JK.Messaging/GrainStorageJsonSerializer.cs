using System.Text.Json;
using Orleans.Storage;

namespace JK.Messaging;

public class GrainStorageJsonSerializer : IGrainStorageSerializer
{
   public BinaryData Serialize<T>(T input)
   {
      return new BinaryData(JsonSerializer.SerializeToUtf8Bytes(input));
   }

   public T? Deserialize<T>(BinaryData input)
   {
      return input.ToObjectFromJson<T>();
   }
}