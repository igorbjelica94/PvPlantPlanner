using PvPlantPlanner.Common.Config;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PvPlantPlanner.UI.Models
{
    public class Transformer : INotifyPropertyChanged
    {
        public Transformer(TransformerDto dto)
        {
            PowerKVA = dto.PowerKVA;
            PowerFactor = dto.PowerFactor;
            Price = dto.Price;
        }

        public Transformer() { }

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
        public double PowerKVA { get; set; }
        public double PowerFactor { get; set; }
        public int Price { get; set; }

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
            if (obj is not Transformer other)
                return false;

            return PowerKVA == other.PowerKVA &&
                   PowerFactor == other.PowerFactor &&
                   Price == other.Price;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PowerKVA, PowerFactor, Price);
        }

        public override string ToString()
        {
            return $"Snaga: {PowerKVA}, Faktor snage: {PowerFactor}, Cena: {Price}";
        }
    }
}
