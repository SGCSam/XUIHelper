using NXEControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        private ObservableCollection<string> _ExtensionGroups = new ObservableCollection<string>();
        private int _SelectedExtensionGroupIndex;
        private bool _IsExtensionGroupSelected;
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

        public ObservableCollection<string> ExtensionGroups
        {
            get
            {
                return _ExtensionGroups;
            }
            private set
            {
                _ExtensionGroups = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedExtensionGroupIndex
        {
            get
            {
                return _SelectedExtensionGroupIndex;
            }
            set
            {
                _SelectedExtensionGroupIndex = value;
                NotifyPropertyChanged();
                IsExtensionGroupSelected = (SelectedExtensionGroupIndex >= 0 && SelectedExtensionGroupIndex < _ExtensionGroups.Count);
            }
        }

        public bool IsExtensionGroupSelected
        {
            get
            {
                return _IsExtensionGroupSelected;
            }
            private set
            {
                _IsExtensionGroupSelected = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand AddCommand
        {
            get
            {
                if(_AddCommand == null)
                {
                    _AddCommand = new NXERelayCommand(x => _ = AddExtensionAsync());
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

        private async Task AddExtensionAsync()
        {
            if (SelectedExtensionGroupIndex < 0 || SelectedExtensionGroupIndex >= ExtensionGroups.Count)
            {
                _ = Constants.HUDManager?.ShowMessageBox("Please select an extension group to add extensions to.", "Invalid Extension Group", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string destPath = Path.Combine(Constants.ExtensionsDirectoryPath, ExtensionGroups[SelectedExtensionGroupIndex], Path.GetFileName(openFileDialog.FileName));
                if(File.Exists(destPath))
                {
                    _ = Constants.HUDManager?.ShowMessageBox(string.Format("Failed to add the extension as a file already exists at the destination path:\n\n{0}", destPath), "Failed to Add Extension", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                File.Copy(openFileDialog.FileName, destPath, true);
                bool successful = await XMLExtensionsManager.TryRegisterExtensionsGroupAsync(ExtensionGroups[SelectedExtensionGroupIndex], destPath);
                if(!successful)
                {
                    File.Delete(destPath);
                    _ = Constants.HUDManager?.ShowMessageBox("Failed to add the extension. Please ensure the selected XML file is valid with no malformities.", "Failed to Add Extension", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                }
            }
        }

        private void RemoveExtension()
        {
            if(SelectedRegisteredExtensionIndex < 0 || SelectedRegisteredExtensionIndex >= RegisteredExtensions.Count)
            {
                return;
            }

            string extensionFilePath = RegisteredExtensions[SelectedRegisteredExtensionIndex];
            XMLExtensionsManager.DeregisterExtensionFile(extensionFilePath);
            File.Delete(extensionFilePath);
        }

        private async Task DeregisterAllExtensions()
        {
            int buttonIndex = await Constants.HUDManager?.ShowMessageBox("Are you sure you want to remove all registered extensions?", "Remove All Extensions", new List<string>() { "Yes, remove all", "No, don't remove all"}, NXEHUD.NXEHUDIconType.Question);
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
            ExtensionGroups.Clear();

            foreach (XMLExtensionsManager.XUIHelperExtensionsGroupData group in XMLExtensionsManager.Groups.Values)
            {
                foreach (XMLExtensionsManager.XUIHelperExtensionsFile extensionFile in group.ExtensionsFiles)
                {
                    RegisteredExtensions.Add(extensionFile.FilePath);
                }

                ExtensionGroups.Add(group.GroupName);
            }

            NotifyPropertyChanged("RegisteredExtensions");
            NotifyPropertyChanged("ExtensionGroups");

            HasRegisteredExtensions = RegisteredExtensions.Count > 0;

            if(RegisteredExtensions.Count > 0)
            {
                SelectedRegisteredExtensionIndex = 0;
                IsExtensionSelected = true;
            }

            if(ExtensionGroups.Count > 0)
            {
                SelectedExtensionGroupIndex = 0;
                IsExtensionGroupSelected = true;
            }
        }

        public ExtensionsPageViewModel()
        {
            OnExtensionGroupChanged(null, EventArgs.Empty);
            XMLExtensionsManager.ExtensionGroupsChanged += OnExtensionGroupChanged;
        }
    }
}
