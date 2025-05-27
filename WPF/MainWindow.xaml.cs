using Core.Model;
using Core.ViewModel;
using Core.Utils;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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
            MainFrame.Navigate(new WelcomePage());

            JobList.ItemsSource = _vm.Jobs;
            _vm.NavigateToHome = () => MainFrame.Navigate(new WelcomePage());
            ToastBridge.ShowToast = ShowToast;
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

        private void BackupStatusButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BackupStatusPage());
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new WelcomePage());
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
            var button = sender as Button;
            var job = button?.DataContext as BackupJob;
            if (job == null)
                return;

            var vm = ViewModelLocator.JobViewModel;
            vm.SetCurrentJob(null);
            vm.SetCurrentJob(job);

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            var page = new JobSettingsPage();
            page.DataContext = vm;
            MainFrame.Navigate(page);
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

        private void RunMultipleButton_Click(object sender, RoutedEventArgs e)
        {
            // Ajoutez ici la logique pour exécuter les jobs sélectionnés
        }


        private void ResetSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            // Décoche tous les jobs (reset selection)
            foreach (var job in _vm.Jobs)
                job.IsChecked = false;
            JobList.ItemsSource = null;
            JobList.ItemsSource = _vm.Jobs;
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AppSettingsPage());
        }
    }
}