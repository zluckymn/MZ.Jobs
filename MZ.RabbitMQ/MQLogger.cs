using System;
using EasyNetQ;

namespace MZ.RabbitMQ
{
    public class MqLogger: IEasyNetQLogger
    {
        public void DebugWrite(string format, params object[] args) {
          //  Console.WriteLine("DebugWrite:\n" + string.Format(format, args));
        }
        public void ErrorWrite(Exception exception) {
            //Console.WriteLine("ErrorWrite\n" + exception.Message);
        }
        public void ErrorWrite(string format, params object[] args) {
            //Console.WriteLine("ErrorWrite:\n" + string.Format(format, args));
        }
        public void InfoWrite(string format, params object[] args) {
            //Console.WriteLine("InfoWrite:\n" + string.Format(format, args));
        }
    }
}
