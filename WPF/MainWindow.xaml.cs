using Core.Model;
using Core.ViewModel;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using WPF.Pages;

namespace WPF
{
    public partial class MainWindow : Window
    {
        private readonly JobViewModel _vm = ViewModelLocator.JobViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Core.Utils.ToastBridge.ShowToast = ShowToast;
            MainFrame.Navigate(new WelcomePage());

            // For testing: create dummy jobs if none exist
            if (!_vm.Jobs.Any())
            {
                _vm.CreateNewJob("Job 1");
                _vm.CreateNewJob("Job 2");
                _vm.CreateNewJob("Job 3");
            }

            JobList.ItemsSource = _vm.Jobs;
        }

        private void TopBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void GlobalSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AppSettingsPage());
        }

        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LogsOverviewPage());
        }

        private void Status_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BackupStatusPage());
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void JobSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BackupJob job)
            {
                _vm.SetCurrentJob(job);
                MainFrame.Navigate(new JobSettingsPage());
            }
        }

        private void AddJobButton_Click(object sender, RoutedEventArgs e)
        {
            string jobName = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(jobName))
            {
                MessageBox.Show("Veuillez entrer un nom de job.", "Nom manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_vm.Jobs.Any(j => j.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ce nom de job existe déjà.", "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _vm.CreateNewJob(jobName);
            SearchBox.Text = "";
        }

        private void SearchJobButton_Click(object sender, RoutedEventArgs e)
        {
            string search = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(search))
            {
                JobList.ItemsSource = _vm.Jobs;
            }
            else
            {
                JobList.ItemsSource = _vm.Jobs.Where(j => j.Name.ToLower().Contains(search)).ToList();
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (RunComboBox.SelectedIndex == 2)
            {
                foreach (var job in _vm.Jobs)
                    job.IsChecked = false;
                JobList.ItemsSource = null;
                JobList.ItemsSource = _vm.Jobs;
            }
        }
        
        public async void ShowToast(string message, int durationMs = 3000)
        {
            ToastText.Text = message;
            ToastHost.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
            ToastHost.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            await Task.Delay(durationMs);

            var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(300)));
            ToastHost.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            await Task.Delay(300);
            ToastHost.Visibility = Visibility.Collapsed;
        }

        private void DeleteJobButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BackupJob job)
            {
                _vm.DeleteJob(job.Id);
            }
        }
        
        public async void ShowToast(string message, int durationMs = 3000)
        {
            ToastText.Text = message;
            ToastHost.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
            ToastHost.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            await Task.Delay(durationMs);

            var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(300)));
            ToastHost.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            await Task.Delay(300);
            ToastHost.Visibility = Visibility.Collapsed;
        }
    }
} 