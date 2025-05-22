using System;

namespace WPF
{
    public static class ToastService
    {
        public static Action<string, int> Show { get; set; }
    }
}