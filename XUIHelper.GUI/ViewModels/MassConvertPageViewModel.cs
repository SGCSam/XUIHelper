using Microsoft.Win32;
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
    public class MassConvertPageViewModel : NXEViewModelBase
    {
        private string _SourceDirectory = @"F:\Code Repos\XUIHelper\XUIHelper.Tests\Test Data\XUR\17559";
        private ICommand _BrowseSourceDirectoryCommand;
        private string _DestinationDirectory = @"F:\XUIHelper Example\output";
        private ICommand _BrowseDestinationDirectoryCommand;
        private bool _IgnoreProperties = true;
        private ObservableCollection<string> _OutputFileTypes = new ObservableCollection<string>() { "XUR v5", "XUR v8", "XUI v12" };
        private int _SelectedOutputFileTypeIndex = 0;
        private ObservableCollection<string> _ExtensionGroups = new ObservableCollection<string>();
        private int _SelectedExtensionGroupIndex = 0;
        private ObservableCollection<string> _LogVerbosityLevels = new ObservableCollection<string>() { "None", "Information", "Verbose" };
        private int _SelectedLogVerbosityLevelIndex = 0;
        private ICommand _NavigateBackCommand;
        private ICommand _ConvertCommand;

        public string SourceDirectory
        {
            get
            {
                return _SourceDirectory;
            }
            set
            {
                _SourceDirectory = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand BrowseSourceDirectoryCommand
        {
            get
            {
                if(_BrowseSourceDirectoryCommand == null)
                {
                    _BrowseSourceDirectoryCommand = new NXERelayCommand(x => BrowseForSourceDirectory());
                }

                return _BrowseSourceDirectoryCommand;
            }
        }

        public string DestinationDirectory
        {
            get
            {
                return _DestinationDirectory;
            }
            set
            {
                _DestinationDirectory = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand BrowseDestinationDirectoryCommand
        {
            get
            {
                if (_BrowseDestinationDirectoryCommand == null)
                {
                    _BrowseDestinationDirectoryCommand = new NXERelayCommand(x => BrowseForDestinationDirectory());
                }

                return _BrowseDestinationDirectoryCommand;
            }
        }

        public bool IgnoreProperties
        {
            get
            {
                return _IgnoreProperties;
            }
            set
            {
                _IgnoreProperties = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> OutputFileTypes
        {
            get
            {
                return _OutputFileTypes;
            }
            private set
            {
                _OutputFileTypes = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedOutputFileTypeIndex
        {
            get
            {
                return _SelectedOutputFileTypeIndex;
            }
            set
            {
                _SelectedOutputFileTypeIndex = value;
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
            }
        }

        public ObservableCollection<string> LogVerbosityLevels
        {
            get
            {
                return _LogVerbosityLevels;
            }
            private set
            {
                _LogVerbosityLevels = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedLogVerbosityLevelIndex
        {
            get
            {
                return _SelectedLogVerbosityLevelIndex;
            }
            set
            {
                _SelectedLogVerbosityLevelIndex = value;
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

        public ICommand ConvertCommand
        {
            get
            {
                if (_ConvertCommand == null)
                {
                    _ConvertCommand = new NXERelayCommand(x => _ = ConvertAsync());
                }

                return _ConvertCommand;
            }
        }

        private void BrowseForSourceDirectory()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                SourceDirectory = folderBrowserDialog.SelectedPath;
            }
        }

        private void BrowseForDestinationDirectory()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                DestinationDirectory = folderBrowserDialog.SelectedPath;
            }
        }

        private void NavigateBack()
        {
            _ = Constants.PageManager.NavigateBackAsync();
        }

        private async Task ConvertAsync()
        {
            try
            {
                if(!XUIHelperCoreUtilities.IsStringValidPath(SourceDirectory))
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The source directory is invalid.", "Invalid Directory", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                if (!XUIHelperCoreUtilities.IsStringValidPath(DestinationDirectory))
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The destination directory is invalid.", "Invalid Directory", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                if(SourceDirectory == DestinationDirectory)
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The destination directory must be different than the source directory.", "Invalid Directory", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                if(SelectedExtensionGroupIndex < 0 || SelectedExtensionGroupIndex >= ExtensionGroups.Count)
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The selected extension group is invalid.", "Invalid Selection Group", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                XUIHelperAPI.SetCurrentExtensionsGroup(ExtensionGroups[SelectedExtensionGroupIndex]);
                XUIHelperAPI.SetAreIgnoredPropertiesActive(IgnoreProperties);

                string logPath = Path.Combine(DestinationDirectory, "Conversion Log.txt");
                switch(_SelectedLogVerbosityLevelIndex)
                {
                    case 0:
                    {
                        break;
                    }
                    case 1:
                    {
                        XUIHelperAPI.SetLogger(logPath, Serilog.Events.LogEventLevel.Information);
                        break;
                    }
                    case 2:
                    {
                        XUIHelperAPI.SetLogger(logPath, Serilog.Events.LogEventLevel.Verbose);
                        break;
                    }
                    default:
                    {
                        _ = Constants.HUDManager?.ShowMessageBox(string.Format("The log verbosity index {0} was unhandled. This is an implementation bug, please contact the developer.", _SelectedLogVerbosityLevelIndex), "Invalid Logger", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                        return;
                    }
                }

                XUIHelperAPI.XUIHelperSupportedFormats? format = null;
                switch (_SelectedOutputFileTypeIndex)
                {
                    case 0:
                    {
                        format = XUIHelperAPI.XUIHelperSupportedFormats.XUR5;
                        break;
                    }
                    case 1:
                    {
                        format = XUIHelperAPI.XUIHelperSupportedFormats.XUR8;
                        break;
                    }
                    case 2:
                    {
                        format = XUIHelperAPI.XUIHelperSupportedFormats.XUI12;
                        break;
                    }
                }

                if(format == null)
                {
                    _ = Constants.HUDManager?.ShowMessageBox(string.Format("The output file type index {0} was unhandled. This is an implementation bug, please contact the developer.", _SelectedOutputFileTypeIndex), "Invalid Logger", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                _ = Constants.HUDManager?.NavigateForwardAsync(new MassConvertProgressPage(SourceDirectory, format.Value, DestinationDirectory));
            }
            catch (Exception ex)
            {
                _ = Constants.HUDManager?.ShowMessageBox(string.Format("An exception occured when converting, the conversion has failed.\n\nThe exception is: {0}", ex), "Conversion Exception", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Exclamation);
            }
        }

        public MassConvertPageViewModel()
        {
            ExtensionGroups.Clear();

            foreach (XMLExtensionsManager.XUIHelperExtensionsGroupData group in XMLExtensionsManager.Groups.Values)
            {
                ExtensionGroups.Add(group.GroupName);
            }

            NotifyPropertyChanged("ExtensionGroups");

            if (ExtensionGroups.Count > 0)
            {
                SelectedExtensionGroupIndex = 0;
            }
        }
    }
}
