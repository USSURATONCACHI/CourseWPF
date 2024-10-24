using CourseWPF.Stores;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CourseWPF.ViewModel.Controls {
    public class PredictionViewModel : ViewModelBase {
        private PredictionStore _predictionStore;

        private ObservableCollection<TableRow> _data;
        public ObservableCollection<TableRow> Data {
            get => _data;
            set {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }


        private IList<double> _predsPlotData;
        public IList<double> PredsPlotData {
            get => _predsPlotData;
            set {
                _predsPlotData = value;
                OnPropertyChanged(nameof(PredsPlotData));
            }
        }

        private IList<double> _originalPlotData;
        public IList<double> OriginalPlotData {
            get => _originalPlotData;
            set {
                _originalPlotData = value;
                OnPropertyChanged(nameof(OriginalPlotData));
            }
        }

        public string PlotTitle => $"{PredictionName}(t)";
        private string _predictionName = "none";
        public string PredictionName {
            get => _predictionName;
            set {
                _predictionName = value;
                OnPropertyChanged(nameof(PredictionName));
                OnPropertyChanged(nameof(PlotTitle));
            }
        }
        public string PredictionTitle => $"A = {_predictionStore.Prediction.TrustFactor}";

        private LineSeries OriginalSeries;
        private LineSeries PredictionSerires;

        private SeriesCollection _livechartsSeries;
        public SeriesCollection LivechartsSeries {
            get => _livechartsSeries;
            set {
                _livechartsSeries = value;
                OnPropertyChanged(nameof(LivechartsSeries));
            }
        }

        private ColorsCollection _livechartsColors;
        public ColorsCollection LivechartsColors {
            get => _livechartsColors;
            set {
                _livechartsColors = value;
                OnPropertyChanged(nameof(LivechartsColors));
            }
        }


        public PredictionViewModel(PredictionStore ps, string paramName) {
            _predictionStore = ps;
            _data = new();
            _predsPlotData = new ObservableCollection<double>();
            _originalPlotData = new ObservableCollection<double>();
            PredictionSerires = new();
            OriginalSeries = new();
            PredictionName = paramName;

            _livechartsColors = new();
            _livechartsSeries = new();
            LivechartsColors.AddRange(new[] { "#22e", "#28e", "#e22", "#ee2", "#e2e", "#2ee" }
                                  .Select(System.Windows.Media.ColorConverter.ConvertFromString)
                                  .OfType<System.Windows.Media.Color>()
                                  .ToList());

            _predictionStore.FullRefresh += () => FillTable();
            _predictionStore.TrustFactorChanged += () => FillTable();
            _predictionStore.EpochAdded += id => FillTable(id);
            _predictionStore.EpochRemoved += id => FillTable(id);
            _predictionStore.EpochChanged += id => {
                Debug.WriteLine($"Epoch {id} changed, refilling");
                FillTable(id);
            };

            FillTable();
        }

        public void RecalculatePrediction() => FillTable(_predictionStore.Prediction.Data.Count);

        public void FillTable(int startFrom = 0) {
            startFrom = 0;
            ObservableCollection<TableRow> newData = new(Data.Take(startFrom));
            IList<double> newGraph = new ObservableCollection<double>(OriginalPlotData.Take(startFrom)), 
                newPreds = new ObservableCollection<double>(PredsPlotData.Take(startFrom));

            var modelData = _predictionStore.Prediction.Data;
            var modelPredictions = _predictionStore.Prediction.Predictions;

            int dataLength = modelData.Count;
            int predsLength = modelPredictions.Count;

            int i = startFrom;
            for (; i < dataLength; i++) {
                newData.Add(new TableRow {
                    Epoch = i,
                    Value = Math.Round(modelData.ElementAt(i), 4),
                    Prediction = Math.Round(modelPredictions.ElementAt(i), 4),
                });

                if (newData.Last().Value is double value)
                    newGraph.Add(value);
                newPreds.Add(newData.Last().Prediction);
            }

            for (; i < predsLength; i++) {
                newData.Add(new TableRow {
                    Epoch = "Прогноз",
                    Value = null,
                    Prediction = Math.Round(modelPredictions.ElementAt(i), 4),
                });
                newPreds.Add(newData.Last().Prediction);
            }

            Data = newData;
            PredsPlotData = newPreds;
            OriginalPlotData = newGraph;

            UpdateChart();
        }

        private void UpdateChart() {
            OriginalSeries = NewLineSeries(OriginalPlotData, "Значения");
            PredictionSerires = NewLineSeries(PredsPlotData, "Прогноз");

            LivechartsSeries = new SeriesCollection() {
                OriginalSeries, PredictionSerires
            };
        }

        private static LineSeries NewLineSeries(IEnumerable<double> data, string title) {
            return new LineSeries {
                Title = title,
                Values = new ChartValues<double>(data),
                DataLabels = true,
                Fill = System.Windows.Media.Brushes.Transparent,
                LineSmoothness = 0.0,
                LabelPoint = p => {
                    var points = data.Select((p, i) => (p, i));

                    string name = "";

                    foreach ((var point, int index) in points) {
                        if (point == p.X && (double)index == p.Y)
                            if (index != (points.Count() - 1))
                                name = $"{index}";
                            else
                                name = "Прогноз";
                    }


                    return name;
                },
                FontSize = 12.0d,
            };
        }

        public class TableRow : INotifyPropertyChanged {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            private object _epoch = "<none>";
            public object Epoch {
                get => _epoch;
                set {
                    _epoch = value;
                    OnPropertyChanged(nameof(Epoch));
                }
            }

            private double? _value;
            public double? Value {
                get => _value;
                set {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }

            private double _prediction;
            public double Prediction {
                get => _prediction;
                set {
                    _prediction = value;
                    OnPropertyChanged(nameof(Prediction));
                }
            }

        }
    }
}
