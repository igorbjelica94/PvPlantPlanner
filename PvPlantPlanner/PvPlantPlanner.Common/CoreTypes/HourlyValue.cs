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
                throw new ArgumentNullException(nameof(value), "HourlyValue se ne moze inicijalizovati praznom {{null}} vrednoscu.");

            SingleValue = value;
            IsSingle = true;
        }

        public HourlyValue(T[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("HourlyValue se ne moze inicijalizovati praznim nizom.", nameof(values));

            HourlyValues = values.ToArray(); // Ensuring the array is copied to prevent external modifications.
            IsSingle = false;
        }

        public T GetValueAtHour(int hour)
        {
            if (hour < 0)
                throw new ArgumentOutOfRangeException(nameof(hour), "Vrednost za sat mora biti nenegativan broj.");

            if (IsSingle)
            {
                if (SingleValue == null)
                    throw new InvalidOperationException("Jedinstvena vrednost nije inicijalizovana u HourlyValue strukturi..");
                return SingleValue;
            }

            if (HourlyValues == null || hour >= HourlyValues.Length)
                throw new ArgumentOutOfRangeException(nameof(hour), $"Sat [{hour}] prevazilazi duzinu satnih vrednosti.");

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
