using PvPlantPlanner.Common.Config;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PvPlantPlanner.UI.Models
{
    public class Battery : INotifyPropertyChanged
    {
        public Battery(BatteryDto dto)
        {
            Power = dto.Power;
            Capacity = dto.Capacity;
            Price = dto.Price;
            Cycles = dto.Cycles;
        }

        public Battery() { }
        private bool _isSelected;
        private int _no;

        public int Id { get; set; }
        public int No
        {
            get => _no;
            set
            {
                if (_no != value)
                {
                    _no = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Power { get; set; }
        public double Capacity { get; set; }
        public int Price { get; set; }
        public int Cycles { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Battery other)
                return false;

            return Power == other.Power &&
                   Capacity == other.Capacity &&
                   Price == other.Price &&
                   Cycles == other.Cycles;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Power, Capacity, Price, Cycles);
        }

        public override string ToString()
        {
            return $"Snaga: {Power}, Kapacitet: {Capacity}, Cena: {Price}, Broj ciklusa: {Cycles}";
        }
    }


}
