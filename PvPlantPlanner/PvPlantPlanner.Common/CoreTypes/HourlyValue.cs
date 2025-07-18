using System;

namespace PvPlantPlanner.Common.CoreTypes
{
    public class HourlyValue<T>
    {
        private T? SingleValue { get; }
        private T[]? HourlyValues { get; }
        private bool IsSingle { get; }

        public HourlyValue(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Single value in HourlyValue cannot be null.");

            SingleValue = value;
            IsSingle = true;
        }

        public HourlyValue(T[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Hourly values in HourlyValue cannot be null or empty.", nameof(values));

            HourlyValues = values.ToArray(); // Ensuring the array is copied to prevent external modifications.
            IsSingle = false;
        }

        public T GetValueAtHour(int hour)
        {
            if (hour < 0)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour value must be non-negative.");

            if (IsSingle)
            {
                if (SingleValue == null)
                    throw new InvalidOperationException("Single value is not initialized.");
                return SingleValue;
            }

            if (HourlyValues == null || hour >= HourlyValues.Length)
                throw new ArgumentOutOfRangeException(nameof(hour), $"Hour ({hour}) exceeds the length of hourly values.");

            return HourlyValues[hour];
        }

        public T this[int hour] => GetValueAtHour(hour);

        public int Length => IsSingle ? 1 : (HourlyValues?.Length ?? 0);

        public static implicit operator HourlyValue<T>(T value) => new HourlyValue<T>(value);

        public static implicit operator HourlyValue<T>(T[] values) => new HourlyValue<T>(values);

        public override string ToString()
        {
            if (IsSingle)
            {
                if (SingleValue == null)
                    return "Single Value: null";
                else
                    return "Single Value: " + SingleValue.ToString();
            }

            if (HourlyValues == null)
                return "Hourly Values: null";

            var preview = string.Join(", ", HourlyValues.Take(24)); // Takes first 24 values from collection
            return $"Hourly Values: [{preview}... total: {HourlyValues.Length}]";
        }
    }
}
