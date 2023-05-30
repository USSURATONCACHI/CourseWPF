using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace CourseWPF.View.Controls {
    internal class DoubleTextBox : TextBox {

        public static readonly DependencyProperty DoubleValueProperty =
        DependencyProperty.Register(
            "DoubleValue",
            typeof(double),
            typeof(DoubleTextBox),
            new FrameworkPropertyMetadata(
                default(double),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDoubleValueChanged
            )
        );

        public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(
            "MinValue",
            typeof(double?),
            typeof(DoubleTextBox),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(
            "MaxValue",
            typeof(double?),
            typeof(DoubleTextBox),
            new PropertyMetadata(null)
        );

        public double DoubleValue {
            get => (double) GetValue(DoubleValueProperty);
            set => SetValue(DoubleValueProperty, value);
        }

        public double? MinValue {
            get => (double?) GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public double? MaxValue {
            get => (double?) GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e) {
            var futureInput = this.Text.Insert(CaretIndex, e.Text);
            e.Handled = !TryParseBothFormats(futureInput, out _);
            base.OnPreviewTextInput(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e) {
            if (TryParseBothFormats(this.Text, out double result)) {
                if (MinValue != null && result < MinValue)
                    result = (double) MinValue;

                if (MaxValue != null && result > MaxValue)
                    result = (double) MaxValue;

                this.DoubleValue = result;
            } else if (MinValue != null)
                this.DoubleValue = (double) MinValue;
            else if (MaxValue != null)
                this.DoubleValue = (double) MaxValue;
            else
                this.DoubleValue = 0.0;

            this.Text = this.DoubleValue.ToString();
            base.OnLostFocus(e);
        }

        private static void OnDoubleValueChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var doubleTextBox = (DoubleTextBox)d;
            double newValue = (double)e.NewValue;
            doubleTextBox.Text = newValue.ToString();
        }

        private static bool TryParseBothFormats(string input, out double result) {
            return
                double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result) ||
                double.TryParse(input, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result);
        }
    }
}
