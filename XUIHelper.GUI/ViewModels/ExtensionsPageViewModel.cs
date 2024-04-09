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
        private bool _HasRegisteredExtensions;
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

        public bool HasRegisteredExtensions
        {
            get
            {
                return _HasRegisteredExtensions;
            }
            private set
            {
                _HasRegisteredExtensions = value;
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
                    _RemoveAllCommand = new NXERelayCommand(x => _ = DeregisterAllExtensions());
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

        private async Task DeregisterAllExtensions()
        {
            int buttonIndex = await Constants.HUDManager?.ShowMessageBox("Are you sure you want to remove all registered extensions?", "Remove All?", new List<string>() { "Yes, remove all", "No, don't remove all"}, NXEHUD.NXEHUDIconType.Question);
            if(buttonIndex == 1)
            {
                return;
            }

            XMLExtensionsManager.DeregisterAllExtensions();
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
            HasRegisteredExtensions = RegisteredExtensions.Count > 0;

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
