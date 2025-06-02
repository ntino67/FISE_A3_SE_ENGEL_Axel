namespace Core.Model.Interfaces
{
    public interface IResourceService
    {
        string GetString(string key, params object[] formatArgs);
    }
}
