using PvPlantPlanner.UI.DatabaseRepo;
using PvPlantPlanner.UI.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PvPlantPlanner.UI
{
    public partial class AddEquipmentWindow : Window
    {
        private readonly IDatabaseRepository _repository;
        private BindingList<BatteryDisplay> _batteries;
        private BindingList<TransformerDisplay> _transformers;
        private bool _isEditing = false;

        public AddEquipmentWindow(IDatabaseRepository repository)
        {
            InitializeComponent();
            _repository = repository;
            LoadData();
            SetupDataGrids();
        }

        private void LoadData()
        {
            _batteries = new BindingList<BatteryDisplay>(
                _repository.GetAllBatteries()
                          .Select((b, i) => new BatteryDisplay
                          {
                              No = i + 1,
                              Id = b.Id,
                              Power = b.Power,
                              Capacity = b.Capacity,
                              Price = b.Price,
                              Cycles = b.Cycles
                          }).ToList());

            _transformers = new BindingList<TransformerDisplay>(
                _repository.GetAllTransformers()
                           .Select((t, i) => new TransformerDisplay
                           {
                               No = i + 1,
                               Id = t.Id,
                               PowerKVA = t.PowerKVA,
                               PowerFactor = t.PowerFactor,
                               Price = t.Price
                           }).ToList());

            _batteries.AllowNew = true;
            _transformers.AllowNew = true;

            _batteries.Add(new BatteryDisplay());
            _transformers.Add(new TransformerDisplay());
        }

        private void SetupDataGrids()
        {
            BatteryDataGrid.ItemsSource = _batteries;
            TransformerDataGrid.ItemsSource = _transformers;

            BatteryDataGrid.PreviewKeyDown += BatteryDataGrid_PreviewKeyDown;
            TransformerDataGrid.PreviewKeyDown += TransformerDataGrid_PreviewKeyDown;
        }

        private void BatteryDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BatteryDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                BatteryDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                if (BatteryDataGrid.SelectedItem is BatteryDisplay battery)
                {
                    if (battery.Power == 0 && battery.Capacity == 0 && battery.Price == 0 && battery.Cycles == 0)
                    {
                        e.Handled = true;
                        return;
                    }

                    if (!ValidateBattery(battery))
                    {
                        MessageBox.Show("Unesite validne vrednosti za bateriju.");
                        e.Handled = true;
                        return;
                    }

                    SaveBattery(battery);

                    _batteries.Add(new BatteryDisplay());
                    BatteryDataGrid.SelectedIndex = _batteries.Count - 1;
                    BatteryDataGrid.ScrollIntoView(_batteries.Last());
                }
            }
        }

        private void TransformerDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TransformerDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                TransformerDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                if (TransformerDataGrid.SelectedItem is TransformerDisplay transformer)
                {
                    if (transformer.PowerKVA == 0 && transformer.PowerFactor == 0 && transformer.Price == 0)
                    {
                        e.Handled = true;
                        return;
                    }

                    if (!ValidateTransformer(transformer))
                    {
                        MessageBox.Show("Unesite validne vrednosti za transformator.");
                        e.Handled = true;
                        return;
                    }

                    SaveTransformer(transformer);

                    _transformers.Add(new TransformerDisplay());
                    TransformerDataGrid.SelectedIndex = _transformers.Count - 1;
                    TransformerDataGrid.ScrollIntoView(_transformers.Last());
                }
            }
        }

        private void SaveChanges(DataGrid grid)
        {
            _isEditing = true;
            try
            {
                if (grid == BatteryDataGrid && BatteryDataGrid.SelectedItem is BatteryDisplay battery)
                    SaveBattery(battery);
                else if (grid == TransformerDataGrid && TransformerDataGrid.SelectedItem is TransformerDisplay transformer)
                    SaveTransformer(transformer);
            }
            finally
            {
                _isEditing = false;
            }
        }

        private void SaveBattery(BatteryDisplay battery)
        {
            if (battery.Power <= 0 || battery.Capacity <= 0 || battery.Price <= 0 || battery.Cycles <= 0)
            {
                MessageBox.Show("Unesite validne vrednosti za sva polja (veće od 0)");
                return;
            }

            var batteryModel = new Battery
            {
                Id = battery.Id,
                Power = battery.Power,
                Capacity = battery.Capacity,
                Price = battery.Price,
                Cycles = battery.Cycles
            };

            if (battery.Id == 0)
            {
                var newId = _repository.AddBattery(batteryModel);
                battery.Id = newId;
            }
            else
            {
                _repository.UpdateBattery(batteryModel);
            }

            UpdateBatteryNumbers(); // <<< uvek ažuriraj brojeve
        }

        private void SaveTransformer(TransformerDisplay transformer)
        {
            if (transformer.PowerKVA <= 0 || transformer.PowerFactor <= 0 || transformer.Price <= 0)
            {
                MessageBox.Show("Unesite validne vrednosti za sva polja (veće od 0)");
                return;
            }

            var transformerModel = new Transformer
            {
                Id = transformer.Id,
                PowerKVA = transformer.PowerKVA,
                PowerFactor = transformer.PowerFactor,
                Price = transformer.Price
            };

            if (transformer.Id == 0)
            {
                var newId = _repository.AddTransformer(transformerModel);
                transformer.Id = newId;
            }
            else
            {
                _repository.UpdateTransformer(transformerModel);
            }

            UpdateTransformerNumbers(); // <<< uvek ažuriraj brojeve
        }
        private void UpdateBatteryNumbers()
        {
            for (int i = 0; i < _batteries.Count; i++)
            {
                _batteries[i].No = i + 1;
            }
        }

        private void UpdateTransformerNumbers()
        {
            for (int i = 0; i < _transformers.Count; i++)
            {
                _transformers[i].No = i + 1;
            }
        }

        private bool ValidateBattery(BatteryDisplay b)
        {
            return b.Power > 0 && b.Capacity > 0 && b.Price > 0 && b.Cycles > 0;
        }

        private bool ValidateTransformer(TransformerDisplay t)
        {
            return t.PowerKVA > 0 && t.PowerFactor > 0 && t.Price > 0;
        }

        private void DeleteSelectedBattery_Click(object sender, RoutedEventArgs e)
        {
            if (BatteryDataGrid.SelectedItem is BatteryDisplay selected)
            {
                if (selected.Id > 0)
                    _repository.DeleteBattery(selected.Id);

                _batteries.Remove(selected);
                UpdateNumbers(_batteries);
            }
        }

        private void DeleteSelectedTransformer_Click(object sender, RoutedEventArgs e)
        {
            if (TransformerDataGrid.SelectedItem is TransformerDisplay selected)
            {
                if (selected.Id > 0)
                    _repository.DeleteTransformer(selected.Id);

                _transformers.Remove(selected);
                UpdateNumbers(_transformers);
            }
        }

        private void UpdateNumbers<T>(BindingList<T> list) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var noProp = item.GetType().GetProperty(nameof(BatteryDisplay.No));
                noProp?.SetValue(item, i + 1);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public class BatteryDisplay : INotifyPropertyChanged
        {
            private int _no, _id, _cycles;
            private double _power, _capacity;
            private int _price;

            public int No { get => _no; set { _no = value; OnPropertyChanged(); } }
            public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }
            public double Power { get => _power; set { _power = value; OnPropertyChanged(); } }
            public double Capacity { get => _capacity; set { _capacity = value; OnPropertyChanged(); } }
            public int Price { get => _price; set { _price = value; OnPropertyChanged(); } }
            public int Cycles { get => _cycles; set { _cycles = value; OnPropertyChanged(); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class TransformerDisplay : INotifyPropertyChanged
        {
            private int _no, _id;
            private double _powerKVA, _powerFactor;
            private int _price;

            public int No { get => _no; set { _no = value; OnPropertyChanged(); } }
            public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }
            public double PowerKVA { get => _powerKVA; set { _powerKVA = value; OnPropertyChanged(); } }
            public double PowerFactor { get => _powerFactor; set { _powerFactor = value; OnPropertyChanged(); } }
            public int Price { get => _price; set { _price = value; OnPropertyChanged(); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
