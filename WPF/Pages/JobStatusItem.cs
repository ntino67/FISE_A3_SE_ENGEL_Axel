using System;

namespace WPF.Pages
{
    internal class JobStatusItem
    {
        public string Status { get; internal set; }
        public int Progress { get; internal set; }
        public DateTime EndTime { get; internal set; }
        public string JobName { get; internal set; }
        public DateTime StartTime { get; internal set; }
    }
}