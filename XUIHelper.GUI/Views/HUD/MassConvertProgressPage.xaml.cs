using NXEControls;
using XUIHelper.Core;
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
    /// Interaction logic for MassConvertProgressPage.xaml
    /// </summary>
    public partial class MassConvertProgressPage : NXEHUDUserControl
    {
        private MassConvertProgressPageViewModel _ViewModel;

        public MassConvertProgressPage(string sourceDir, XUIHelperAPI.XUIHelperSupportedFormats format, string destDir)
        {
            _ViewModel = new MassConvertProgressPageViewModel(sourceDir, format, destDir);
            DataContext = _ViewModel;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _ = _ViewModel.ConvertAsync();
        }
    }
}
