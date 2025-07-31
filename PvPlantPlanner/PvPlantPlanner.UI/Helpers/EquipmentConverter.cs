using PvPlantPlanner.Common.Config;
using PvPlantPlanner.UI.Models;

namespace PvPlantPlanner.UI.Helpers
{
    public static class EquipmentConverter
    {
        public static BatteryDto ToDto(this Battery uiModel)
        {
            return new BatteryDto
            {
                Power = uiModel.Power,
                Capacity = uiModel.Capacity,
                Price = uiModel.Price,
                Cycles = uiModel.Cycles
            };
        }

        public static TransformerDto ToDto(this Transformer uiModel)
        {
            return new TransformerDto
            {
                PowerKVA = uiModel.PowerKVA,
                PowerFactor = uiModel.PowerFactor,
                Price = uiModel.Price
            };
        }
        public static BaseConfig ToBaseConfig(this MainWindow window)
        {
            var config = new BaseConfig
            {
                InstalledPower = double.Parse(window.InstalledPowerTextBox.Text),
                MaxApprovedPower = double.Parse(window.MaxApprovedPowerTextBox.Text),
                ConstructionPrice = double.Parse(window.ConstructionPriceTextBox.Text),
                MaxGridPower = double.Parse(window.MaxGridPowerTextBox.Text),
                ElectricityPrice = double.Parse(window.ElectricityPriceTextBox.Text),
                MaxBatteryPower = double.Parse(window.MaxBatteryPowerTextBox.Text),
                SelectedBatteries = window.SelectedBatteries.Select(b => b.ToDto()).ToList(),
                SelectedTransformers = window.SelectedTransformers.Select(t => t.ToDto()).ToList()
            };

            // Self consumption
            if (window.SelfConsumptionFactorRadioButton.IsChecked == true)
            {
                config.SelfConsumptionFactor = double.Parse(window.SelfConsumptionFactorTextBox.Text);
            }

            // Price mode
            if (window.FixedPriceRadioButton.IsChecked == true)
            {
                config.FixedPrice = double.Parse(window.FixedPriceTextBox.Text);
                config.NegativePrice = double.Parse(window.NegativePriceTextBox.Text);
            }
            else
            {
                config.TradingCommission = double.Parse(window.TradingCommissionTextBox.Text);
                config.MinSellingPrice = double.Parse(window.MinSellingPriceTextBox.Text);
                config.MinBatteryDischargePrice = double.Parse(window.MinBatteryDischargePriceTextBox.Text);
            }

            return config;
        }

        public static ImportExportConfig ToImportExportConfig(this MainWindow window, string generationDataPath, string marketPricePath, string? selfConsumptionPath)
        {
            return new ImportExportConfig
            {
                BaseConfig = window.ToBaseConfig(),
                GenerationDataFile = generationDataPath,
                MarketPriceFile = marketPricePath,
                SelfConsumptionDataFile = window.SelfConsumptionFactorRadioButton.IsChecked == true ? null : selfConsumptionPath
            };
        }

        public static CalculationConfig ToCalculationConfig(this MainWindow window, List<double> generationData, List<double> marketPrice, List<double>? selfConsumption)
        {
            return new CalculationConfig
            {
                BaseConfig = window.ToBaseConfig(),
                GenerationData = generationData,
                MarketPrice = marketPrice,
                SelfConsumptionData = window.SelfConsumptionFactorRadioButton.IsChecked == true ? null : selfConsumption
            };
        }

    }
}
