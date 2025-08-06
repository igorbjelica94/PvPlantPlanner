using PvPlantPlanner.Common.Consts;

namespace PvPlantPlanner.Common.Helpers
{
    public static class MathHelper
    {
        public const double DefaultTolerance = 1e-6;

        #region Relation functions

        public static bool IsApproximatelyEqual(double a, double b, double epsilon = DefaultTolerance) => Math.Abs(a - b) <= epsilon;
        public static bool IsGreaterThanZero(double value, double epsilon = DefaultTolerance) => value > epsilon;
        public static bool IsGreaterThanOrEqualToZero(double value, double epsilon = DefaultTolerance) => value >= -epsilon;
        public static bool IsLessThanZero(double value, double epsilon = DefaultTolerance) => value < -epsilon;
        public static bool IsLessThanOrEqualToZero(double value, double epsilon = DefaultTolerance) => value <= epsilon;

        public static bool IsApproximatelyZero(double value, double epsilon = DefaultTolerance) => Math.Abs(value) <= epsilon;
        public static bool IsGreaterThan(double a, double b, double epsilon = DefaultTolerance) => a > b + epsilon;
        public static bool IsGreaterThanOrApproxEqual(double a, double b, double epsilon = DefaultTolerance) => a >= b - epsilon;
        public static bool IsLessThan(double a, double b, double epsilon = DefaultTolerance) => a < b - epsilon;
        public static bool IsLessThanOrApproxEqual(double a, double b, double epsilon = DefaultTolerance) => a <= b + epsilon;


        #endregion

        #region Time functions

        public static int GetMonthIndexForHour(int hour)
        {
            int dayOfYear = hour / 24;

            for (int i = 0; i < TimeConstants.MonthBounds.Length; i++)
            {
                if (dayOfYear < TimeConstants.MonthBounds[i])
                {
                    return i;
                }
            }
            return 11;
        }

        #endregion
    }

}
