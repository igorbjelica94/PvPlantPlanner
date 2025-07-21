using PvPlantPlanner.UI.DatabaseRepo;
using PvPlantPlanner.UI.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace PvPlantPlanner.UI.Views
{
    public partial class TransformerList : Window
    {
        private readonly IDatabaseRepository _repository;
        public ObservableCollection<Transformer> Transformers { get; set; }
        public List<Transformer> SelectedTransformers { get; private set; } = new List<Transformer>();

        public TransformerList(IDatabaseRepository repository)
        {
            InitializeComponent();
            _repository = repository;
            LoadTransformersFromDatabase();
            DataContext = this;
        }

        private void LoadTransformersFromDatabase()
        {
            var transformersFromDb = _repository.GetAllTransformers();

            Transformers = new ObservableCollection<Transformer>(
                transformersFromDb.Select((t, index) => new Transformer
                {
                    Id = t.Id,
                    No = index + 1,
                    PowerKVA = t.PowerKVA,
                    PowerFactor = t.PowerFactor,
                    Price = t.Price,
                    IsSelected = false
                }));
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTransformers = Transformers.Where(t => t.IsSelected).ToList();
            this.DialogResult = true;
            this.Close();
        }
    }
}