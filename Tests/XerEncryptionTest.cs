using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using CryptoSoft;

namespace Tests
{
    [TestFixture]
    public class XorEncryptionTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Test]
        public void EncryptAndDecryptFile_RestoresOriginalContent()
        {
            // Arrange
            string filePath = Path.Combine(_tempDir, "secret.txt");
            string originalContent = "Ceci est un test de CryptoSoft!";
            File.WriteAllText(filePath, originalContent);
            string key = "motdepasse123";
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // Act - Encrypt
            XorEncryption.EncryptFile(filePath, keyBytes);

            // Assert - file is encrypted, original gone, encrypted exists, contents changed
            Assert.That(File.Exists(filePath), Is.False);
            Assert.That(File.Exists(filePath + ".enc"), Is.True);
            string encryptedText = File.ReadAllText(filePath + ".enc");
            Assert.That(encryptedText, Is.Not.EqualTo(originalContent)); // It's no longer plain

            // Act - Decrypt
            XorEncryption.DecryptFile(filePath + ".enc", keyBytes);

            // Assert - decrypted file is back, encrypted is gone, content is as original
            Assert.That(File.Exists(filePath + ".enc"), Is.False);
            Assert.That(File.Exists(filePath), Is.True);
            string decrypted = File.ReadAllText(filePath);
            Assert.That(decrypted, Is.EqualTo(originalContent));
        }

        [Test]
        public void DecryptFile_Throws_IfNotEncExtension()
        {
            string filePath = Path.Combine(_tempDir, "file.txt");
            File.WriteAllText(filePath, "hello world");
            byte[] key = Encoding.UTF8.GetBytes("abc");
            Assert.Throws<Exception>(() => XorEncryption.DecryptFile(filePath, key));
        }

        [Test]
        public void EncryptFile_ThenDecryptWithWrongKey_FailsContentCheck()
        {
            string filePath = Path.Combine(_tempDir, "data.txt");
            File.WriteAllText(filePath, "Important information!");
            var goodKey = Encoding.UTF8.GetBytes("goodkey");
            var badKey = Encoding.UTF8.GetBytes("badkey");

            XorEncryption.EncryptFile(filePath, goodKey);
            Assert.That(File.Exists(filePath + ".enc"), Is.True);

            // Decrypt with bad key
            XorEncryption.DecryptFile(filePath + ".enc", badKey);
            string result = File.ReadAllText(filePath);

            // Not original
            Assert.That(result, Is.Not.EqualTo("Important information!"));
        }

        [Test]
        public void EncryptFile_FileIsDeletedAfterEncryption()
        {
            string filePath = Path.Combine(_tempDir, "gone.txt");
            File.WriteAllText(filePath, "bye bye");
            byte[] key = Encoding.UTF8.GetBytes("testkey");

            XorEncryption.EncryptFile(filePath, key);

            Assert.That(File.Exists(filePath), Is.False);
            Assert.That(File.Exists(filePath + ".enc"), Is.True);
        }

        [Test]
        public void DecryptFile_FileIsDeletedAfterDecryption()
        {
            string filePath = Path.Combine(_tempDir, "goneagain.txt");
            File.WriteAllText(filePath, "adios");
            byte[] key = Encoding.UTF8.GetBytes("testkey");

            XorEncryption.EncryptFile(filePath, key);
            Assert.That(File.Exists(filePath + ".enc"), Is.True);

            XorEncryption.DecryptFile(filePath + ".enc", key);

            Assert.That(File.Exists(filePath + ".enc"), Is.False);
            Assert.That(File.Exists(filePath), Is.True);
        }
    }
}
