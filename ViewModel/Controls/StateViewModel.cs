using CourseWPF.Model;
using CourseWPF.Stores;
using OxyPlot;
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

        private IList<DataPoint> _mainPlotData;
        public IList<DataPoint> MainPlotData
        {
            get => _mainPlotData;
            set
            {
                _mainPlotData = value;
                OnPropertyChanged(nameof(MainPlotData));
            }
        }

        private IList<DataPoint> _lowPlotData;
        public IList<DataPoint> LowPlotData
        {
            get => _lowPlotData;
            set
            {
                _lowPlotData = value;
                OnPropertyChanged(nameof(LowPlotData));
            }
        }

        private IList<DataPoint> _highPlotData;
        public IList<DataPoint> HighPlotData
        {
            get => _highPlotData;
            set
            {
                _highPlotData = value;
                OnPropertyChanged(nameof(HighPlotData));
            }
        }

        private bool _showBorders;
        public bool ShowBorders
        {
            get => _showBorders;
            set
            {
                _showBorders = value;
                OnPropertyChanged(nameof(ShowBorders));
                OnPropertyChanged(nameof(BordersVisibility));
            }
        }
        public Visibility BordersVisibility { get => _showBorders ? Visibility.Visible : Visibility.Hidden; }

        private BlockStateStore _blockStateStore;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public StateViewModel(BlockStateStore bss) {
            _blockStateStore = bss;
            RefillTable();

            bss.EpochAdded += id => {
                AddEpoch(id);
                UpdatePrediction();
            };
            bss.EpochRemoved += id => {
                RemoveEpoch(id);
                UpdatePrediction();
            };
            bss.EpochChanged += id => {
                UpdateEpoch(id);
                UpdatePrediction();
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

        public void AddEpoch(
            IList<TableRow> data, 
            IList<DataPoint> mainPlotData, 
            IList<DataPoint> highPlotData, 
            IList<DataPoint> lowPlotData,
            int id, IEnumerable<BlockState.EpochData>? dataIn = null
         ) {
            var modelData = dataIn is null ? _blockStateStore.BlockState.Data : dataIn;

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
            var modelData = _blockStateStore.BlockState.Data;
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

            _mainPlotData[id] = new(currentData.Module, currentData.Angle);
            _highPlotData[id] = new(currentData.ModuleHigh, currentData.AngleHigh);
            _lowPlotData[id] = new(currentData.ModuleLow, currentData.AngleLow);
        }

        public void RemoveEpoch(int id) {
            _data.RemoveAt(id);
            _mainPlotData.RemoveAt(id);
            _highPlotData.RemoveAt(id);
            _lowPlotData.RemoveAt(id);
            UpdateIds(_data);
        }

        public void UpdateIds(IList<TableRow> data) {
            var modelDataCount = _blockStateStore.BlockState.Data.Count();
            for (int i = 0; i < data.Count; i++)
                data[i].Epoch = i == (modelDataCount - 1) ? "Прогноз" : i;
        }

        public void RefillTable()
        {
            var newData = new ObservableCollection<TableRow>();
            var newMainPlotData = new ObservableCollection<DataPoint>();
            var newHighPlotData = new ObservableCollection<DataPoint>();
            var newLowPlotData = new ObservableCollection<DataPoint>();

            var modelData = _blockStateStore.BlockState.Data;
            for (int i = 0; i < modelData.Count(); i++)
                AddEpoch(
                    newData, newMainPlotData, newHighPlotData, newLowPlotData,
                    i, modelData
                );

            Data = newData;
            MainPlotData = newMainPlotData;
            HighPlotData = newHighPlotData;
            LowPlotData = newLowPlotData;
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
