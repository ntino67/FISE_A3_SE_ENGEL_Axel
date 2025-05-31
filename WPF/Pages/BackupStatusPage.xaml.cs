using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Core.Model;
using Core.ViewModel;
using WPF.Infrastructure;
using WPF.Converter;

namespace WPF.Pages
{
    public partial class BackupStatusPage : Page
    {
        private readonly BackupStatusViewModel _statusVm;

        public BackupStatusPage()
        {
            InitializeComponent();
            _statusVm = new BackupStatusViewModel(ViewModelLocator.JobViewModel);
            DataContext = _statusVm;
        }
    }
}