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
        public BackupStatusPage()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.JobViewModel;
        }
    }
}