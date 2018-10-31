using System;
using MongoDB.Driver;
using Yinhe.ProcessingCenter;

namespace BusinessLogicLayer.Common
{
    public  class MongoOpCollection
    {

        public static MongoOperation GetMongoOp()
        {
            var mongoDbOp = new MongoOperation(SysAppConfig.DataBaseConnectionString);
            return mongoDbOp;
        }
        public static MongoOperation GetMongoOp(string connStr)
        {
            var mongoDbOp = new MongoOperation(connStr);
            return mongoDbOp;
        }
        public static DataOperation GetDataOp()
        {
            var mongoDbOp = new DataOperation(SysAppConfig.DataBaseConnectionString);
          
            return mongoDbOp;
        }
        public static DataOperation GetDataOp(string connStr)
        {
            var mongoDbOp = new DataOperation(GetMongoOp(connStr));
            return mongoDbOp;
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
            builder.SocketTimeout = new TimeSpan(00, 03, 59);
            var mongoDbOp = new MongoOperation(builder);
            return mongoDbOp;
        }
    }
}
