using NXEControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace XUIHelper.GUI
{
    public class MainMenuPageViewModel : NXEViewModelBase
    {
        private ICommand _SingleConvertCommand;
        private ICommand _AboutCommand;
        private ICommand _ExitCommand;

        public ICommand SingleConvertCommand
        {
            get
            {
                if (_SingleConvertCommand == null)
                {
                    _SingleConvertCommand = new NXERelayCommand(x => NavigateToSingleConvert());
                }

                return _SingleConvertCommand;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                if(_AboutCommand == null)
                {
                    _AboutCommand = new NXERelayCommand(x => NavigateToAbout());
                }

                return _AboutCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_ExitCommand == null)
                {
                    _ExitCommand = new NXERelayCommand(x => Exit());
                }

                return _ExitCommand;
            }
        }

        private void NavigateToSingleConvert()
        {
            _ = Constants.PageManager.NavigateForwardAsync(new SingleConvertPage());
        }

        private void NavigateToAbout()
        {
            _ = Constants.PageManager.NavigateForwardAsync(new AboutPage());
        }

        private void Exit()
        {
            Environment.Exit(0);
        }
    }
}
