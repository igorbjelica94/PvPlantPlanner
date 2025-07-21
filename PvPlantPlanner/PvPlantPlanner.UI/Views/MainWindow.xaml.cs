using Microsoft.Win32;
using PvPlantPlanner.UI.DatabaseRepo;
using PvPlantPlanner.UI.Models;
using PvPlantPlanner.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PvPlantPlanner.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Battery> SelectedBatteries { get; set; } = new ObservableCollection<Battery>();
        public ObservableCollection<Transformer> SelectedTransformers { get; set; } = new ObservableCollection<Transformer>();
        private readonly IDatabaseRepository _repository;

        public MainWindow()
        {
            this.Title = "AL&SA PVB";
            InitializeComponent();
            DataContext = this;
            _repository = new DatabaseRepository();
        }

        #region Button Click Events
        private void Button_Upload_Plant_Generation_Click(object sender, RoutedEventArgs e)
        {
            StatusIcon_P_Gen_Data.Text = "✔️";
            StatusIcon_P_Gen_Data.Foreground = Brushes.Green;
        }

        private void Button_Upload_EnegyMarket_Price_Click(object sender, RoutedEventArgs e)
        {
            StatusIcon_Market_Price.Text = "❌";
            StatusIcon_Market_Price.Foreground = Brushes.Red;
        }

        private void Button_AddBattery_Click(object sender, RoutedEventArgs e)
        {
            var batteryWindow = new BatteryList(_repository);
            batteryWindow.Owner = this;
            if (batteryWindow.ShowDialog() == true)
            {
                foreach (var battery in batteryWindow.SelectedBatteries)
                {
                    if (!SelectedBatteries.Contains(battery))
                    {
                        SelectedBatteries.Add(battery);
                        UpdateBatteryNumbers();
                    }
                }
            }
        }
        private void Button_AddTransformer_Click(object sender, RoutedEventArgs e)
        {
            var transformerWindow = new TransformerList(_repository);
            transformerWindow.Owner = this;
            if (transformerWindow.ShowDialog() == true)
            {
                foreach (var transformer in transformerWindow.SelectedTransformers)
                {
                    if (!SelectedTransformers.Contains(transformer))
                    {
                        SelectedTransformers.Add(transformer);
                        UpdateTransformersNumbers();
                    }
                }
            }
        }

        private void Button_Generate_Report_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && myDataGrid.SelectedItem is Battery item)
            {
                var items = myDataGrid.ItemsSource as ObservableCollection<Battery>;
                items?.Remove(item);
            }
        }

        private void DeleteBattery_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Battery battery)
            {
                SelectedBatteries.Remove(battery);
                UpdateBatteryNumbers();
            }
        }
        private void DeleteTransformer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Transformer transformer)
            {
                SelectedTransformers.Remove(transformer);
                UpdateTransformersNumbers();
            }
        }

        private void UpdateBatteryNumbers()
        {
            for (int i = 0; i < SelectedBatteries.Count; i++)
            {
                SelectedBatteries[i].No = i + 1;
            }
        }

        private void UpdateTransformersNumbers()
        {
            for (int i = 0; i < SelectedTransformers.Count; i++)
            {
                SelectedTransformers[i].No = i + 1;
            }
        }

        #endregion
        #region File Events
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void LoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Configuration File",
                Filter = "Config Files (*.json;*.xml;*.cfg)|*.json;*.xml;*.cfg|All Files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            bool? result = dialog.ShowDialog();
        }

        #endregion
        #region Equipment Events
        private void AddEquipment_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEquipmentWindow(_repository);
            window.ShowDialog();
        }
        #endregion


    }
}