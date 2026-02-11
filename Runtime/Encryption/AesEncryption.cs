using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace fefek5.Systems.EncryptionSystem
{
    /// <summary>
    /// AES is a symmetric 256-bit encryption algorithm.
    /// Read more: http://en.wikipedia.org/wiki/Advanced_Encryption_Standard
    /// </summary>
    public static class AesEncryption
    {
        private const int KEY_SIZE = 256;
        private const string SALT = "g46dzQ80";
        private const string INIT_VECTOR = "OFRna74m*aze01xY";

        public static string Encrypt(this string plainText, string password) => 
            plainText.Encrypt(password, SALT, INIT_VECTOR);

        public static string Encrypt(this string plainText, string password, string salt, string initVector) =>
            plainText.EncryptToBytes(password, salt, initVector).ToBase64String();
        
        public static byte[] EncryptToBytes(this string plainText, string password) =>
            plainText.EncryptToBytes(password, SALT, INIT_VECTOR);

        public static byte[] EncryptToBytes(this string plainText, string password, string salt, string initVector)
        {
            var plainTextBytes = plainText.ToBytes();
            var initVectorBytes = initVector.ToBytes();
            var keyBytes = GetKeyBytes(password, salt);

            using var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            using var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            using var memStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();

            return memStream.ToArray();
        }
        
        public static string Decrypt(this string cipherText, string password) => 
            cipherText.Decrypt(password, SALT, INIT_VECTOR);
        
        public static string Decrypt(this string cipherText, string password, string salt, string initVector)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText.Replace(' ', '+'));
            return cipherTextBytes.Decrypt(password, salt, initVector).TrimEnd('\0');
        }

        public static string Decrypt(this byte[] cipherTextBytes, string password, string salt, string initVector)
        {
            var plainTextBytes = new byte[cipherTextBytes.Length];
            var initVectorBytes = initVector.ToBytes();
            var keyBytes = GetKeyBytes(password, salt);

            using var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            using var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            using var memStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
            var byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }
        
        public static byte[] ToBytes(this string text) => Encoding.UTF8.GetBytes(text);
        
        public static string ToBase64String(this byte[] bytes) => Convert.ToBase64String(bytes);

        private static byte[] GetKeyBytes(string password, string salt) =>
            new Rfc2898DeriveBytes(password, salt.ToBytes()).GetBytes(KEY_SIZE / 8);
    }
}