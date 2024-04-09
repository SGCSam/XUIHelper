using NXEControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using XUIHelper.Core;

namespace XUIHelper.GUI
{
    public class ExtensionsPageViewModel : NXEViewModelBase
    {
        private ObservableCollection<string> _RegisteredExtensions = new ObservableCollection<string>();
        private int _SelectedRegisteredExtensionIndex;
        private bool _IsExtensionSelected;
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
                IsExtensionSelected = (SelectedRegisteredExtensionIndex >= 0 && SelectedRegisteredExtensionIndex < _RegisteredExtensions.Count);
            }
        }

        public bool IsExtensionSelected
        {
            get
            {
                return _IsExtensionSelected;
            }
            private set
            {
                _IsExtensionSelected = value;
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
            if(SelectedRegisteredExtensionIndex < 0 || SelectedRegisteredExtensionIndex >= RegisteredExtensions.Count)
            {
                return;
            }

            XMLExtensionsManager.DeregisterExtensionFile(RegisteredExtensions[SelectedRegisteredExtensionIndex]);
        }

        private async Task RemoveAllExtensions()
        {
            _ = Constants.HUDManager?.ShowMessageBox("Remove All.", "Debug");
        }

        private void NavigateBack()
        {
            _ = Constants.PageManager.NavigateBackAsync();
        }

        private void OnExtensionGroupChanged(object? sender, EventArgs e)
        {
            RegisteredExtensions.Clear();
            int oldIndex = SelectedRegisteredExtensionIndex;

            foreach (XMLExtensionsManager.XUIHelperExtensionsGroupData group in XMLExtensionsManager.Groups.Values)
            {
                foreach (XMLExtensionsManager.XUIHelperExtensionsFile extensionFile in group.ExtensionsFiles)
                {
                    RegisteredExtensions.Add(extensionFile.FilePath);
                }
            }

            NotifyPropertyChanged("RegisteredExtensions");

            if(oldIndex < RegisteredExtensions.Count)
            {
                SelectedRegisteredExtensionIndex = oldIndex;
            }
            else if(RegisteredExtensions.Count > 0)
            {
                SelectedRegisteredExtensionIndex = RegisteredExtensions.Count - 1;
            }
            else
            {
                SelectedRegisteredExtensionIndex = -1;
            }
        }

        public ExtensionsPageViewModel()
        {
            OnExtensionGroupChanged(null, EventArgs.Empty);
            XMLExtensionsManager.ExtensionGroupsChanged += OnExtensionGroupChanged;
        }
    }
}
