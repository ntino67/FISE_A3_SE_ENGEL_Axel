using Core.Utils;

namespace Core.Model.Interfaces
{
    public interface IUIService
    {
        void ShowToast(string toastText, int duration = 3000);
        DeleteJobChoice ConfirmDeleteJobWithFiles(string jobName, string targetDir);
    }
}