namespace EasySave.Models
{
    public class AppConfiguration
    {
        public string Language { get; set; }
        public List<string> BlockedApps { get; set; }
        // Reserved for v2.0: file extensions that should be encrypted
        public List<string> EncryptedExtensions { get; set; }
    }
}
