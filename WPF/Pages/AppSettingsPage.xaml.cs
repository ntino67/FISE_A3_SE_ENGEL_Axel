using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Core.Model.Interfaces;
using Core.ViewModel;
using System.IO;

namespace WPF.Pages
{

    public partial class AppSettingsPage : Page
    {
        private readonly ISettingsViewModel _viewModel;
        private readonly IUIService _uiService;

        public AppSettingsPage()
        {
            InitializeComponent();

            try
            {
                _viewModel = Infrastructure.ViewModelLocator.SettingsViewModel;
                _uiService = Infrastructure.ViewModelLocator.UiService;
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation de la page : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddExtensionButton_Click(object sender, RoutedEventArgs e)
        {
            string extension = ExtensionTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(extension))
            {
                _viewModel.AddEncryptionExtension(extension);
                ExtensionTextBox.Clear();
            }
        }

        private void RemoveExtensionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string extension)
            {
                _viewModel.RemoveEncryptionExtension(extension);
            }
        }

        private async void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.SaveSettings();

                string message = Application.Current.Resources["SettingsSaved"] as string ?? "✓ Paramètres enregistrés";
                _uiService.ShowToast(message, 3000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement des paramètres : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ReloadSettings();
            BlockingAppTextBox.Clear();
        }

        private void AddBlockingAppButton_Click(object sender, RoutedEventArgs e)
        {
            string appName = BlockingAppTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(appName))
            {
                _viewModel.AddBlockingApplication(appName);
                // Toast de confirmation
                string message = string.Format(
                    Application.Current.Resources["BlockingAppAdded"] as string ??
                    "Application bloquante ajoutée: {0}", appName);
                _uiService.ShowToast(message, 2000);

                BlockingAppTextBox.Clear();
            }
        }

        private void RemoveBlockingAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string appName)
            {
                _viewModel.RemoveBlockingApplication(appName);

                // Toast de confirmation
                string message = string.Format(
                    Application.Current.Resources["BlockingAppRemoved"] as string ??
                    "Application bloquante supprimée: {0}", appName);
                _uiService.ShowToast(message, 2000);
            }
        }

        private void OpenDailyLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _viewModel.DailyLogFilePath;
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Le fichier journal n'existe pas encore.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowDailyLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder(_viewModel.LogsDirectoryPath);
        }

        private void OpenWarningsLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _viewModel.WarningsLogFilePath;
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Le fichier journal des avertissements n'existe pas encore.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowWarningsLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder(_viewModel.LogsDirectoryPath);
        }

        private void OpenFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Le dossier n'existe pas.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du dossier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OpenStateFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _viewModel.StateFilePath;
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Le fichier d'état n'existe pas encore.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStateFileFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _viewModel.StateFilePath;
                string folderPath = Path.GetDirectoryName(filePath);
                OpenFolder(folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du dossier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenXmlLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _viewModel.XmlLogFilePath;
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Le fichier journal XML n'existe pas encore.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowXmlLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder(_viewModel.LogsDirectoryPath);
        }

        private void OpenJsonLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _viewModel.JsonLogFilePath;
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Le fichier journal JSON n'existe pas encore.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowJsonLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder(_viewModel.LogsDirectoryPath);
        }


        private void DetectRunningAppButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Récupérer tous les processus en cours
                Process[] processes = Process.GetProcesses();

                // Créer une fenêtre de dialogue pour sélectionner un processus
                var selectProcessDialog = new Window
                {
                    Title = Application.Current.Resources["RunningApps"] as string ?? "Applications en cours d'exécution",
                    Width = 500,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e2e")),
                    WindowStyle = WindowStyle.ToolWindow,
                    ResizeMode = ResizeMode.NoResize
                };

                var mainGrid = new Grid();
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.Margin = new Thickness(20);

                // En-tête
                var headerText = new TextBlock
                {
                    Text = Application.Current.Resources["RunningApps"] as string ?? "Applications en cours d'exécution",
                    FontSize = 22,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 15),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cdd6f4"))
                };
                Grid.SetRow(headerText, 0);

                // Liste des processus avec recherche
                var processGrid = new Grid();
                processGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                processGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                var searchBox = new TextBox
                {
                    Height = 40,
                    Margin = new Thickness(0, 0, 0, 10),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#313244")),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cdd6f4")),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(15, 0, 15, 0),
                    FontSize = 16,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(searchBox, 0);

                var processBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232336")),
                    CornerRadius = new CornerRadius(8)
                };
                Grid.SetRow(processBorder, 1);

                var processListView = new ListView
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cdd6f4"))
                };

                // Trier et ajouter les processus à la liste
                var processItems = processes
                    .Select(p => new { Name = p.ProcessName, Id = p.Id, DisplayName = $"{p.ProcessName} (PID: {p.Id})" })
                    .OrderBy(p => p.Name)
                    .ToList();

                processListView.ItemsSource = processItems;
                processListView.DisplayMemberPath = "DisplayName";

                // Style pour les items de la liste
                var itemContainerStyle = new Style(typeof(ListViewItem));
                itemContainerStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
                itemContainerStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10)));

                var mouseTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
                mouseTrigger.Setters.Add(new Setter(BackgroundProperty,
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#45475a"))));
                itemContainerStyle.Triggers.Add(mouseTrigger);

                processListView.ItemContainerStyle = itemContainerStyle;

                // Recherche
                searchBox.TextChanged += (s, args) =>
                {
                    string searchTerm = searchBox.Text.ToLower();
                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        processListView.ItemsSource = processItems;
                    }
                    else
                    {
                        processListView.ItemsSource = processItems
                            .Where(p => p.DisplayName.ToLower().Contains(searchTerm))
                            .ToList();
                    }
                };

                // Gestionnaire d'événements pour la sélection
                processListView.MouseDoubleClick += (s, args) =>
                {
                    if (processListView.SelectedItem != null)
                    {
                        string selectedApp = (processListView.SelectedItem as dynamic).Name;
                        BlockingAppTextBox.Text = selectedApp;
                        selectProcessDialog.Close();
                    }
                };

                processBorder.Child = processListView;

                processGrid.Children.Add(searchBox);
                processGrid.Children.Add(processBorder);
                Grid.SetRow(processGrid, 1);

                // Boutons
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 15, 0, 0)
                };
                Grid.SetRow(buttonPanel, 2);

                var selectButton = new Button
                {
                    Content = Application.Current.Resources["Select"] as string ?? "Sélectionner",
                    Height = 40,
                    Width = 120,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#74c7ec")),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232336")),
                    FontWeight = FontWeights.Bold,
                    BorderThickness = new Thickness(0)
                };

                selectButton.Click += (s, args) =>
                {
                    if (processListView.SelectedItem != null)
                    {
                        string selectedApp = (processListView.SelectedItem as dynamic).Name;
                        BlockingAppTextBox.Text = selectedApp;
                        selectProcessDialog.Close();
                    }
                };

                var cancelDialogButton = new Button
                {
                    Content = Application.Current.Resources["Cancel"] as string ?? "Annuler",
                    Height = 40,
                    Width = 100,
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#74c7ec")),
                    FontWeight = FontWeights.Bold,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#74c7ec"))
                };

                cancelDialogButton.Click += (s, args) => selectProcessDialog.Close();

                buttonPanel.Children.Add(selectButton);
                buttonPanel.Children.Add(cancelDialogButton);

                mainGrid.Children.Add(headerText);
                mainGrid.Children.Add(processGrid);
                mainGrid.Children.Add(buttonPanel);

                selectProcessDialog.Content = mainGrid;
                selectProcessDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la détection des applications en cours : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}