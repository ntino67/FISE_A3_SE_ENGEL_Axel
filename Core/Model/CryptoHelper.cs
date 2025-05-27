using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Model.Implementations;
using Core.Model.Interfaces;
using CryptoSoft;

namespace Core.Model
{
    internal static class CryptoHelper
    {
        internal static async Task<long> Encrypt(string sourceDirectory, IEnumerable<string> extensions, string wildcard, string secretKey, IProgress<float> progress)
        {
          
            return await Task.Run(() => ProcessDirectory(sourceDirectory, true, extensions, wildcard, secretKey, progress));
        }

        internal static async Task<long> Decrypt(string sourceDirectory, string secretKey, IProgress<float> progress)
        {
            return await Task.Run(() => ProcessDirectory(sourceDirectory, false, new[] { ".enc" }, null, secretKey, progress));
        }

        private static long ProcessDirectory(string folderPath, bool encrypt, IEnumerable<string> extensions, string wildcard, string secretKey, IProgress<float> progress)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Directory '{folderPath}' not found.");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            List<string> filesToProcess;

            if (!encrypt)
            {
                filesToProcess = Directory.GetFiles(folderPath, wildcard, SearchOption.AllDirectories).ToList();
            }

            else if (!string.IsNullOrWhiteSpace(wildcard))
            {
                try
                {
                    filesToProcess = Directory.GetFiles(folderPath, wildcard, SearchOption.AllDirectories).ToList();

                }
                catch (Exception ex)
                {
                    filesToProcess = new List<string>();
                    Console.WriteLine($"[CryptoHelper] Error retrieving files with wildcard '{wildcard}': {ex.Message}");
                }
            }

            // Sinon, utiliser les extensions spécifiées
            else if (extensions != null && extensions.Any())
            {
                filesToProcess = GetFilesByExtensions(folderPath, extensions);
            }
            // Aucune règle spécifiée, ne rien faire
            else
            {
                return 0;
            }


            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            int successCount = 0, failCount = 0;
            int numFiles = filesToProcess.Count;


            foreach (var file in filesToProcess)
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
                    failCount++;
                    Console.WriteLine($"[CryptoHelper] Error processing file '{file}': {ex.Message}");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"[CryptoHelper] {successCount} files processed successfully, {failCount} failed. Time: {stopwatch.ElapsedMilliseconds}ms");
            return stopwatch.ElapsedMilliseconds;
        }

        private static List<string> GetFilesByExtensions(string folderPath, IEnumerable<string> extensions)
        {
            if (extensions == null || !extensions.Any())
                return new List<string>();

            var normalizedExtensions = extensions
                .Select(ext => ext.StartsWith(".") ? ext.ToLower() : "." + ext.ToLower())
                .ToHashSet();

            return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(file =>
                {
                    string extension = Path.GetExtension(file).ToLower();
                    return normalizedExtensions.Contains(extension);
                })
                .ToList();
        }
    }
}