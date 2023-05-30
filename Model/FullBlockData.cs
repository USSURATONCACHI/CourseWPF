using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Model {
    public class FullBlockData {
        private List<List<double>> _epochs;
        private double _trustFactor, _errorFactor;

        private BlockState _blockState;

        private Prediction _module = Prediction.Empty(), 
            _moduleHigh = Prediction.Empty(), 
            _moduleLow = Prediction.Empty();

        private Prediction _angle = Prediction.Empty(), 
            _angleHigh = Prediction.Empty(), 
            _angleLow = Prediction.Empty();

        public double TrustFactor { get => _trustFactor; set => SetTrustFactor(value); }
        public double ErrorFactor { get => _errorFactor; set => SetErrorFactor(value); }
        public BlockState BlockState { get => _blockState; }
        public Prediction ModulePrediction { get => _module; }
        public Prediction ModuleHighPrediction { get => _moduleHigh; }
        public Prediction ModuleLowPrediction { get => _moduleLow; }
        public Prediction AnglePrediction { get => _angle; }
        public Prediction AngleHighPrediction { get => _angleHigh; }
        public Prediction AngleLowPrediction { get => _angleLow; }

        public FullBlockData(IEnumerable<IEnumerable<double>> epochs, double trustFactor, double errorFactor) {
            _epochs = epochs.Select(epoch => epoch.ToList()).ToList();
            _trustFactor = trustFactor;
            _errorFactor = errorFactor;

            _blockState = new BlockState(_epochs, _trustFactor, _errorFactor);
            InitAllPredictions();
        }

        public void InitAllPredictions() {
            _module = new Prediction(_epochs.Select(Vector.Length), _trustFactor);
            _angle = new Prediction(Vector.VecsToAngles(_epochs), _trustFactor);
            InitHighLowPredictions();
        }
        public void InitHighLowPredictions() {
            var trustFactor = _trustFactor;
            var errorFactor = _errorFactor;

            var epochsHigh = _epochs.Select(epoch => Vector.AddNum(epoch, errorFactor));
            var epochsLow = _epochs.Select(epoch => Vector.AddNum(epoch, -errorFactor));

            _moduleHigh = new Prediction(epochsHigh.Select(Vector.Length), trustFactor);
            _moduleLow = new Prediction(epochsLow.Select(Vector.Length), trustFactor);

            _angleHigh = new Prediction(Vector.VecsToAngles(epochsHigh), trustFactor);
            _angleLow = new Prediction(Vector.VecsToAngles(epochsLow), trustFactor);
        }


        // Data changes
        public void AddEpoch(int pos, IEnumerable<double> epoch) {
            Util.Assert(epoch.Count() == _epochs[0].Count, $"FullBlockData.AddEpoch - length does not match " +
                $"({epoch.Count()} vs {_epochs[0].Count})");

            _epochs.Insert(pos, epoch.ToList());
            _blockState.AddEpoch(pos, epoch);
            InitAllPredictions();
        }

        public void RemoveEpoch(int pos) {
            _epochs.RemoveAt(pos);
            _blockState.RemoveEpoch(pos);
            InitAllPredictions();
        }

        public void SetEpoch(int pos, IEnumerable<double> heights) {
            Util.Assert(heights.Count() == _epochs[0].Count, $"FullBlockData.SetEpoch - length does not match " +
                $"({heights.Count()} vs {_epochs[0].Count})");

            _epochs[pos] = heights.ToList();
            _blockState.SetEpoch(pos, heights);
            InitAllPredictions();
        }

        public void SetEpochValue(int epochId, int pointId, double value) {
            _epochs[epochId][pointId] = value;
            _blockState.SetEpochValue(epochId, pointId, value);
            InitAllPredictions();
        }

        public void AddPoint(int pos, IEnumerable<double> ptEpochs) {
            foreach ((var epoch, var newValue) in _epochs.Zip(ptEpochs))
                epoch.Insert(pos, newValue);

            _blockState.AddPoint(pos, ptEpochs);
            InitAllPredictions();
        }

        public void RemovePoint(int pos) {
            foreach (var epoch in _epochs)
                epoch.RemoveAt(pos);

            _blockState.RemovePoint(pos);
            InitAllPredictions();
        }

        public void SetPoint(int pos, IEnumerable<double> newPtEpochs) {
            foreach ((var epoch, var newValue) in _epochs.Zip(newPtEpochs))
                epoch[pos] = newValue;
            _blockState.SetPoint(pos, newPtEpochs);
            InitAllPredictions();
        }


        public void SetTrustFactor(double value) {
            _trustFactor = value;
            _blockState.TrustFactor = value;

            foreach (var prediction in GetAllPredictions())
                prediction.TrustFactor = value;
        }

        public void SetErrorFactor(double value) {
            _errorFactor = value;
            _blockState.ErrorFactor = value;
            InitHighLowPredictions();
        }


        private IEnumerable<Prediction> GetAllPredictions() {
            return new List<Prediction>() {
                _module, _moduleHigh, _moduleLow,
                _angle, _angleHigh, _angleLow
            };
        }
    }
}
