public MainWindow()
{
    InitializeComponent();
    
    DataContext = _vm;
    MainFrame.Navigate(new WelcomePage());
    
    // Utilisez DisplayedJobs au lieu de Jobs directement
    JobList.ItemsSource = _vm.DisplayedJobs;
    _vm.NavigateToHome = () => MainFrame.Navigate(new WelcomePage());
    ToastBridge.ShowToast = ShowToast;
}

private void SearchJobButton_Click(object sender, RoutedEventArgs e)
{
    string search = SearchBox.Text?.Trim().ToLower();
    _vm.FilterJobs(search);
    // Le binding rafraîchira automatiquement grâce à l'appel de OnPropertyChanged
}

private void ResetSelectionButton_Click(object sender, RoutedEventArgs e)
{
    _vm.ResetAllJobSelections();
    // Le binding rafraîchira automatiquement
}