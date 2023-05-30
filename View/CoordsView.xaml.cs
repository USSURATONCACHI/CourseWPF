using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
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
    /// Логика взаимодействия для CoordsView.xaml
    /// </summary>
    public partial class CoordsView : UserControl {
        public CoordsView() {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            if (DataContext is CoordsViewModel vm)
                vm.EditingEnded(sender, e);
            // Check for shit. If shit -> block editing
        }
    }
}
