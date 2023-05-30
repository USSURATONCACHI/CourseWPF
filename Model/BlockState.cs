using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Model {
    public class BlockState {
        private List<List<double>> _epochs;
        private double _trustFactor, _errorFactor;
        private List<EpochData> _data;
        private EpochData _prediction;

        public double TrustFactor {
            get => _trustFactor;
            set {
                _trustFactor = value;
                RecalculatePrediction();
            }
        }
        public double ErrorFactor {
            get => _errorFactor;
            set {
                _errorFactor = value;
                Recalculate();
            }
        }

        public EpochData Prediction { get => _prediction; }

        public IEnumerable<EpochData> Data {
            get => _data.Append(Prediction);
        }


        public BlockState(IEnumerable<IEnumerable<double>> epochs, double trustFactor, double errorFactor) {
            if (epochs.Count() < 2)
                throw new ArgumentException("BlockState.BlockState - Cannot make prediction with less than two epochs. " +
                    $"Got {epochs.Count()} epochs.");

            _epochs = epochs.Select(row => row.ToList()).ToList();
            _data = new();
            _prediction = new();
            _trustFactor = trustFactor;
            _errorFactor = errorFactor;
            Recalculate();
        }

        public void AddPoint(int atPos, IEnumerable<double> ptEpochs) {
            foreach ((var epoch, var valueToAdd) in _epochs.Zip(ptEpochs))
                epoch.Insert(atPos, valueToAdd);
            Recalculate();
        }

        public void SetPoint(int atPos, IEnumerable<double> ptEpochs) {
            foreach ((var epoch, var newValue) in _epochs.Zip(ptEpochs))
                epoch[atPos] = newValue;
            Recalculate();
        }

        public void RemovePoint(int pos) {
            foreach (var epoch in _epochs)
                epoch.RemoveAt(pos);
            Recalculate();
        }

        public void SetEpoch(int epochId, IEnumerable<double> heights) {
            Util.Assert(heights.Count() == _epochs[0].Count, 
                $"BlockState.SetEpoch - heights must be same length (it is {heights.Count()}) as points count ({_epochs[0].Count})");
            _epochs[epochId] = heights.ToList();
            Recalculate();
        }

        public void SetEpochValue(int epochId, int pointId, double value) {
            _epochs[epochId][pointId] = value;
            Recalculate();
        }

        public void AddEpoch(int atPos, IEnumerable<double> heights) {
            Util.Assert(heights.Count() == _epochs[0].Count,
                $"BlockState.AddEpoch - heights must be same length (it is {heights.Count()}) as points count ({_epochs[0].Count})");
            _epochs.Insert(atPos, heights.ToList());
            Recalculate();
        }

        public void RemoveEpoch(int fromPos) {
            if (_epochs.Count() < 2)
                throw new ArgumentException("BlockState.RemoveEpoch - Cannot proceed with less than two epochs. Cannot make prediction with <2 epochs.");

            _epochs.RemoveAt(fromPos);
            Recalculate();
        }

        public void Recalculate() {
            var epochs_count = _epochs.Count;

            var firstEpoch = _epochs.First();
            var firstEpochHigh = Vector.AddNum(firstEpoch, ErrorFactor);
            var firstEpochLow = Vector.AddNum(firstEpoch, -ErrorFactor);

            _data.Clear();
            foreach (var epoch in _epochs) {
                var epochHigh = Vector.AddNum(epoch, ErrorFactor);
                var epochLow = Vector.AddNum(epoch, -ErrorFactor);

                _data.Add(new EpochData() {
                    Module = Vector.Length(epoch),
                    ModuleHigh = Vector.Length(epochHigh),
                    ModuleLow = Vector.Length(epochLow),
                    Angle = Vector.Angle(epoch, firstEpoch),
                    AngleHigh = Vector.Angle(epochHigh, firstEpochHigh),
                    AngleLow = Vector.Angle(epochLow, firstEpochLow),
                });
            }

            RecalculatePrediction();
        }

        public void RecalculatePrediction() {
            Prediction.Module       = Util.ExpForecast(_data.Select(ed => ed.Module     ), TrustFactor);
            Prediction.ModuleHigh   = Util.ExpForecast(_data.Select(ed => ed.ModuleHigh ), TrustFactor);
            Prediction.ModuleLow    = Util.ExpForecast(_data.Select(ed => ed.ModuleLow  ), TrustFactor);
            Prediction.Angle        = Util.ExpForecast(_data.Select(ed => ed.Angle      ), TrustFactor);
            Prediction.AngleHigh    = Util.ExpForecast(_data.Select(ed => ed.AngleHigh  ), TrustFactor);
            Prediction.AngleLow     = Util.ExpForecast(_data.Select(ed => ed.AngleLow   ), TrustFactor);
        }

        public class EpochData {
            public double Module, ModuleHigh, ModuleLow;
            public double Angle, AngleHigh, AngleLow;
        }
    }
}
