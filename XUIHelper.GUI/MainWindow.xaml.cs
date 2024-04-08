using System;
using System.Collections.Generic;
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

namespace XUIHelper.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO: Extensions group manager
        //TODO: Ignore properties checkbox

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Constants.Initialize(hud);
            _ = Constants.PageManager.NavigateForwardAsync(new MainMenuPage());
        }

        public MainWindow()
        {
            Loaded += OnLoaded;
            DataContext = new MainWindowViewModel();
            InitializeComponent();
        }

        ~MainWindow()
        {
            Loaded -= OnLoaded;
        }
    }
}
