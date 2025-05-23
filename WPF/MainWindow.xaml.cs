using Core.Model;
using Core.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using WPF.Infrastructure;
using WPF.Pages;

namespace WPF
{
    public partial class MainWindow : Window
    {
        private readonly JobViewModel _vm = ViewModelLocator.JobViewModel;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _vm;

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

            var fadeSlideIn = new Storyboard();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            var slideIn = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, -50),
                To = new Thickness(0, 0, 0, 30),
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(fadeIn, ToastHost);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));

            Storyboard.SetTarget(slideIn, ToastHost);
            Storyboard.SetTargetProperty(slideIn, new PropertyPath("Margin"));

            fadeSlideIn.Children.Add(fadeIn);
            fadeSlideIn.Children.Add(slideIn);
            fadeSlideIn.Begin();

            await Task.Delay(durationMs);

            var fadeSlideOut = new Storyboard();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            var slideOut = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 30),
                To = new Thickness(0, 0, 0, -50),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(fadeOut, ToastHost);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

            Storyboard.SetTarget(slideOut, ToastHost);
            Storyboard.SetTargetProperty(slideOut, new PropertyPath("Margin"));

            fadeSlideOut.Children.Add(fadeOut);
            fadeSlideOut.Children.Add(slideOut);
            fadeSlideOut.Begin();

            await Task.Delay(300);
            ToastHost.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _vm.CreateJobCommand?.RaiseCanExecuteChanged();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AppSettingsPage());

        }
    }
}