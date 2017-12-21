using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yinhe.ProcessingCenter;
using MongoDB.Driver;

namespace MZ.Jobs.Core
{
    public  class MongoOpCollection
    {

        public static MongoOperation GetMongoOp()
        {
            var mongoDbOp =   GetTimeOutMongoOp(SysAppConfig.DataBaseConnectionString);
            return mongoDbOp;
        }
        public static MongoOperation GetMongoOp(string connStr)
        {
            var mongoDbOp = GetTimeOutMongoOp(connStr);
            return mongoDbOp;
        }
        public static DataOperation GetDataOp()
        {
            var mongoDbOp = new DataOperation(GetMongoOp());
          
            return mongoDbOp;
        }
        public static DataOperation GetDataOp(string connStr)
        {
            var mongoDbOp = new DataOperation(GetMongoOp(connStr));
            return mongoDbOp;
        }

        /// <summary>
        /// 解析Mongo字符串
        /// </summary>
        /// <param name="connection">mongodb://MZsa:MZdba@192.168.1.115:37088/XHNEW20171121</param>
        /// <returns></returns>
        public static MongoOperation GetTimeOutMongoOp(string connection)
        {
            var server = MongoServer.Create(connection);
            var userName = server.Settings.DefaultCredentials.Username;
            var password = server.Settings.DefaultCredentials.Password;
            var ip = server.Instance.Address.Host;
            var port = server.Instance.Address.Port;
            var beginIndex = connection.LastIndexOf("/", StringComparison.Ordinal);
            var dataBaseName = connection.Substring(beginIndex+1, connection.Length - beginIndex - 1);
            return GetTimeOutMongoOp(ip, port, dataBaseName, userName, password);
        }

        /// <summary>
        /// 生成长连接的连接字符串
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="databaseName"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <returns></returns>
        public static MongoOperation GetTimeOutMongoOp(string ip,int port, string databaseName,string userName,string passWord)
        {

            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress(ip, port);
            builder.DatabaseName = databaseName;
            builder.Username = userName;
            builder.Password = passWord;
            builder.SocketTimeout = new TimeSpan(00, 09, 59);
            var mongoDbOp = new MongoOperation(builder);
            return mongoDbOp;
        }
    }
}
