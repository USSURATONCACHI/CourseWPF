using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Model {
    public class Prediction {
        private List<double> _data;
        private List<double> _predictions;
        private double _trustFactor;


        public IReadOnlyCollection<double> Data => _data;
        public IReadOnlyCollection<double> Predictions => _predictions;
        public double TrustFactor { 
            get => _trustFactor;
            set {
                _trustFactor = value;
                Recalculate();
            }
        }

        public static Prediction Empty() => new Prediction();
        private Prediction() {
            _data = new();
            _predictions = new();
            _trustFactor = 0.0;
        }

        public Prediction(IEnumerable<double> data, double trustFactor) {
            _data = data.ToList();
            _predictions = new();
            _trustFactor = trustFactor;
            Recalculate();
        }

        public void UpdateData(IEnumerable<double> data) {
            _data = data.ToList();
            Recalculate();
        }

        public void SetValue(int at, double value) {
            _data[at] = value;
            Recalculate();
        }

        public void Recalculate() {
            _predictions.Clear();

            double prevValue = Vector.Average(_data);
            for (int i = 0; i < _data.Count; i++) {
                var prediction = _data[i] * TrustFactor + prevValue * (1.0 - TrustFactor);

                prevValue = prediction;
                _predictions.Add(prediction);
            }

            var avg = Vector.Average(_data.Skip(1));
            _predictions.Add(avg * TrustFactor + prevValue * (1.0 - TrustFactor));
        }
    }
}
