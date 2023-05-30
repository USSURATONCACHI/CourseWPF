using CourseWPF.Stores;
using OxyPlot;
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


        private IList<DataPoint> _predsPlotData;
        public IList<DataPoint> PredsPlotData {
            get => _predsPlotData;
            set {
                _predsPlotData = value;
                OnPropertyChanged(nameof(PredsPlotData));
            }
        }

        private IList<DataPoint> _originalPlotData;
        public IList<DataPoint> OriginalPlotData {
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

        public PredictionViewModel(PredictionStore ps, string paramName) {
            _predictionStore = ps;
            _data = new();
            _predsPlotData = new ObservableCollection<DataPoint>();
            _originalPlotData = new ObservableCollection<DataPoint>();
            PredictionName = paramName;

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
            IList<DataPoint> newGraph = new ObservableCollection<DataPoint>(OriginalPlotData.Take(startFrom)), 
                newPreds = new ObservableCollection<DataPoint>(PredsPlotData.Take(startFrom));

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
                    newGraph.Add(new((double) i, value));
                newPreds.Add(new((double) i, newData.Last().Prediction));
            }

            for (; i < predsLength; i++) {
                newData.Add(new TableRow {
                    Epoch = "Прогноз",
                    Value = null,
                    Prediction = Math.Round(modelPredictions.ElementAt(i), 4),
                });
                newPreds.Add(new((double) i, newData.Last().Prediction));
            }

            Data = newData;
            PredsPlotData = newPreds;
            OriginalPlotData = newGraph;
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
