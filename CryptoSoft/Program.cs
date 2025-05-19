using System;
using System.IO;
using System.Text;

namespace CryptoSoft
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if the correct number of arguments are provided (3 : operation, file path, key)
            if (args.Length != 3 || (args[0] != "-e" && args[0] != "-d"))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  CryptoSoft.exe -e <filepath> <secretKey>   : Encrypt file");
                Console.WriteLine("  CryptoSoft.exe -d <filepath> <secretKey>  : Decrypt file");
                return;
            }

            string filePath = args[1];

            try
            {
                if (args[0] == "-e")
                {
                    byte[] key = Encoding.UTF8.GetBytes(args[2]);
                    EncryptFile(filePath, key);
                    Console.WriteLine($"Encrypted: {filePath}.xor");
                }
                else
                {
                    byte[] key = Encoding.UTF8.GetBytes(args[2]);
                    DecryptFile(filePath, key);
                    Console.WriteLine($"Decrypted: {filePath.Replace(".xor", "")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void EncryptFile(string inputFile, byte[] key)
        {
            string outputFile = inputFile + ".xor";
            byte[] inputBytes = File.ReadAllBytes(inputFile);
            byte[] encryptedBytes = XorBytes(inputBytes, key);
            File.WriteAllBytes(outputFile, encryptedBytes);
        }

        static void DecryptFile(string inputFile, byte[] key)
        {
            if (!inputFile.EndsWith(".xor"))
                throw new Exception("File does not have .xor extension");

            string outputFile = inputFile.Replace(".xor", "");
            byte[] inputBytes = File.ReadAllBytes(inputFile);
            byte[] decryptedBytes = XorBytes(inputBytes, key);
            File.WriteAllBytes(outputFile, decryptedBytes);
        }

        static byte[] XorBytes(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byte mask = (byte)((i * 31 + 17) % 256); // Perturbateur simple
                result[i] = (byte)(data[i] ^ key[i % key.Length] ^ mask);
            }
            return result;
        }

    }
}
