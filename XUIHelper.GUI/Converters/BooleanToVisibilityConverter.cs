using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace XUIHelper.GUI
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            bool param = true;

            if (value is bool)
            {
                flag = (bool)value;
            }

            else if (value is bool?)
            {
                bool? nullable = (bool?)value;
                flag = nullable.HasValue ? nullable.Value : false;
            }

            bool outBool = true;

            if (parameter != null)
            {
                try
                {
                    if (Boolean.TryParse(parameter.ToString(), out outBool))
                    {
                        param = outBool;
                    }
                }

                catch { }
            }

            if (!param)
            {
                flag = !flag;
            }

            return (flag ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
