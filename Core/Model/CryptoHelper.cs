using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Core.Model
{
    internal static class CryptoHelper
    {
        private const string CryptoSoftPath = @"C:\WorkDirMQ\FISE_A3_SE_ENGEL_Axel\CryptoSoft\bin\Debug\netcoreapp3.1\CryptoSoft.exe";

        internal static string Encrypt(string sourceDirectory, IEnumerable<string> extensions, string SecretKey)
        {
            return ProcessDirectory(sourceDirectory, "-e", extensions, SecretKey);
        }

        internal static string Decrypt(string sourceDirectory, string SecretKey)
        {
            return ProcessDirectory(sourceDirectory, "-d", new[] { ".xor" }, SecretKey);
        }

        private static string ProcessDirectory(string folderPath, string mode, IEnumerable<string> extensions, string SecretKey)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Directory '{folderPath}' not found.");

            var normalizedExtensions = extensions.Select(ext => ext.StartsWith(".") ? ext.ToLower() : "." + ext.ToLower()).ToHashSet();
            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                    .Where(file => normalizedExtensions.Contains(Path.GetExtension(file).ToLower()));

            foreach (var file in allFiles)
            {
                bool success = RunCryptoSoft(mode, file, SecretKey);
                if (!success)
                    throw new Exception($"Failed to process file: {file}");
            }

            return folderPath;
        }

        private static bool RunCryptoSoft(string mode, string filePath, string key)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = CryptoSoftPath,
                    Arguments = $"{mode} \"{filePath}\" \"{key}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine($"Error on {filePath}: {error}");
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }
    }
}
