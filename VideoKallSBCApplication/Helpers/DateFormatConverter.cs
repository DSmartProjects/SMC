using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace VideoKallSBCApplication.Helpers
{
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            DateTime dt = DateTime.Parse(value.ToString());
            return dt.ToString(CultureInfo.CreateSpecificCulture(Constants.US_DATE_FORMATE));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
