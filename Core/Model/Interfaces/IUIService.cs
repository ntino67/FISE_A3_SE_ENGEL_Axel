namespace Core.Model.Interfaces
{
    public interface IUIService
    {
        void ShowToast(string toastText, int duration = 3000);
        bool Confirm(string message, string title = "Confirm");
    }
}