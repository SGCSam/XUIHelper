using NXEControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XUIHelper.GUI
{
    public class AboutPageViewModel : NXEViewModelBase
    {
        private Version _AppVersion = new Version(0, 0, 0, 0);
        private ICommand _NavigateBackCommand;

        public Version AppVersion
        {
            get
            {
                return _AppVersion;
            }
            private set
            {
                _AppVersion = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand NavigateBackCommand
        {
            get
            {
                if (_NavigateBackCommand == null)
                {
                    _NavigateBackCommand = new NXERelayCommand(x => NavigateBack());
                }

                return _NavigateBackCommand;
            }
        }

        private void NavigateBack()
        {
            _ = Constants.PageManager.NavigateBackAsync();
        }

        public AboutPageViewModel()
        {
            Assembly? entryAssembly = Assembly.GetEntryAssembly();
            if(entryAssembly == null)
            {
                return;
            }

            Version? ver = entryAssembly.GetName().Version;
            if(ver == null)
            {
                return;
            }

            AppVersion = ver;
        }
    }
}
