using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WhiteBoard.Utils
{
    /// <summary>
    /// 安全帮助类
    /// </summary>
    public class SecurityHelper
    {
        /// <summary>
        /// 用MD5加密字符串，可选择生成16位或者32位的加密字符串
        /// </summary>
        /// <param name="str">待加密的字符串</param>
        /// <param name="bit">位数，一般取值16 或 32</param>
        /// <returns>MD5字符串</returns>
        public static string MD5Encrypt(string str, int bit = 32)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes;
            hashedDataBytes = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            foreach (byte i in hashedDataBytes)
            {
                sb.Append(i.ToString("x2"));
            }
            if (bit == 16)
            {
                return sb.ToString().Substring(8, 16).ToLower();
            }
            else
            {
                return sb.ToString().ToLower();
            }
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="content">加密内容</param>
        /// <returns>加密后内容</returns>
        public static string DESEncrypt(string content, string key)
        {
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(content);
                    des.Key = Encoding.UTF8.GetBytes(key);
                    des.IV = Encoding.UTF8.GetBytes(key);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(inputByteArray, 0, inputByteArray.Length);
                            cs.FlushFinalBlock();

                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量，但从未使用过
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="encryptContent">加密后内容</param>
        /// <returns>原密码</returns>
        public static string DESDecrypt(string encryptContent, string key)
        {
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] inputByteArray = Convert.FromBase64String(encryptContent);
                    des.Key = Encoding.UTF8.GetBytes(key);
                    des.IV = Encoding.UTF8.GetBytes(key);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(inputByteArray, 0, inputByteArray.Length);
                            cs.FlushFinalBlock();

                            return Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量，但从未使用过
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="content">内容</param>
        /// <returns>加密字符串</returns>
        public static string Base64Encrypt(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            string res = Convert.ToBase64String(bytes);
            return res;
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="encryptContent">加密字符串</param>
        /// <returns>解密字符串</returns>
        public static string Base64Decrypt(string encryptContent)
        {
            byte[] bytes = Convert.FromBase64String(encryptContent);
            string res = Encoding.UTF8.GetString(bytes);
            return res;
        }
    }
}
