using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Ibuki.Classes.Converters {
    public class ValueConverterGroup : List<IValueConverter>, IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string culture) {
            return this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
