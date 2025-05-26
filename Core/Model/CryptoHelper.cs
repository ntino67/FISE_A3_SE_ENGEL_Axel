using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CryptoSoft;

namespace Core.Model
{
    internal static class CryptoHelper
    {
        internal static long Encrypt(string sourceDirectory, IEnumerable<string> extensions, string secretKey)
        {
            return ProcessDirectory(sourceDirectory, true, extensions, secretKey);
        }

        internal static long Decrypt(string sourceDirectory, string secretKey)
        {
            return ProcessDirectory(sourceDirectory, false, new[] { ".enc" }, secretKey);
        }

        private static long ProcessDirectory(string folderPath, bool encrypt, IEnumerable<string> extensions, string secretKey)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Directory '{folderPath}' not found.");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var normalizedExtensions = extensions
                .Select(ext => ext.StartsWith(".") ? ext.ToLower() : "." + ext.ToLower())
                .ToHashSet();

            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(file =>
                {
                    string extension = Path.GetExtension(file).ToLower();
                    return normalizedExtensions.Contains(extension);
                })
                .ToList();

            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            int successCount = 0, failCount = 0;

            foreach (var file in allFiles)
            {
                if (!File.Exists(file))
                    continue;

                try
                {
                    if (encrypt)
                        XorEncryption.EncryptFile(file, keyBytes);
                    else
                        XorEncryption.DecryptFile(file, keyBytes);

                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process file {file}: {ex.Message}");
                    failCount++;
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"[CryptoHelper] {successCount} files processed successfully, {failCount} failed. Time: {stopwatch.ElapsedMilliseconds}ms");
            return stopwatch.ElapsedMilliseconds;
        }
    }
}