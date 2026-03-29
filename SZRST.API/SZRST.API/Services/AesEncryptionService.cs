using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SZRST.API.Services
{
	public class AesEncryptionService : IEncryptionService
	{
		private readonly byte[] _key;

		public AesEncryptionService(IConfiguration configuration)
		{
			var keyString = configuration["Encryption:Key"]
				?? throw new InvalidOperationException("Encryption:Key is not configured in appsettings.");

			_key = Convert.FromBase64String(keyString);
		}

		public string Encrypt(string plainText)
		{
			if (string.IsNullOrEmpty(plainText))
				return plainText;

			using var aes = Aes.Create();
			aes.Key = _key;
			aes.GenerateIV();

			using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream();

			ms.Write(aes.IV, 0, aes.IV.Length);

			using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
			using (var sw = new StreamWriter(cs, Encoding.UTF8))
			{
				sw.Write(plainText);
			}

			return Convert.ToBase64String(ms.ToArray());
		}

		public string Decrypt(string cipherText)
		{
			if (string.IsNullOrEmpty(cipherText))
				return cipherText;

			var fullCipher = Convert.FromBase64String(cipherText);

			using var aes = Aes.Create();
			aes.Key = _key;

			var iv = new byte[aes.BlockSize / 8];
			Array.Copy(fullCipher, 0, iv, 0, iv.Length);
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
			using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
			using var sr = new StreamReader(cs, Encoding.UTF8);

			return sr.ReadToEnd();
		}
	}
}
