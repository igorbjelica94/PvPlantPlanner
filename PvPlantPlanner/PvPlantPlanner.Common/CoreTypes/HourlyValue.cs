using System;

namespace PvPlantPlanner.Common.CoreTypes
{
    public class HourlyValue<T>
    {
        private readonly T? _singleValue;
        private readonly T[]? _hourlyValues;
        private readonly bool _isSingle;

        public HourlyValue(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Single value in HourlyValue cannot be null.");

            _singleValue = value;
            _isSingle = true;
        }

        public HourlyValue(T[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Hourly values in HourlyValue cannot be null or empty.", nameof(values));

            _hourlyValues = values.ToArray(); // Ensuring the array is copied to prevent external modifications.
            _isSingle = false;
        }

        public T GetValueAtHour(int hour)
        {
            if (hour < 0)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour value must be non-negative.");

            if (_isSingle)
            {
                if (_singleValue == null)
                    throw new InvalidOperationException("Single value is not initialized.");
                return _singleValue;
            }

            if (_hourlyValues == null || hour >= _hourlyValues.Length)
                throw new ArgumentOutOfRangeException(nameof(hour), $"Hour ({hour}) exceeds the length of hourly values.");

            return _hourlyValues[hour];
        }

        public T this[int hour] => GetValueAtHour(hour);

        public int Length => _isSingle ? 1 : (_hourlyValues?.Length ?? 0);

        public static implicit operator HourlyValue<T>(T value) => new HourlyValue<T>(value);

        public static implicit operator HourlyValue<T>(T[] values) => new HourlyValue<T>(values);

        public override string ToString()
        {
            if (_isSingle)
            {
                if (_singleValue == null)
                    return "Single Value: null";
                else
                    return "Single Value: " + _singleValue.ToString();
            }

            if (_hourlyValues == null)
                return "Hourly Values: null";

            var preview = string.Join(", ", _hourlyValues.Take(24)); // Takes first 24 values from collection
            return $"Hourly Values: [{preview}... total: {_hourlyValues.Length}]";
        }
    }
}
