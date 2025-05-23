using System;
using System.IO;
using System.Text;

namespace CryptoSoft
{
    public static class XorEncryption
    {
        public static void EncryptFile(string inputFile, byte[] key)
        {
            string outputFile = inputFile + ".enc";
            byte[] inputBytes = File.ReadAllBytes(inputFile);
            byte[] encryptedBytes = XorBytes(inputBytes, key);
            File.WriteAllBytes(outputFile, encryptedBytes);
            File.Delete(inputFile);
        }

        public static void DecryptFile(string inputFile, byte[] key)
        {
            if (!inputFile.EndsWith(".enc"))
                throw new Exception("File does not have .enc extension");

            string outputFile = inputFile.Replace(".enc", "");
            byte[] inputBytes = File.ReadAllBytes(inputFile);
            byte[] decryptedBytes = XorBytes(inputBytes, key);
            File.WriteAllBytes(outputFile, decryptedBytes);
            File.Delete(inputFile);
        }

        private static byte[] XorBytes(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byte mask = (byte)((i * 31 + 17) % 256);
                result[i] = (byte)(data[i] ^ key[i % key.Length] ^ mask);
            }
            return result;
        }

        // Optional: CLI for previous version
        public static void Main(string[] args)
        {
            if (args.Length != 3 || (args[0] != "-e" && args[0] != "-d"))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  CryptoSoft.exe -e <filepath> <key>   : Encrypt file");
                Console.WriteLine("  CryptoSoft.exe -d <filepath> <key>   : Decrypt file");
                return;
            }

            string filePath = args[1];
            byte[] key = Encoding.UTF8.GetBytes(args[2]);

            try
            {
                if (args[0] == "-e")
                {
                    EncryptFile(filePath, key);
                    Console.WriteLine($"Encrypted: {filePath}.enc");
                }
                else
                {
                    DecryptFile(filePath, key);
                    Console.WriteLine($"Decrypted: {filePath.Replace(".enc", "")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
