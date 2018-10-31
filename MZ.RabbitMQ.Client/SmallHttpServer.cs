using System.Net;
using System.Text;

namespace MZ.RabbitMQ.Client
{
    public class SmallHttpServer
    {
        public void Listen(string ip,int port)
        {
            HttpListener httpListener = new HttpListener();
            var url = $"http://+:{port}/";
            httpListener.Prefixes.Add(url);
            httpListener.Start();
            while (true)
            {
                HttpListenerContext ctx = httpListener.GetContext();
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentEncoding = Encoding.UTF8;
                byte[] buffer = Encoding.UTF8.GetBytes("pong");
                ctx.Response.ContentLength64 = buffer.Length;
                var output = ctx.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                ctx.Response.Close();
            }
        }
    }
}
