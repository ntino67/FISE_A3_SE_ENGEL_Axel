using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoSoft;

namespace Core.Model
{
    internal static class CryptoHelper
    {
        internal static async Task<long> Encrypt(string sourceDirectory, IEnumerable<string> extensions, string secretKey, IProgress<float> progress)
        {
            return await Task.Run(() => ProcessDirectory(sourceDirectory, true, extensions, secretKey, progress));
        }

        internal static async Task<long> Decrypt(string sourceDirectory, string secretKey, IProgress<float> progress)
        {
            return await Task.Run(() => ProcessDirectory(sourceDirectory, false, new[] { ".enc" }, secretKey, progress));
        }

        private static long ProcessDirectory(string folderPath, bool encrypt, IEnumerable<string> extensions, string secretKey, IProgress<float> progress)
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
            int numFiles = allFiles.Count;

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
                    progress.Report((float)successCount / numFiles * 100);
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