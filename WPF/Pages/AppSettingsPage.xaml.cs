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

namespace WPF.Pages
{
    public partial class AppSettingsPage : Page
    {
        private readonly IConfigurationManager _configManager;
        private readonly SettingsViewModel _viewModel;

        public AppSettingsPage()
        {
            InitializeComponent();

            try
            {
                // Récupérer le ConfigurationManager depuis l'application
                _configManager = ((App)Application.Current).ConfigurationManager;

                // Créer et initialiser le ViewModel
                _viewModel = new SettingsViewModel(_configManager);
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation de la page : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.SaveSettings();

                // Animation et retour visuel
                StatusMessage.Text = "✓ Paramètres enregistrés";
                StatusMessage.Visibility = Visibility.Visible;

                // Animation de fondu
                StatusMessage.Opacity = 0;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                StatusMessage.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                // Après 3 secondes, faire disparaître le message
                await System.Threading.Tasks.Task.Delay(3000);
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                fadeOut.Completed += (s, args) => StatusMessage.Visibility = Visibility.Collapsed;
                StatusMessage.BeginAnimation(UIElement.OpacityProperty, fadeOut);
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
                BlockingAppTextBox.Clear();
            }
        }

        private void RemoveBlockingAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string appName)
            {
                _viewModel.RemoveBlockingApplication(appName);
            }
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
                    Title = "Sélectionner un processus",
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
                    Text = "Applications en cours d'exécution",
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
                    Content = "Sélectionner",
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
                    Content = "Annuler",
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


        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedLangContent = selectedItem.Content.ToString();
                string langCode = GetLanguageCodeFromContent(selectedLangContent);

                if (!string.IsNullOrEmpty(langCode))
                {
                    SetLang(langCode);
                }
            }
        }

        private string GetLanguageCodeFromContent(string content)
        {
            switch (content)
            {
                case "Français":
                    return "fr_FR";
                case "English":
                    return "en_US";
                case "Español": // If you add Spanish to your ComboBox
                    return "es_ES";
                default:
                    return "en_US"; // Default language
            }
        }

        private void SetLang(string lang)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

            Application.Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary resdict = new ResourceDictionary()
            {
                Source = new Uri($"/Utils/Language/Dictionary_{lang}.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(resdict);
        }
    }
}
