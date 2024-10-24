using CourseWPF.Model;
using CourseWPF.Stores;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace CourseWPF.ViewModel.Controls {
    public class StateViewModel : ViewModelBase
    {
        private ObservableCollection<TableRow> _data;
        public ObservableCollection<TableRow> Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        private ObservableCollection<ObservablePoint> _mainPlotData;
        public ObservableCollection<ObservablePoint> MainPlotData
        {
            get => _mainPlotData;
            set
            {
                _mainPlotData = value;
                OnPropertyChanged(nameof(MainPlotData));
            }
        }

        private SeriesCollection _livechartsSeries;
        public SeriesCollection LivechartsSeries {
            get => _livechartsSeries;
            set {
                _livechartsSeries = value;
                OnPropertyChanged(nameof(LivechartsSeries));
            }
        }

        private LineSeries ModuleSeries;
        private LineSeries HighSeries;
        private LineSeries LowSeries;

        private ColorsCollection _livechartsColors;
        public ColorsCollection LivechartsColors {
            get => _livechartsColors;
            set {
                _livechartsColors = value;
                OnPropertyChanged(nameof(LivechartsColors));
            }
        }

        private ObservableCollection<ObservablePoint> _lowPlotData;
        public ObservableCollection<ObservablePoint> LowPlotData
        {
            get => _lowPlotData;
            set
            {
                _lowPlotData = value;
                OnPropertyChanged(nameof(LowPlotData));
            }
        }

        private ObservableCollection<ObservablePoint> _highPlotData;
        public ObservableCollection<ObservablePoint> HighPlotData
        {
            get => _highPlotData;
            set
            {
                _highPlotData = value;
                OnPropertyChanged(nameof(HighPlotData));
            }
        }

        private bool _showBorders = true;
        public bool ShowBorders
        {
            get => _showBorders;
            set
            {
                _showBorders = value;
                OnPropertyChanged(nameof(ShowBorders));
                OnPropertyChanged(nameof(BordersVisibility));

                try {
                    LowSeries.Visibility = BordersVisibility;
                    HighSeries.Visibility = BordersVisibility;
                } catch (Exception) { /*cum*/ }
            }
        }

        public Visibility BordersVisibility { get => _showBorders ? Visibility.Visible : Visibility.Hidden; }

        private BlockStateStore _blockStateStore;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        
        public StateViewModel(BlockStateStore bss) {
            _blockStateStore = bss;
            RefillTable();

            LivechartsColors = new ColorsCollection();
            LivechartsColors.AddRange(new[] { "#e22", "#e2e", "#22e", "#ee2", "#e2e", "#2ee" }
                                  .Select(System.Windows.Media.ColorConverter.ConvertFromString)
                                  .OfType<System.Windows.Media.Color>()
                                  .ToList());

            bss.EpochAdded += id => {
                AddEpoch(id);
                RefillTable();
            };
            bss.EpochRemoved += id => {
                RemoveEpoch(id);
                RefillTable();
            };
            bss.EpochChanged += id => {
                UpdateEpoch(id);
                RefillTable();
            };

            bss.FullRefresh += () => 
            RefillTable();

            bss.TrustFactorChanged += UpdatePrediction;
            bss.ErrorFactorChanged += () => RefillTable();
        }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

        public void AddEpoch(int id, IEnumerable<BlockState.EpochData>? dataIn = null) {
            AddEpoch(_data, _mainPlotData, _highPlotData, _lowPlotData, id, dataIn);
        }

        private BlockState.EpochData RoundTo(BlockState.EpochData data, int sign) {
            return new BlockState.EpochData {
                Angle = Math.Round(data.Angle, sign),
                AngleHigh = Math.Round(data.AngleHigh, sign),
                AngleLow = Math.Round(data.AngleLow, sign),
                Module = Math.Round(data.Module, sign),
                ModuleHigh = Math.Round(data.ModuleHigh, sign),
                ModuleLow = Math.Round(data.ModuleLow, sign)
            };
        }

        private IEnumerable<BlockState.EpochData> GetData() => 
            _blockStateStore.BlockState.Data.Select(data => RoundTo(data, 5));

        public void AddEpoch(
            IList<TableRow> data, 
            ObservableCollection<ObservablePoint> mainPlotData, 
            ObservableCollection<ObservablePoint> highPlotData,
            ObservableCollection<ObservablePoint> lowPlotData,
            int id, IEnumerable<BlockState.EpochData>? dataIn = null
         ) {
            var modelData = dataIn is null ? GetData() : dataIn;

            var currentData = modelData.ElementAt(id);
            var moduleFirst = modelData.First().Module;
            data.Insert(id, new TableRow {
                Module = currentData.Module,
                ModuleHigh = currentData.ModuleHigh,
                ModuleLow = currentData.ModuleLow,
                Angle = currentData.Angle,
                AngleHigh = currentData.AngleHigh,
                AngleLow = currentData.AngleLow,
                Diff = Math.Abs(currentData.Module - moduleFirst)
            });

            mainPlotData.Insert(id, new(currentData.Module, currentData.Angle));
            highPlotData.Insert(id, new(currentData.ModuleHigh, currentData.AngleHigh));
            lowPlotData.Insert(id, new(currentData.ModuleLow, currentData.AngleLow));

            UpdateIds(data);
        }

        public void UpdatePrediction() {
            UpdateEpoch(_blockStateStore.BlockState.Data.Count() - 1);
        }

        public void UpdateEpoch(int id) {
            var modelData = GetData();
            var currentData = modelData.ElementAt(id);
            var moduleFirst = modelData.First().Module;
            var row = _data[id];

            row.Module = currentData.Module;
            row.ModuleHigh = currentData.ModuleHigh;
            row.ModuleLow = currentData.ModuleLow;
            row.Angle = currentData.Angle;
            row.AngleHigh = currentData.AngleHigh;
            row.AngleLow = currentData.AngleLow;
            row.Diff = Math.Abs(currentData.Module - moduleFirst);

            MainPlotData[id] = new(currentData.Module, currentData.Angle);
            HighPlotData[id] = new(currentData.ModuleHigh, currentData.AngleHigh);
            LowPlotData[id] = new(currentData.ModuleLow, currentData.AngleLow);

            UpdateChart();
        }

        public void RemoveEpoch(int id) {
            _data.RemoveAt(id);
            _mainPlotData.RemoveAt(id);
            _highPlotData.RemoveAt(id);
            _lowPlotData.RemoveAt(id);
            UpdateIds(_data);
            UpdateChart();
        }

        public void UpdateIds(IList<TableRow> data) {
            var modelDataCount = _blockStateStore.BlockState.Data.Count();
            for (int i = 0; i < data.Count; i++)
                data[i].Epoch = i == (modelDataCount - 1) ? "Прогноз" : i;
        }

        public void RefillTable()
        {
            var newData = new ObservableCollection<TableRow>();
            var newMainPlotData = new ObservableCollection<ObservablePoint>();
            var newHighPlotData = new ObservableCollection<ObservablePoint>();
            var newLowPlotData = new ObservableCollection<ObservablePoint>();

            var modelData = GetData();
            for (int i = 0; i < modelData.Count(); i++)
                AddEpoch(
                    newData, newMainPlotData, newHighPlotData, newLowPlotData,
                    i, modelData
                );

            Data = newData;
            MainPlotData = newMainPlotData;
            HighPlotData = newHighPlotData;
            LowPlotData = newLowPlotData;

            UpdateChart();
        }

        private void UpdateChart() {
            ModuleSeries = NewLineSeries(MainPlotData, "M(a)");
            HighSeries = NewLineSeries(HighPlotData, "M+(a)");
            LowSeries = NewLineSeries(LowPlotData, "M-(a)");

            LivechartsSeries = new SeriesCollection() { 
                ModuleSeries, HighSeries, LowSeries,
            };

            ShowBorders = true;
        }

        private static LineSeries NewLineSeries(IList<ObservablePoint> data, string title) {
            return new LineSeries {
                Title = title,
                Values = new ChartValues<ObservablePoint>(data),
                DataLabels = true,
                Fill = System.Windows.Media.Brushes.Transparent,
                LineSmoothness = 0.0,
                LabelPoint = p => {
                    var points = data.Select((p, i) => (p, i));

                    string name = "";

                    foreach ((var point, int index) in points) {
                        if (point.X == p.X && point.Y == p.Y)
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

        public class TableRow : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new(propertyName));

            private object _epoch = "none";
            public object Epoch
            {
                get => _epoch;
                set
                {
                    _epoch = value;
                    OnPropertyChanged(nameof(Epoch));
                }
            }

            private double _module;
            public double Module
            {
                get => _module;
                set
                {
                    _module = value;
                    OnPropertyChanged(nameof(Module));
                }
            }

            private double _moduleHigh;
            public double ModuleHigh
            {
                get => _moduleHigh;
                set
                {
                    _moduleHigh = value;
                    OnPropertyChanged(nameof(ModuleHigh));
                    OnPropertyChanged(nameof(MaxDiff));
                    OnPropertyChanged(nameof(Status));
                }
            }

            private double _moduleLow;
            public double ModuleLow
            {
                get => _moduleLow;
                set
                {
                    _moduleLow = value;
                    OnPropertyChanged(nameof(ModuleLow));
                    OnPropertyChanged(nameof(MaxDiff));
                    OnPropertyChanged(nameof(Status));
                }
            }

            private double _angle;
            public double Angle
            {
                get => _angle;
                set
                {
                    _angle = value;
                    OnPropertyChanged(nameof(Angle));
                }
            }

            private double _angleLow;
            public double AngleLow
            {
                get => _angleLow;
                set
                {
                    _angleLow = value;
                    OnPropertyChanged(nameof(AngleLow));
                }
            }

            private double _angleHigh;
            public double AngleHigh
            {
                get => _angleHigh;
                set
                {
                    _angleHigh = value;
                    OnPropertyChanged(nameof(AngleHigh));
                }
            }

            private double _diff;
            public double Diff
            {
                get => _diff;
                set
                {
                    _diff = value;
                    OnPropertyChanged(nameof(Diff));
                    OnPropertyChanged(nameof(Status));
                }
            }

            public double MaxDiff { get => Math.Abs(ModuleHigh - ModuleLow); }

            public string Status { get => Diff > MaxDiff ? "Превышение" : Diff == MaxDiff ? "Точка бифуркации" : "Ок"; }
        }
    }
}
