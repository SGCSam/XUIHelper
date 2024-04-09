using NXEControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XUIHelper.GUI
{
    public class ExtensionsPageViewModel : NXEViewModelBase
    {
        private ObservableCollection<string> _RegisteredExtensions = new ObservableCollection<string>();
        private int _SelectedRegisteredExtensionIndex;
        private ICommand _AddCommand;
        private ICommand _RemoveCommand;
        private ICommand _RemoveAllCommand;
        private ICommand _NavigateBackCommand;

        public ObservableCollection<string> RegisteredExtensions
        {
            get
            {
                return _RegisteredExtensions;
            }
            private set
            {
                _RegisteredExtensions = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedRegisteredExtensionIndex
        {
            get
            {
                return _SelectedRegisteredExtensionIndex;
            }
            set
            {
                _SelectedRegisteredExtensionIndex = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand AddCommand
        {
            get
            {
                if(_AddCommand == null)
                {
                    _AddCommand = new NXERelayCommand(x => AddExtension());
                }

                return _AddCommand;
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_RemoveCommand == null)
                {
                    _RemoveCommand = new NXERelayCommand(x => RemoveExtension());
                }

                return _RemoveCommand;
            }
        }

        public ICommand RemoveAllCommand
        {
            get
            {
                if (_RemoveAllCommand == null)
                {
                    _RemoveAllCommand = new NXERelayCommand(x => RemoveAllExtensions());
                }

                return _RemoveAllCommand;
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

        private void AddExtension()
        {
            _ = Constants.HUDManager?.ShowMessageBox("Add.", "Debug");
        }

        private void RemoveExtension()
        {
            _ = Constants.HUDManager?.ShowMessageBox("Remove.", "Debug");
        }

        private void RemoveAllExtensions()
        {
            _ = Constants.HUDManager?.ShowMessageBox("Remove All.", "Debug");
        }

        private void NavigateBack()
        {
            _ = Constants.PageManager.NavigateBackAsync();
        }
    }
}
