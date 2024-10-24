using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Model {
    public class Vector {
        public static double Length(IEnumerable<double> vector) =>
            Math.Sqrt(vector.Sum(x => x * x));
        public static double Angle(IEnumerable<double> v1, IEnumerable<double> v2) {
            double dotProduct = v1.Zip(v2, (x, y) => x * y).Sum();
            var cos = dotProduct / (Length(v1) * Length(v2));
            return Math.Acos(cos > 1 ? 1 : cos) * (180 / Math.PI);
        }

        public static IEnumerable<double> AddNum(IEnumerable<double> v1, double add) =>
            v1.Select((a) => a + add);

        public static double Average(IEnumerable<double> v) => v.Sum() / v.Count();


        public static IEnumerable<double> VecsToModules(IEnumerable<IEnumerable<double>> vectors) =>
            vectors.Select(Vector.Length);

        public static IEnumerable<double> VecsToAngles(IEnumerable<IEnumerable<double>> vectors) {
            var first = vectors.First();
            var retval =  vectors.Select((vec, index) => Vector.Angle(vec, first));
            return retval;
        }
    }
}