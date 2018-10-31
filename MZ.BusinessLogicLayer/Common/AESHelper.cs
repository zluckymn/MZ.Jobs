using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace BusinessLogicLayer
{
    /// <summary>
    /// AES对称加解密
    /// </summary>
    public class AESHelper
    {
        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="Data">被加密的明文</param>  
        /// <param name="Key">密钥</param>  
        /// <param name="Vector">向量</param>  
        /// <returns>密文</returns>  
        public static String AESEncrypt(String Data, String Key, String Vector)
        {
            Byte[] plainBytes = Encoding.UTF8.GetBytes(Data);

            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);
            Byte[] bVector = new Byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);

            Byte[] Cryptograph = null; // 加密后的密文  

            Rijndael Aes = Rijndael.Create();
            try
            {
                // 开辟一块内存流  
                using (MemoryStream Memory = new MemoryStream())
                {
                    // 把内存流对象包装成加密流对象  
                    using (CryptoStream Encryptor = new CryptoStream(Memory,
                     Aes.CreateEncryptor(bKey, bVector),
                     CryptoStreamMode.Write))
                    {
                        // 明文数据写入加密流  
                        Encryptor.Write(plainBytes, 0, plainBytes.Length);
                        Encryptor.FlushFinalBlock();

                        Cryptograph = Memory.ToArray();
                    }
                }
            }
            catch
            {
                Cryptograph = null;
            }

            return Convert.ToBase64String(Cryptograph);
        }
        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="Data">被解密的密文</param>  
        /// <param name="Key">密钥</param>  
        /// <param name="Vector">向量</param>  
        /// <returns>明文</returns>  
        public static String AESDecrypt(String Data, String Key, String Vector)
        {
            Byte[] encryptedBytes = Convert.FromBase64String(Data);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);
            Byte[] bVector = new Byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);

            Byte[] original = null; // 解密后的明文  

            Rijndael Aes = Rijndael.Create();
            try
            {
                // 开辟一块内存流，存储密文  
                using (MemoryStream Memory = new MemoryStream(encryptedBytes))
                {
                    // 把内存流对象包装成加密流对象  
                    using (CryptoStream Decryptor = new CryptoStream(Memory,
                    Aes.CreateDecryptor(bKey, bVector),
                    CryptoStreamMode.Read))
                    {
                        // 明文存储区  
                        using (MemoryStream originalMemory = new MemoryStream())
                        {
                            Byte[] Buffer = new Byte[1024];
                            Int32 readBytes = 0;
                            while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
                            {
                                originalMemory.Write(Buffer, 0, readBytes);
                            }

                            original = originalMemory.ToArray();
                        }
                    }
                }
            }
            catch
            {
                original = null;
            }
            return Encoding.UTF8.GetString(original);
        }
        /// <summary>  
        /// AES加密(无向量)  
        /// </summary>  
        /// <param name="plainBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>密文</returns>  
        public static string AESEncrypt(String Data, String Key)
        {
            MemoryStream mStream = new MemoryStream();
            RijndaelManaged aes = new RijndaelManaged();

            byte[] plainBytes = Encoding.UTF8.GetBytes(Data);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            //aes.Key = _key;  
            aes.Key = bKey;
            //aes.IV = _iV;  
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }
        /// <summary>  
        /// AES解密(无向量)  
        /// </summary>  
        /// <param name="encryptedBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>明文</returns>  
        public static string AESDecrypt(String Data, String Key)
        {
            Byte[] encryptedBytes = Convert.FromBase64String(Data);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(encryptedBytes);
            //mStream.Write( encryptedBytes, 0, encryptedBytes.Length );  
            //mStream.Seek( 0, SeekOrigin.Begin );  
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            //aes.IV = _iV;  
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                byte[] tmp = new byte[encryptedBytes.Length + 32];
                int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
                byte[] ret = new byte[len];
                Array.Copy(tmp, 0, ret, 0, len);
                return Encoding.UTF8.GetString(ret);
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }

        /// <summary>
        ///  AES加密（Java平台进行解密）
        ///  参考：http://www.cnblogs.com/lzrabbit/p/3639503.html
        /// </summary>
        /// <param name="str">加密字符串</param>
        /// <param name="key">秘钥必须是16位</param>
        /// <returns></returns>
        public static string AesEncryptForJava(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        ///  AES 解密（Java平台进行加密）
        ///  参考：http://www.cnblogs.com/lzrabbit/p/3639503.html
        /// </summary>
        /// <param name="str">加密后字符串</param>
        /// <param name="key">秘钥必须是16位</param>
        /// <returns></returns>
        public static string AesDecryptForJava(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        ///  AES 解密（Java平台进行加密）(HNJY)
        ///  参考：http://www.cnblogs.com/lzrabbit/p/3639503.html
        /// </summary>
        /// <param name="str">加密后字符串</param>
        /// <param name="key">秘钥必须是16位</param>
        /// <returns></returns>
        public static string AesDecryptForJavaHNJY(string str, string key)
        {
            try
            {
                RijndaelManaged rijndaelCipher = new RijndaelManaged();

                rijndaelCipher.Mode = CipherMode.ECB;

                rijndaelCipher.Padding = PaddingMode.PKCS7;

                byte[] encryptedData = Convert.FromBase64String(str);

                byte[] pwdBytes = Convert.FromBase64String(key);

                byte[] keyBytes = new byte[16];

                int len = pwdBytes.Length;

                if (len > keyBytes.Length) len = keyBytes.Length;

                System.Array.Copy(pwdBytes, keyBytes, len);

                rijndaelCipher.Key = keyBytes;

                byte[] IV = new byte[rijndaelCipher.BlockSize / 8];
                rijndaelCipher.IV = IV;

                ICryptoTransform transform = rijndaelCipher.CreateDecryptor();

                byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                return Encoding.UTF8.GetString(plainText);
            }
            catch {
                return "";
            }

        }
    }
}
