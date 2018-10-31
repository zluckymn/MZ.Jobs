using EasyNetQ;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace MZ.RabbitMQ
{
    /// <summary>
    /// EasyNetQ 帮助类
    /// </summary>
    public class EasyNetQHelper : IDisposable
    {
        readonly MqLogger mqLogger = new MqLogger();
        
        public static ConcurrentDictionary<string,IBus> BusDic = new ConcurrentDictionary<string, IBus>();
        #region 快速队列获取
        
        
        /// <summary>
        /// 动态virtualHost队列
        /// </summary>
        /// <param name="virtualHost"></param>
        /// <returns></returns>
        public EasyNetQHelper VhGeneraterHelper(string virtualHost)
        {
               
                return new EasyNetQHelper(RabbitMqConfig.RabbitMqVirtualHostStr(virtualHost), mqLogger);
        }
        /// <summary>
        /// 动态virtualHost队列
        /// </summary>
        /// <param name="virtualHost"></param>
        /// <returns></returns>
        public EasyNetQHelper VhGeneraterHelper(RabbitMqVirtualHostType virtualHost)
        {
           
            return new EasyNetQHelper(RabbitMqConfig.RabbitMqVirtualHostStr(virtualHost), mqLogger);
        }

      
        #endregion
        public DateTime SuccessTime { get; set; }
        public string Id { get; set; }

        public Action OnDispose { get; set; }
        
        public void Dispose()
        {
             Bus?.Dispose();
             OnDispose?.Invoke();
        }

        public IBus Bus { get; }

        /// <summary>
        /// RabbitMQ host 地址
        /// </summary>
        public string ConnectionString { get; }

        public IEasyNetQLogger Logger { get; }

        public EasyNetQHelper()
        {
            //Logger = _mqLogger;
            //ConnectionString = RabbitMqConfig.RabbitMqHostStr;
            //if (busDic.ContainsKey(ConnectionString))
            //{
            //    Bus = busDic[ConnectionString];

            //}
            //else
            //{
            //    Bus = RabbitHutch.CreateBus(RabbitMqConfig.RabbitMqHostStr,
            //        ser =>
            //        {
            //            ser.Register<ISerializer>(_ => new CustomerMessagePackSerializer(new TypeNameSerializer()));
            //            if (_mqLogger != null)
            //            {
            //                ser.Register(_ => _mqLogger);
            //            }
            //        });
            //    busDic.Add(ConnectionString, Bus);
            //}
        }

        
        public EasyNetQHelper(string connenctionString, IEasyNetQLogger logger = null)
        {
            Logger = logger;
            ConnectionString = connenctionString;
            if (BusDic.ContainsKey(ConnectionString))
            {
                Bus = BusDic[ConnectionString];

            }
            else
            {
                Bus = RabbitHutch.CreateBus(connenctionString,
                    ser =>
                    {
                        ser.Register<ISerializer>(_ => new CustomerMessagePackSerializer(new TypeNameSerializer()));
                        if (logger != null)
                        {
                            ser.Register(_ => logger);
                        }
                    });
                if (!BusDic.ContainsKey(ConnectionString))
                {
                    try
                    {
                        BusDic.TryAdd(ConnectionString, Bus);
                    }
                    catch (System.ArgumentException ex)
                    {

                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
            }
        }

        #region 对外公开方法
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <typeparam name="T">泛型实体对象</typeparam>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public bool Broadcast<T>(T message) where T : class
        {
            try
            {
                Bus.Publish(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
          
            return true;
        }

        /// <summary>
        /// 侦听消息
        /// </summary>
        /// <typeparam name="T">泛型消息实体对象</typeparam>
        /// <param name="subscribeId">订阅id</param>
        /// <param name="onMessage">消费消息回调</param>
        /// <param name="listenCallback">成功监听回调</param>

        public ISubscriptionResult Listen<T>(string subscribeId, Action<T> onMessage, Action listenCallback = null) where T : class
        {
            var result = Bus.Subscribe(subscribeId, onMessage);
            listenCallback?.Invoke();
            return result;
        }
        /// <summary>
        /// 侦听消息 处理消息时候，不要占用太多的时间，会影响消息的处理效率，所以，遇到占用长时间的处理方法，最好用异步处理
        /// </summary>
        /// <typeparam name="T">泛型消息实体对象</typeparam>
        /// <param name="subscribeId">订阅id</param>
        /// <param name="onMessage">消费消息回调</param>
        /// <param name="listenCallback">成功监听回调</param>
        public ISubscriptionResult ListenAsync<T>(string subscribeId, Func<T, Task> onMessage, Action listenCallback = null)
            where T : class
        {
            var result = Bus.SubscribeAsync(subscribeId, onMessage);
            listenCallback?.Invoke();
            return result;
        }
        #endregion


        public void SetLastSuccessTime(DateTime date)
        {
            if (SuccessTime < date)
            {
                SuccessTime = date;
            }
        }
    }
}
