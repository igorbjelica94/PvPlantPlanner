using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.Common.Helpers
{
    public static class MathHelper
    {
        public const double DefaultTolerance = 1e-6;

        public static bool ApproximatelyEqual(double a, double b, double tolerance = DefaultTolerance) => Math.Abs(a - b) <= tolerance;
        public static bool GreaterThanZero(double value, double epsilon = DefaultTolerance) => value > epsilon;
        public static bool LessThanZero(double value, double epsilon = DefaultTolerance) => value < -epsilon;
        public static bool IsApproximatelyZero(double value, double epsilon = DefaultTolerance) => Math.Abs(value) <= epsilon;
    }

}
