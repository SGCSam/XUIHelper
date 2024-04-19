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
    public class SingleConvertPageViewModel : NXEViewModelBase
    {
        private string _SourceFilePath;
        private ICommand _BrowseSourceFilePathCommand;
        private string _DestinationFilePath;
        private ICommand _BrowseDestinationFilePathCommand;
        private bool _IgnoreProperties = true;
        private ObservableCollection<string> _OutputFileTypes = new ObservableCollection<string>() { "XUR v5", "XUR v8", "XUI v12" };
        private int _SelectedOutputFileTypeIndex = 0;
        private ObservableCollection<string> _ExtensionGroups = new ObservableCollection<string>();
        private int _SelectedExtensionGroupIndex = 0;
        private ObservableCollection<string> _LogVerbosityLevels = new ObservableCollection<string>() { "None", "Information", "Verbose" };
        private int _SelectedLogVerbosityLevelIndex = 0;
        private ICommand _NavigateBackCommand;
        private ICommand _ConvertCommand;
        private bool _IsConverting;

        public string SourceFilePath
        {
            get
            {
                return _SourceFilePath;
            }
            set
            {
                _SourceFilePath = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand BrowseSourceFilePathCommand
        {
            get
            {
                if(_BrowseSourceFilePathCommand == null)
                {
                    _BrowseSourceFilePathCommand = new NXERelayCommand(x => BrowseForSourceFilePath());
                }

                return _BrowseSourceFilePathCommand;
            }
        }

        public string DestinationFilePath
        {
            get
            {
                return _DestinationFilePath;
            }
            set
            {
                _DestinationFilePath = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand BrowseDestinationFilePathCommand
        {
            get
            {
                if (_BrowseDestinationFilePathCommand == null)
                {
                    _BrowseDestinationFilePathCommand = new NXERelayCommand(x => BrowseForDestinationFilePath());
                }

                return _BrowseDestinationFilePathCommand;
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

        public bool IsConverting
        {
            get
            {
                return _IsConverting;
            }
            private set
            {
                _IsConverting = value;
                NotifyPropertyChanged();
            }
        }
        
        private void BrowseForSourceFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XUI files (*.xui)|*.xui|XUR files (*.xur)|*.xur";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SourceFilePath = openFileDialog.FileName;
            }
        }

        private void BrowseForDestinationFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = false;
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                DestinationFilePath = openFileDialog.FileName;
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
                IsConverting = true;

                if(!XUIHelperCoreUtilities.IsStringValidPath(SourceFilePath))
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The source file path is invalid.", "Invalid File Path", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                if (!XUIHelperCoreUtilities.IsStringValidPath(DestinationFilePath))
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The destination file path is invalid.", "Invalid File Path", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                if(SelectedExtensionGroupIndex < 0 || SelectedExtensionGroupIndex >= ExtensionGroups.Count)
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The selected extension group is invalid.", "Invalid Selection Group", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                    return;
                }

                XUIHelperAPI.SetCurrentExtensionsGroup(ExtensionGroups[SelectedExtensionGroupIndex]);
                XUIHelperAPI.SetAreIgnoredPropertiesActive(IgnoreProperties);

                string logPath = Path.Combine(Path.GetDirectoryName(DestinationFilePath), "Conversion Log.txt");
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

                IsConverting = true;
                bool successful = await XUIHelperAPI.TryConvertAsync(SourceFilePath, format.Value, DestinationFilePath);
                IsConverting = false;

                if (!successful)
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The conversion has failed. Consider enabling logging and consult the log file for more information.", "Conversion Failed", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error);
                }
                else
                {
                    _ = Constants.HUDManager?.ShowMessageBox("The conversion was successful!", "Conversion Succeeded", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Exclamation);
                }
            }
            catch (Exception ex)
            {
                _ = Constants.HUDManager?.ShowMessageBox(string.Format("An exception occured when converting, the conversion has failed.\n\nThe exception is: {0}", ex), "Conversion Exception", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Exclamation);
            }
            finally
            {
                IsConverting = false;
            }
        }

        public SingleConvertPageViewModel()
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
