using CourseWPF.Commands;
using CourseWPF.Model;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWPF.ViewModel {
    public class Level4ViewModel : ViewModelBase {
        private Project _project;

        private ObservableCollection<PointCheck> _pointsChecks;
        public ObservableCollection<PointCheck> PointsChecks {
            get => _pointsChecks;
            set {
                _pointsChecks = value;
                OnPropertyChanged(nameof(PointsChecks));
            }
        }

        private ObservableCollection<OxyPlot.Wpf.LineSeries> _series;
        public ObservableCollection<OxyPlot.Wpf.LineSeries> Series {
            get => _series;
            set {
                _series = value;
                OnPropertyChanged(nameof(Series));
            }
        }

        public List<List<DataPoint>> SeriesData { get; set; }

        private List<int> _shownPoints;

        public ICommand ShowCommand { get; }

        public Level4ViewModel(Project project) {
            _project = project;
            _pointsChecks = new();
            _series = new();
            _shownPoints = new();
            SeriesData = new();

            for (int pointId = 0; pointId < project.PointsCount; pointId++)
                PointsChecks.Add(new PointCheck() {
                    PointName = $"{pointId + 1}",
                    IsChecked = false,
                });

            _project.PointAdded += AddPoint;
            _project.PointRemoved += RemovePoint;
            _project.EpochAdded += epochId => RecalculateAll();
            _project.EpochRemoved += epochId => RecalculateAll();
            _project.EpochChanged += epochId => RecalculateAll();

            ShowCommand = new CallbackCommand(obj => Show());
        }

        private void Show() {
            _shownPoints = GetSelectedPoints();
            RecalculateAll();
        }

        private void RecalculateAll() {
            var series = new ObservableCollection<OxyPlot.Wpf.LineSeries>();
            var seriesData = new List<List<DataPoint>>();

            foreach (var pointId in _shownPoints) {
                var serie = new OxyPlot.Wpf.LineSeries();

                var data = _project.Level4Points[pointId].Predictions
                    .Select((point, index) => new DataPoint(point, (double) index))
                    .ToList();
                seriesData.Add(data);
                serie.ItemsSource = data;
                series.Add(serie);
            }
            
            Series = series;
        }

        private List<int> GetSelectedPoints() {
            var points = new List<int>();

            foreach ((var point, int id) in PointsChecks.Select((p, i) => (p, i)))
                if (point.IsChecked)
                    points.Add(id);

            return points;
        }

        private void AddPoint(int pointId) {
            PointsChecks.RemoveAt(pointId);
            UpdateNames(pointId);
        }

        private void RemovePoint(int pointId) {
            PointsChecks.Insert(pointId, new PointCheck() {
                PointName = $"{pointId + 1}",
                IsChecked = false,
            });
            UpdateNames(pointId + 1);
        }

        private void UpdateNames(int startFrom) {
            for (int i = startFrom; i < PointsChecks.Count; i++)
                PointsChecks[i].PointName = $"{i + 1}";
        }

        public class PointCheck : INotifyPropertyChanged {
            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            private string _pointName = "";
            public string PointName {
                get => _pointName;
                set {
                    _pointName = value;
                    OnPropertyChanged(nameof(PointName));
                }
            }

            private bool _isChecked = false;
            public bool IsChecked {
                get => _isChecked;
                set {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }
    }
}
