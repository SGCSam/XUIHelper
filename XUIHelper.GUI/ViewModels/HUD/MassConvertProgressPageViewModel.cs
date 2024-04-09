using NXEControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core;

namespace XUIHelper.GUI
{
    public class MassConvertProgressPageViewModel : NXEViewModelBase, IXUIHelperProgressable
    {
        private float _Progress;
        private bool _IsIndeterminate;
        private string _Description = string.Empty;

        public float Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                _Progress = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsIndeterminate
        {
            get
            {
                return _IsIndeterminate;
            }
            set
            {
                _IsIndeterminate = value;
                NotifyPropertyChanged();
            }
        }

        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
                NotifyPropertyChanged();
            }
        }

        private string _SourceDirectory;
        private XUIHelperAPI.XUIHelperSupportedFormats _Format;
        private string _DestinationDirectory;

        public async Task ConvertAsync()
        {
            XUIHelperAPI.MassConversionResult result = await XUIHelperAPI.TryMassConvertDirectoryAsync(_SourceDirectory, _Format, _DestinationDirectory, this);
            if (!result.Successful)
            {
                _ = Constants.HUDManager?.ShowMessageBox("The conversion has failed. Consider enabling logging and consult the log file for more information.", "Conversion Failed", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Error, true);
            }
            else
            {
                _ = Constants.HUDManager?.ShowMessageBox(string.Format("The conversion has completed!\n\nSuccessful Conversions: {0}\nFailed Conversions: {1}\nSuccess Rate: {2}%",
                        result.SuccessfulWorkCount,
                        result.FailedWorkCount,
                        Convert.ToInt32((result.SuccessfulWorkCount / (float)(result.SuccessfulWorkCount + result.FailedWorkCount)) * 100.0f)
                        ),
                    "Conversion Completed", System.Windows.MessageBoxButton.OK, NXEHUD.NXEHUDIconType.Exclamation, true);
            }
        }

        public MassConvertProgressPageViewModel(string sourceDir, XUIHelperAPI.XUIHelperSupportedFormats format, string destDir)
        {
            _SourceDirectory = sourceDir;
            _Format = format;
            _DestinationDirectory = destDir;
        }
    }
}
