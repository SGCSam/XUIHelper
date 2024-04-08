using NXEControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XUIHelper.GUI
{
    public class SingleConvertPageViewModel : NXEViewModelBase
    {
        private ICommand _NavigateBackCommand;

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
    }
}
