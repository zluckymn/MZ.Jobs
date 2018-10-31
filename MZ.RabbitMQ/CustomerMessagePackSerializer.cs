using EasyNetQ;
using MessagePack;
 

namespace MZ.RabbitMQ
{
    public class CustomerMessagePackSerializer : ISerializer
    {
        private readonly ITypeNameSerializer _typeNameSerializer;

        public CustomerMessagePackSerializer(ITypeNameSerializer typeNameSerializer)
        {
            this._typeNameSerializer = typeNameSerializer;
        }
        public byte[] MessageToBytes<T>(T message) where T : class
        {
            return MessagePackSerializer.Serialize(message, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        public T BytesToMessage<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        public object BytesToMessage(string typeName, byte[] bytes)
        {
            var type = _typeNameSerializer.DeSerialize(typeName);
            var obj = MessagePackSerializer.NonGeneric.Deserialize(type, bytes, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            return obj;
        }
   
}
}
