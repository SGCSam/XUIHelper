using NXEControls;
using XUIHelper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XUIHelper.GUI
{
    public class MainWindowViewModel : NXEViewModelBase
    {
        private string _TransitionLabelText = string.Empty;
        private object? _Content;

        public string TransitionLabelText
        {
            get
            {
                return _TransitionLabelText;
            }

            private set
            {
                _TransitionLabelText = value;
                NotifyPropertyChanged();
            }
        }

        public object? Content
        {
            get
            {
                return _Content;
            }

            private set
            {
                _Content = value;
                NotifyPropertyChanged();
            }
        }

        private void OnNavigatedForward(object? sender, EventArgs e)
        {
            UpdateToCurrentPage();
        }

        private void OnNavigatedBack(object? sender, EventArgs e)
        {
            UpdateToCurrentPage();
        }

        private void UpdateToCurrentPage()
        {
            Content = Constants.PageManager.CurrentPage;

            if(Content is NXEUserControl nxeUserControl)
            {
                TransitionLabelText = nxeUserControl.Title;
            }
        }



        public MainWindowViewModel()
        {
            Constants.PageManager.NavigatedForward += OnNavigatedForward;
            Constants.PageManager.NavigatedBack += OnNavigatedBack;

            _ = XUIHelperAPI.RegisterExtensionsFromDirectoryAsync(Constants.ExtensionsDirectoryPath);
        }
    }
}
