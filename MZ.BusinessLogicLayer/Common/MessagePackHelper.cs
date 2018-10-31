using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using MessagePack;

namespace BusinessLogicLayer
{
    /// <summary>
    /// AES对称加解密
    /// </summary>
    public class MessagePackHelper
    {
        /// <summary>
        /// 对数据进行压缩保存
        /// </summary>
        /// <param name="bsonDoc"></param>
        /// <returns></returns>
        public byte[]  MessagePackObject(string runLog)
        {
            var runLogByte = MessagePackSerializer.Serialize(runLog,
            MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            return runLogByte;
        }
        /// <summary>
        /// 对数据进行解压缩保存
        /// </summary>
        /// <param name="bsonDoc"></param>
        /// <returns></returns>
        public string MessageDePackObject(byte[] dePackeObject)
        {
            var runLog = MessagePackSerializer.Deserialize<string>(dePackeObject,
                    MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            
            return runLog;
        }
    }
}
