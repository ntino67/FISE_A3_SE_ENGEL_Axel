using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoSoft
{
    class Program
    {
        // Clé et IV (vecteur d'initialisation) – À sécuriser dans une vraie app !
        private static readonly byte[] key = Encoding.UTF8.GetBytes("1234567890abcdef"); // 16 bytes = 128 bits
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("abcdef1234567890"); // 16 bytes

        static void Main(string[] args)
        {
            if (args.Length != 2 || (args[0] != "-e" && args[0] != "-d"))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  CryptoSoft.exe -e <filepath>   : Encrypt file");
                Console.WriteLine("  CryptoSoft.exe -d <filepath>   : Decrypt file");
                return;
            }

            string filePath = args[1];

            try
            {
                if (args[0] == "-e")
                {
                    EncryptFile(filePath);
                    Console.WriteLine($"Encrypted: {filePath}.aes");
                }
                else
                {
                    DecryptFile(filePath);
                    Console.WriteLine($"Decrypted: {filePath.Replace(".aes", "")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void EncryptFile(string inputFile)
        {
            string outputFile = inputFile + ".aes";

            using (FileStream input = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (FileStream output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (CryptoStream cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    input.CopyTo(cryptoStream);
                }
            }
        }

        static void DecryptFile(string inputFile)
        {
            if (!inputFile.EndsWith(".aes"))
                throw new Exception("File does not have .aes extension");

            string outputFile = inputFile.Replace(".aes", "");

            using (FileStream input = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (FileStream output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (CryptoStream cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(output);
                }
            }
        }

    }
}
