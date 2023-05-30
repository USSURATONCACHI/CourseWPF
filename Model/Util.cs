using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Model {
    public class Util {
        public static void Assert(bool assertion, string errorMsg) {
            if (!assertion)
                throw new Exception($"Assertion failed: {errorMsg}");
        }

        public static IEnumerable<T> FilterDataByPointsIds<T>(IEnumerable<T> data, IEnumerable<int> pointsIds) {
            return data.Where((value, id) => pointsIds.Contains(id));
        }


        public static double ExpForecast(IEnumerable<double> data, double TrustFactor) {
            double average = Vector.Average(data);
            double prediction = average * (1.0 - TrustFactor) + data.First() * TrustFactor;

            foreach (double x in data)
                prediction = prediction * (1.0 - TrustFactor) + x * TrustFactor;

            prediction = prediction * (1.0 - TrustFactor) + Vector.Average(data.Skip(1)) * TrustFactor;
            return prediction;
        }

    }
}
