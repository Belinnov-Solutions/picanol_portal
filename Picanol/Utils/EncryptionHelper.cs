using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Picanol.Utils
{
    public static class EncryptionHelper
    {
        public static string Encryptor(string strText, string EncrKey)
        {
            byte[] byKey = { };
            byte[] IV =
              {
        18,52,86,120,144,171,205,239
        };
            byKey = System.Text.Encoding.UTF8.GetBytes(EncrKey);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = System.Text.Encoding.UTF8.GetBytes(strText);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }


        public static string DecryptFromBase64String(string stringToDecrypt, string EncryptionKey)
        {
            byte[] inputByteArray = new byte[stringToDecrypt.Length];
            byte[] byKey = { };
            byte[] IV =
            {
        18,52,86,120,144,171,205,239
        };
            byKey = System.Text.Encoding.UTF8.GetBytes(EncryptionKey);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(stringToDecrypt);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            return encoding.GetString(ms.ToArray());
        }
    }
}