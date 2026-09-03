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

            using var symmetricKey = CreateAes();

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
            return cipherTextBytes.Decrypt(password, salt, initVector);
        }

        public static string Decrypt(this byte[] cipherTextBytes, string password, string salt, string initVector)
        {
            var initVectorBytes = initVector.ToBytes();
            var keyBytes = GetKeyBytes(password, salt);

            using var symmetricKey = CreateAes();

            using var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            using var memStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);

            // CryptoStream.Read is not required to fill the buffer in one call. Reading it once returns the
            // whole payload on Mono but drops the trailing block on CoreCLR, so drain the stream instead.
            using var plainTextStream = new MemoryStream(cipherTextBytes.Length);
            cryptoStream.CopyTo(plainTextStream);

            return Encoding.UTF8.GetString(plainTextStream.GetBuffer(), 0, (int)plainTextStream.Length);
        }

        public static byte[] ToBytes(this string text) => Encoding.UTF8.GetBytes(text);

        public static string ToBase64String(this byte[] bytes) => Convert.ToBase64String(bytes);

        /// <summary>
        /// AES-256-CBC with PKCS7 padding. Byte for byte compatible with the RijndaelManaged setup this
        /// replaces, so files written by earlier versions still decrypt.
        /// </summary>
        private static Aes CreateAes()
        {
            var aes = Aes.Create();

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            return aes;
        }

        /// <summary>
        /// Derive the AES key from a password and a salt.
        /// <para>
        /// The result depends only on that pair, yet PBKDF2 costs around 5 ms under Mono and used to be paid
        /// on every single encrypt and decrypt. The last result is kept so that repeated saves reuse it.
        /// The field is written as one reference assignment, which is atomic, so no lock is needed.
        /// </para>
        /// </summary>
        private static byte[] GetKeyBytes(string password, string salt)
        {
            var cachedKey = _cachedKey;

            if (cachedKey != null && cachedKey.Password == password && cachedKey.Salt == salt)
                return cachedKey.Key;

            var keyBytes = new Rfc2898DeriveBytes(password, salt.ToBytes()).GetBytes(KEY_SIZE / 8);

            _cachedKey = new DerivedKey(password, salt, keyBytes);

            return keyBytes;
        }

        private static volatile DerivedKey _cachedKey;

        /// <summary>
        /// A derived key together with the password and salt it was derived from.
        /// </summary>
        private sealed class DerivedKey
        {
            public readonly string Password;
            public readonly string Salt;
            public readonly byte[] Key;

            public DerivedKey(string password, string salt, byte[] key)
            {
                Password = password;
                Salt = salt;
                Key = key;
            }
        }
    }
}