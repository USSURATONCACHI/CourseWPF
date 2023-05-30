using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace CourseWPF.Util {
    public class ConcatStringExtension : MarkupExtension {
        //Converter to generate the string
        class ConcatString : IValueConverter {
            public string InitString { get; set; }

            #region IValueConverter Members
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
                //append the string
                return InitString + value.ToString();
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
                throw new NotImplementedException();
            }
            #endregion
        }
        //the value to bind to
        public Binding BindTo { get; set; }
        //the string to attach in front of the value
        public string AttachString { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            //modify the binding by setting the converter
            BindTo.Converter = new ConcatString { InitString = AttachString };
            return BindTo.ProvideValue(serviceProvider);
        }
    }
}
