﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Ibuki.Classes.Converters {
    public class BooleanNegationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return !(value is bool && (bool)value);
        }
    }
}
