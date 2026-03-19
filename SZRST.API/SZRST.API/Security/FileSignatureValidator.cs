using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SZRST.API.Security
{
	public static class FileSignatureValidator
	{
		private static readonly Dictionary<string, List<byte[]>> Signatures = new(StringComparer.OrdinalIgnoreCase)
		{
			[".jpg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
			[".jpeg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
			[".png"] = new() { new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
			[".webp"] = new() { new byte[] { 0x52, 0x49, 0x46, 0x46 } }
		};

		public static bool IsValidImage(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return false;
			}

			var extension = Path.GetExtension(file.FileName);
			if (string.IsNullOrWhiteSpace(extension) || !Signatures.TryGetValue(extension, out var signatures))
			{
				return false;
			}

			using var stream = file.OpenReadStream();
			using var reader = new BinaryReader(stream);
			var headerBytes = reader.ReadBytes(12);

			if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
			{
				return headerBytes.Length >= 12 &&
				       headerBytes.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) &&
				       headerBytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 });
			}

			return signatures.Any(signature =>
				headerBytes.Length >= signature.Length &&
				headerBytes.Take(signature.Length).SequenceEqual(signature));
		}
	}
}
