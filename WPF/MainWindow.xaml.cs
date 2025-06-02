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
using Core.Model.Interfaces;
using WPF.Services;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Text.Json;
using Timer = System.Timers.Timer;
using System.Collections.Concurrent;
using System.Net;


namespace WPF
{
    public partial class MainWindow : Window
    {
        private readonly IJobViewModel _vm = ViewModelLocator.JobViewModel;
        private WebSocketHost _wsHost;
        private Timer _jobUpdateTimer;



        public MainWindow()
        {
            InitializeComponent();

            _wsHost = new WebSocketHost();
            _ = _wsHost.StartAsync(); // héberge le serveur WebSocket
            _jobUpdateTimer = new Timer(2000); // 2000ms = 2s
            _jobUpdateTimer.Elapsed += async (s, e) => await BroadcastJobsUpdate();
            _jobUpdateTimer.AutoReset = true;
            _jobUpdateTimer.Start();


            DataContext = _vm;
            MainFrame.Navigate(new WelcomePage());

            JobList.ItemsSource = _vm.DisplayedJobs;
            _vm.NavigateToHome = () =>
            {
                var currentPage = MainFrame.Content;
                if (currentPage is AppSettingsPage || currentPage is BackupStatusPage)
                {
                    return;
                }
                MainFrame.Navigate(new WelcomePage());
            };
            ToastBridge.ShowToast = ShowToast;
        }
        private async Task BroadcastJobsUpdate()
        {
            if (_wsHost == null)
                return;

            var jobsToSend = _vm.Jobs
                .Where(j => !(j.Status == JobStatus.Ready))
                .ToList();

            foreach (var job in jobsToSend)
            {
                await _wsHost.BroadcastJobAsync(job);
            }
        }


        public class WebSocketHost
    {
        private readonly HttpListener _listener;
        private readonly ConcurrentBag<WebSocket> _clients = new ConcurrentBag<WebSocket>();

        public WebSocketHost(string uriPrefix = "http://localhost:5000/ws/")
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(uriPrefix);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            while (true)
            {
                var context = await _listener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _clients.Add(wsContext.WebSocket);
                    _ = Listen(wsContext.WebSocket); // fire-and-forget
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

            private async Task Listen(WebSocket socket)
            {
                var buffer = new byte[1024];
                while (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            Console.WriteLine($"📥 Reçu : {message}");

                            if (message.Contains("run-all"))
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ViewModelLocator.JobViewModel.ExecuteAllJobs();
                                });
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }

                _clients.TryTake(out var _);
            }


            public async Task BroadcastJobAsync(BackupJob job)
        {
            var json = JsonSerializer.Serialize(new
            {
                id = job.Id,
                name = job.Name,
                progress = job.Progress,
                status = job.Status.ToString(),
                startTime = job.StartTime,
                endTime = job.EndTime
            });

            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            foreach (var socket in _clients.Where(s => s.State == WebSocketState.Open))
            {
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
}
        public async Task SendJobUpdate(BackupJob job)
        {
            if (_wsHost != null)
                await _wsHost.BroadcastJobAsync(job);
        }



        private void TopBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
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
            if (job == null) { 
                ShowToast("⚠️ Job selection error", 3000);
                return;
            }
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
            _vm.FilterJobs(search);
            if (string.IsNullOrEmpty(search))
            {
                JobList.ItemsSource = _vm.Jobs;
            }
            else
            {
                JobList.ItemsSource = _vm.Jobs.Where(j => j.Name.ToLower().Contains(search)).ToList();
            }
        }

        private void ResetSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.ResetAllJobSelections();
            // Décoche tous les jobs (reset selection)
            foreach (var job in _vm.Jobs)
                job.IsChecked = false;
            JobList.ItemsSource = null;
            JobList.ItemsSource = _vm.Jobs;
        }

        public async void ShowToast(string message, int durationMs = 3000)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ShowToast(message, durationMs));
                return;
            }
            
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
    }
}