using ClosedXML.Excel;
using Microsoft.Win32;
using Newtonsoft.Json;
using PvPlantPlanner.Common.Config;
using PvPlantPlanner.UI.DatabaseRepo;
using PvPlantPlanner.UI.Helpers;
using PvPlantPlanner.UI.Models;
using PvPlantPlanner.UI.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Formatting = Newtonsoft.Json.Formatting;

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

        private string generationDataFilePath;
        private string marketPriceFilePath;
        private string selfConsumptionDataFilePath;

        private List<double> generationData;
        private List<double> marketPriceData;
        private List<double>? selfConsumptionData;

        private DateTime? StartTime;

        public MainWindow()
        {
            this.Title = "AL&SA PVB";
            InitializeComponent();
            DataContext = this;
            _repository = new DatabaseRepository();
        }

        #region Button Click Events

        private async void Button_Upload_Plant_Generation_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Excel fajlovi (*.xlsx)|*.xlsx"
            };

            if (openDialog.ShowDialog() == true)
            {
                var filePath = openDialog.FileName;

                StatusIcon_P_Gen_Data.Text = "...";
                StatusIcon_P_Gen_Data.Foreground = System.Windows.Media.Brushes.Gray;

                await Task.Run(() =>
                {
                    try
                    {
                        var data = new List<double>();
                        DateTime startTime = DateTime.MinValue;
                        bool isFirst = true;

                        using (var workbook = new XLWorkbook(filePath))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed();

                            foreach (var row in rows.Skip(1))
                            {
                                var timestamp = row.Cell(1).GetDateTime();
                                var value = row.Cell(2).GetDouble();

                                if (isFirst)
                                {
                                    startTime = timestamp;
                                    isFirst = false;
                                }

                                data.Add(value);
                            }
                        }

                        // Ažuriraj UI iz glavnog threada
                        Dispatcher.Invoke(() =>
                        {
                            generationData = data;
                            StartTime = startTime;
                            StatusIcon_P_Gen_Data.Text = "✓";
                            StatusIcon_P_Gen_Data.Foreground = System.Windows.Media.Brushes.Green;
                        });
                        generationDataFilePath = filePath;
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            StatusIcon_P_Gen_Data.Text = "✗";
                            StatusIcon_P_Gen_Data.Foreground = System.Windows.Media.Brushes.Red;
                            MessageBox.Show("Greška prilikom učitavanja fajla:\n" + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }

        private async void Button_Upload_EnegyMarket_Price_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Excel fajlovi (*.xlsx)|*.xlsx|CSV fajlovi (*.csv)|*.csv"
            };

            if (openDialog.ShowDialog() == true)
            {
                var filePath = openDialog.FileName;

                // Privremeno prikaži da je u toku
                StatusIcon_Market_Price.Text = "...";
                StatusIcon_Market_Price.Foreground = System.Windows.Media.Brushes.Gray;

                await Task.Run(() =>
                {
                    try
                    {
                        var data = new List<double>();
                        DateTime startTime = DateTime.MinValue;
                        bool isFirst = true;

                        using (var workbook = new XLWorkbook(filePath))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed();

                            foreach (var row in rows.Skip(1))
                            {
                                var timestamp = row.Cell(1).GetDateTime();
                                var value = row.Cell(2).GetDouble();

                                if (isFirst)
                                {
                                    startTime = timestamp;
                                    isFirst = false;
                                }

                                data.Add(value);
                            }
                        }

                        // Ažuriraj UI i podatke iz UI threada
                        Dispatcher.Invoke(() =>
                        {
                            marketPriceData = data;
                            StartTime = startTime;
                            StatusIcon_Market_Price.Text = "✓";
                            StatusIcon_Market_Price.Foreground = System.Windows.Media.Brushes.Green;
                        });
                        marketPriceFilePath = filePath;
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            StatusIcon_Market_Price.Text = "✗";
                            StatusIcon_Market_Price.Foreground = System.Windows.Media.Brushes.Red;
                            MessageBox.Show("Greška prilikom učitavanja fajla: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }


        private async void Button_SelfConsumptionImport_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Excel fajlovi (*.xlsx)|*.xlsx|CSV fajlovi (*.csv)|*.csv"
            };

            if (openDialog.ShowDialog() == true)
            {
                var filePath = openDialog.FileName;

                // Prikaži status "Učitavanje..."
                StatusIcon_SelfConsumption.Text = "...";
                StatusIcon_SelfConsumption.Foreground = System.Windows.Media.Brushes.Gray;

                bool success = false;

                await Task.Run(() =>
                {
                    try
                    {
                        var data = new List<double>();
                        DateTime startTime = DateTime.MinValue;

                        using (var workbook = new ClosedXML.Excel.XLWorkbook(filePath))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed();
                            bool isFirst = true;

                            foreach (var row in rows.Skip(1))
                            {
                                var timestamp = row.Cell(1).GetDateTime();
                                var value = row.Cell(2).GetDouble();

                                if (isFirst)
                                {
                                    startTime = timestamp;
                                    isFirst = false;
                                }

                                data.Add(value);
                            }
                        }

                        // Prebaci rezultat u glavne promenljive u UI threadu
                        Dispatcher.Invoke(() =>
                        {
                            selfConsumptionData = data;
                            StartTime = startTime;
                            StatusIcon_SelfConsumption.Text = "✓";
                            StatusIcon_SelfConsumption.Foreground = System.Windows.Media.Brushes.Green;
                        });

                        success = true;
                        selfConsumptionDataFilePath = filePath;
    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            StatusIcon_SelfConsumption.Text = "✗";
                            StatusIcon_SelfConsumption.Foreground = System.Windows.Media.Brushes.Red;
                            MessageBox.Show("Greška prilikom učitavanja fajla:\n" + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }

        private void ExportConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Provera validnosti unosa
                if (!InputValidator.ValidateInputs(this, out string errorMessage))
                {
                    MessageBox.Show($"Greške u unosu:\n\n{errorMessage}",
                                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Kreiranje konfiguracije za eksport
                var plantConfig = this.ToImportExportConfig(
                    generationDataFilePath,
                    marketPriceFilePath,
                    selfConsumptionDataFilePath);

                // Serijalizacija u JSON
                var json = JsonConvert.SerializeObject(plantConfig, Formatting.Indented);

                // Dijalog za čuvanje fajla
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON fajlovi (*.json)|*.json",
                    FileName = "Konfiguracija_Elektrane.json",
                    DefaultExt = ".json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, json);
                    MessageBox.Show("Konfiguracija je uspešno sačuvana!",
                                  "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Došlo je do greške prilikom eksporta konfiguracije: {ex.Message}",
                              "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            // Provera validnosti unosa
            if (!InputValidator.ValidateInputs(this, out string errorMessage))
            {
                MessageBox.Show($"Greške u unosu:\n\n{errorMessage}",
                                "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kreiranje konfiguracije za eksport
            var plantConfig = this.ToCalculationConfig(
                generationData,
                marketPriceData,
                selfConsumptionData);

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

        private void SelfConsumptionFactorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (StatusIcon_SelfConsumption != null)
            {
                StatusIcon_SelfConsumption.Text = string.Empty;
                selfConsumptionDataFilePath = string.Empty;

                SelfConsumptionFactorTextBox.IsEnabled = true;
            }
        }
        private void MarketTradingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (NegativePriceTextBox != null)
            {
                NegativePriceTextBox.Text = string.Empty;
                FixedPriceTextBox.Text = string.Empty;

                FixedPriceTextBox.IsEnabled = false;
                NegativePriceTextBox.IsEnabled = false;

                TradingCommissionTextBox.IsEnabled = true;
                MinSellingPriceTextBox.IsEnabled = true;
                MinBatteryDischargePriceTextBox.IsEnabled = true;
            }
        }

        private void FixedPriceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (TradingCommissionTextBox != null)
            {
                TradingCommissionTextBox.Text = string.Empty;
                MinSellingPriceTextBox.Text = string.Empty;
                MinBatteryDischargePriceTextBox.Text = string.Empty;

                TradingCommissionTextBox.IsEnabled = false;
                MinSellingPriceTextBox.IsEnabled = false;
                MinBatteryDischargePriceTextBox.IsEnabled = false;

                NegativePriceTextBox.IsEnabled = true;
                FixedPriceTextBox.IsEnabled = true;

            }
        }

        private void SelfConsumptionHourlyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (TradingCommissionTextBox != null)
                SelfConsumptionFactorTextBox.Text = string.Empty;
            SelfConsumptionFactorTextBox.IsEnabled = false;
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
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dialog.FileName);
                    var config = JsonConvert.DeserializeObject<ImportExportConfig>(json);

                    // Update UI with loaded configuration
                    UpdateUIFromConfig(config);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading configuration: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateUIFromConfig(ImportExportConfig config)
        {
            // General plant data
            InstalledPowerTextBox.Text = config.BaseConfig.InstalledPower.ToString();
            MaxApprovedPowerTextBox.Text = config.BaseConfig.MaxApprovedPower.ToString();
            ConstructionPriceTextBox.Text = config.BaseConfig.ConstructionPrice.ToString();

            // Self-consumption section
            MaxGridPowerTextBox.Text = config.BaseConfig.MaxGridPower.ToString();
            ElectricityPriceTextBox.Text = config.BaseConfig.ElectricityPrice.ToString();

            // Handle self-consumption mode
            if (config.BaseConfig.SelfConsumptionFactor.HasValue)
            {
                SelfConsumptionFactorRadioButton.IsChecked = true;
                SelfConsumptionFactorTextBox.Text = config.BaseConfig.SelfConsumptionFactor?.ToString() ?? "0";
            }
            else if (config.SelfConsumptionDataFile is not null)
            {
                SelfConsumptionHourlyRadioButton.IsChecked = true;
                SelfConsumptionFactorTextBox.Text = string.Empty;
                selfConsumptionDataFilePath = config.SelfConsumptionDataFile;

                bool loaded = LoadSelfConsumptionFromFile(selfConsumptionDataFilePath);

                if (loaded)
                {
                    StatusIcon_SelfConsumption.Text = "✓";
                    StatusIcon_SelfConsumption.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    StatusIcon_SelfConsumption.Text = "✗";
                    StatusIcon_SelfConsumption.Foreground = System.Windows.Media.Brushes.Red;
                }
            }


            if (config.BaseConfig.FixedPrice.HasValue)
            {
                FixedPriceRadioButton.IsChecked = true;
                FixedPriceTextBox.Text = config.BaseConfig.FixedPrice.Value.ToString();
                NegativePriceTextBox.Text = config.BaseConfig.NegativePrice?.ToString() ?? "0";
            }
            else
            {
                MarketTradingRadioButton.IsChecked = true;
                TradingCommissionTextBox.Text = config.BaseConfig.TradingCommission.ToString() ?? "0";
                MinSellingPriceTextBox.Text = config.BaseConfig.MinSellingPrice.ToString() ?? "0";
                MinBatteryDischargePriceTextBox.Text = config.BaseConfig.MinBatteryDischargePrice.ToString() ?? "0";
            }

            // Battery system
            MaxBatteryPowerTextBox.Text = config.BaseConfig.MaxBatteryPower.ToString();

            // Load batteries and transformers (assuming you have methods to convert from DTO)
            SelectedBatteries.Clear();
            foreach (var dto in config.BaseConfig.SelectedBatteries ?? Enumerable.Empty<BatteryDto>())
            {
                SelectedBatteries.Add(new Battery(dto) { No = SelectedBatteries.Count + 1 });
            }

            SelectedTransformers.Clear();
            foreach (var dto in config.BaseConfig.SelectedTransformers ?? Enumerable.Empty<TransformerDto>())
            {
                SelectedTransformers.Add(new Transformer(dto) { No = SelectedTransformers.Count + 1 });
            }

            myDataGrid.Items.Refresh();

            if (!string.IsNullOrEmpty(config.GenerationDataFile))
            {
                generationDataFilePath = config.GenerationDataFile;

                bool loaded = LoadGenerationDataFromFile(generationDataFilePath);

                if (loaded)
                {
                    StatusIcon_P_Gen_Data.Text = "✓";
                    StatusIcon_P_Gen_Data.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    StatusIcon_P_Gen_Data.Text = "✗";
                    StatusIcon_P_Gen_Data.Foreground = System.Windows.Media.Brushes.Red;
                }
            }

            if (!string.IsNullOrEmpty(config.MarketPriceFile))
            {
                marketPriceFilePath = config.MarketPriceFile;

                bool loaded = LoadMarketPriceDataFromFile(marketPriceFilePath);

                if (loaded)
                {
                    StatusIcon_Market_Price.Text = "✓";
                    StatusIcon_Market_Price.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    StatusIcon_Market_Price.Text = "✗";
                    StatusIcon_Market_Price.Foreground = System.Windows.Media.Brushes.Red;
                }
            }

        }

        #endregion
        #region Equipment Events
        private void AddEquipment_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEquipmentWindow(_repository);
            window.ShowDialog();
        }
        #endregion


        private bool LoadSelfConsumptionFromFile(string filePath)
        {
            try
            {
                var data = new List<double>();
                DateTime startTime = DateTime.MinValue;
                bool isFirst = true;

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    foreach (var row in rows.Skip(1))
                    {
                        var timestamp = row.Cell(1).GetDateTime();
                        var value = row.Cell(2).GetDouble();

                        if (isFirst)
                        {
                            StartTime = timestamp;
                            isFirst = false;
                        }
                        data.Add(value);
                    }
                }
                selfConsumptionData = data;
                return true;
            }
            catch
            {

                return false;
            }
        }

        private bool LoadMarketPriceDataFromFile(string filePath)
        {
            try
            {
                var data = new List<double>();
                DateTime startTime = DateTime.MinValue;
                bool isFirst = true;

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    foreach (var row in rows.Skip(1))
                    {
                        var timestamp = row.Cell(1).GetDateTime();
                        var value = row.Cell(2).GetDouble();

                        if (isFirst)
                        {
                            StartTime = timestamp;
                            isFirst = false;
                        }
                        data.Add(value);
                    }
                }
                marketPriceData = data;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom učitavanja fajla:\n" + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool LoadGenerationDataFromFile(string filePath)
        {
            try
            {
                var data = new List<double>();
                DateTime startTime = DateTime.MinValue;
                bool isFirst = true;

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    foreach (var row in rows.Skip(1))
                    {
                        var timestamp = row.Cell(1).GetDateTime();
                        var value = row.Cell(2).GetDouble();

                        if (isFirst)
                        {
                            StartTime = timestamp;
                            isFirst = false;
                        }
                        data.Add(value);
                    }
                }
                generationData = data;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom učitavanja fajla:\n" + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


    }
}