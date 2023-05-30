using CourseWPF.ViewModel;
using OxyPlot;
using OxyPlot.Series;
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
    /// Логика взаимодействия для Level4View.xaml
    /// </summary>
    public partial class Level4View : UserControl {
        public Level4View() {
            InitializeComponent();

            OxyPlot.Wpf.LineSeries ls = new OxyPlot.Wpf.LineSeries();
            ls.StrokeThickness = 1;
            List<DataPoint> dataPoints = new List<DataPoint> {
                new DataPoint(0, 1),
                new DataPoint(10, 100),
                new DataPoint(20, 200),
                new DataPoint(30, 300)
            };
            ls.ItemsSource = dataPoints;

            Plot.Series.Add(ls);

            // DataContextChanged += Level4View_DataContextChanged;
        }

        private void Level4View_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext is Level4ViewModel viewmodel) {
                viewmodel.PropertyChanged += Viewmodel_PropertyChanged;

                if (viewmodel.Series is not null) {
                    SetSeries(viewmodel.Series);
                    viewmodel.Series.CollectionChanged += (sender, args) => {
                        if (DataContext is Level4ViewModel vm)
                            SetSeries(vm.Series);
                    };
                } else {
                    Plot.Series.Clear();
                }
            }
        }

        private void Viewmodel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(DataContext is Level4ViewModel viewmodel && e.PropertyName == "Series") {
                if (viewmodel.Series is not null) {
                    SetSeries(viewmodel.Series);
                    viewmodel.Series.CollectionChanged += (sender, args) => {
                        if (DataContext is Level4ViewModel vm && vm.Series is not null)
                            SetSeries(vm.Series);
                    };
                } else {
                    Plot.Series.Clear();
                }
            }
        }

        private void SetSeries(IEnumerable<OxyPlot.Wpf.LineSeries> series) {
            Plot.Series.Clear();
            foreach (var item in series)
                Plot.Series.Add(item);
        }
    }
}
