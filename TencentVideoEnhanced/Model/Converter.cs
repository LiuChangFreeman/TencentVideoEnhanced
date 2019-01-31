using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace TencentVideoEnhanced.Model
{
    public class ToggleSwitchVisibilityConverter : IValueConverter
    {
        private Collection<string> ShowAsToggleButton = new Collection<string>(){ "X001", "X003", "X004", "X005" };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string id = (string)value;
            if (ShowAsToggleButton.Contains(id))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ToggleButtonVisibilityConverter : IValueConverter
    {
        private Collection<string> ShowAsToggleButton = new Collection<string>() { "X002" };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string id = (string)value;
            if (ShowAsToggleButton.Contains(id))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ToggleButtonContentConverter : IValueConverter
    {
        private Collection<string> ShowAsToggleButton = new Collection<string>() { "X002" };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool status = (bool)value;
            if (status)
            {
                return "历史页";
            }
            else
            {
                return "搜索页";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
