using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.Common.Helpers
{
    public static class MathHelper
    {
        public static bool ApproximatelyEqual(double a, double b, double tolerance = 1e-6)
        {
            return Math.Abs(a - b) <= tolerance;
        }
    }

}
