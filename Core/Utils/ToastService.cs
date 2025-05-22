using System;

namespace Core.Utils
{
    public static class ToastBridge
    {
        public static Action<string, int> ShowToast { get; set; }
    }
}