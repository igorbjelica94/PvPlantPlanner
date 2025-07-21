using PvPlantPlanner.UI.DatabaseRepo;
using PvPlantPlanner.UI.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace PvPlantPlanner.UI.Views
{
    public partial class BatteryList : Window
    {
        private readonly IDatabaseRepository _repository;
        public ObservableCollection<Battery> Batteries { get; set; }
        public List<Battery> SelectedBatteries { get; private set; } = new List<Battery>();

        public BatteryList(IDatabaseRepository repository)
        {
            InitializeComponent();
            _repository = repository;
            LoadBatteriesFromDatabase();
            DataContext = this;
        }

        private void LoadBatteriesFromDatabase()
        {
            // Učitavanje baterija iz baze podataka
            var batteriesFromDb = _repository.GetAllBatteries();

            // Konvertovanje u ObservableCollection i dodavanje rednih brojeva
            Batteries = new ObservableCollection<Battery>(
                batteriesFromDb.Select((b, index) => new Battery
                {
                    Id = b.Id,
                    No = index + 1,
                    Power = b.Power,
                    Capacity = b.Capacity,
                    Price = b.Price,
                    Cycles = b.Cycles,
                    IsSelected = false
                }));
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedBatteries = Batteries.Where(b => b.IsSelected).ToList();

            this.DialogResult = true;
            this.Close();
        }
    }
}