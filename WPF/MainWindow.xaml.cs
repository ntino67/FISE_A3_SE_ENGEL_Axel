using EasySave_2._0_from_ProSoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPF
{
    public partial class MainWindow : Window
    {
        private List<JobItem> allJobs = new List<JobItem>();

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LogoPage());

            for (int i = 1; i <= 10; i++)
            {
                allJobs.Add(new JobItem
                {
                    Name = $"Job {i}",
                    IsChecked = false,
                    IsActive = (i % 2 == 0)
                });
            }
            JobList.ItemsSource = allJobs;
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Navigation vers SettingsPage
        private void GlobalSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingsPage());
        }

        private void Bouton2_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LogsPage());
        }

        private void Bouton3_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StatusPage());
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void JobSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is JobItem jobItem)
            {
                MainFrame.Navigate(new JobPage());
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
            if (allJobs.Any(j => j.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ce nom de job existe déjà.", "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var newJob = new JobItem
            {
                Name = jobName,
                IsChecked = false,
                IsActive = false
            };
            allJobs.Add(newJob);
            JobList.ItemsSource = null;
            JobList.ItemsSource = allJobs;
            SearchBox.Text = "";
        }

        private void SearchJobButton_Click(object sender, RoutedEventArgs e)
        {
            string search = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(search))
            {
                JobList.ItemsSource = allJobs;
            }
            else
            {
                JobList.ItemsSource = allJobs.Where(j => j.Name.ToLower().Contains(search)).ToList();
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (RunComboBox.SelectedIndex == 2)
            {
                foreach (var job in allJobs)
                    job.IsChecked = false;
                JobList.ItemsSource = null;
                JobList.ItemsSource = allJobs;
            }
        }
    }

    public class JobItem
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public bool IsActive { get; set; }
    }
}