using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CourseWPF.View {
    /// <summary>
    /// Логика взаимодействия для StateView.xaml
    /// </summary>
    public partial class StateView : UserControl {
        public StateView() {
            InitializeComponent();
            CheckedShowModules(this, new());
            CheckedShowAngles(this, new());
            CheckedShowCalcs(this, new());
        }

        public void CheckedShowModules(object? sender, RoutedEventArgs e) {
            ShowHideColumns(ShowModulesCheckbox, new string[] { "M", "M+", "M-" });
        }

        public void CheckedShowAngles(object? sender, RoutedEventArgs e) {
            ShowHideColumns(ShowAnglesCheckbox, new string[] { "A", "A+", "A-" });
        }

        public void CheckedShowCalcs(object? sender, RoutedEventArgs e) {
            ShowHideColumns(ShowCalcsCheckbox, new string[] { "L", "2E" });
        }

        private void ShowHideColumns(CheckBox checkbox, IEnumerable<string> columns) {
            var visibility = (checkbox.IsChecked ?? false) ? Visibility.Visible : Visibility.Hidden;

            var cols = MainGrid.Columns.Where(col => columns.Contains(col.Header.ToString()));
            foreach (var col in cols)
                col.Visibility = visibility;
        }
    }
}
