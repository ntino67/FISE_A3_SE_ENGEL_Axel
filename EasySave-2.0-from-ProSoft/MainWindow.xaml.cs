using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasySave_2._0_from_ProSoft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Exemple de données
            List<JobItem> JobsList = new List<JobItem>
            {
                new JobItem { Name = "Backup 1", IsChecked = true, IsActive = false },
                new JobItem { Name = "Backup 2", IsChecked = false, IsActive = true }
            };
                for (int i = 2; i < 100; i++)
                {
                    JobsList.Add(new JobItem { Name = "Backup " + (i + 1), IsChecked = false, IsActive = false });
                }
            JobList.ItemsSource = JobsList;
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

    }

}

public class JobItem
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
    public bool IsActive { get; set; }
}