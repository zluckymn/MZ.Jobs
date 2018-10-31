using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net.Mail;

namespace BusinessLogicLayer
{
    /// <summary>
    /// send email
    /// </summary>
    public class AntaSmtpHelper
    {
        private static string SendMailAddress { get; set; } = CusAppConfig.SendMailAddress;
        private static string SendMailPassWord { get; set; } = CusAppConfig.SendMailPassWord;

        public static SmtpClient loadSmtpInfo()
        {
            SmtpClient client = new SmtpClient();
            client.Host = "mail.anta.com";
            client.Credentials = new System.Net.NetworkCredential(SendMailAddress, SendMailPassWord);
            return client;
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userList"></param>
        /// <param name="htmlContent"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static MailMessage buildMailMessage(List<string> userList, string htmlContent, string title)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(SendMailAddress, "监控系统");

            Encoding chtEnc = Encoding.BigEndianUnicode;

            foreach (string user in userList)
            {
                MailAddress address = new MailAddress(user, " ", chtEnc);
                message.Bcc.Add(address);
            }


            message.IsBodyHtml = true;
            message.Body = htmlContent;
            message.Subject = (title).ToString();
            return message;
        }
        public static string CheckDtNow()
        {
            string str = "您好";
            try
            {
                DateTime dt = DateTime.Now;
                int h = dt.Hour;
                if (0 < h && h < 3)
                    str = "晚上好";
                else if (2 < h && h < 12)
                    str = "早上好";
                else if (h == 12)
                    str = "中午好";
                else if (12 < h && h < 19)
                    str = "下午好";
                else if (18 < h && h < 24)
                    str = "晚上好";
                else
                    str = "您好";
            }
            catch
            {
            }
            return str;
        }
    }
 
}
