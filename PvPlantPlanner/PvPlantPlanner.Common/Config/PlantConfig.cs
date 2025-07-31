namespace PvPlantPlanner.Common.Config
{
    public class BaseConfig
    {
        public double InstalledPower { get; set; }
        public double MaxApprovedPower { get; set; }
        public double ConstructionPrice { get; set; }

        public double MaxGridPower { get; set; }
        public double? SelfConsumptionFactor { get; set; }

        public double ElectricityPrice { get; set; }
        public double TradingCommission { get; set; }
        public double MinSellingPrice { get; set; }
        public double MinBatteryDischargePrice { get; set; }
        public double? FixedPrice { get; set; }
        public double? NegativePrice { get; set; }

        public double MaxBatteryPower { get; set; }
        public List<BatteryDto> SelectedBatteries { get; set; } = new();
        public List<TransformerDto> SelectedTransformers { get; set; } = new();
    }

    public class CalculationConfig
    {
        public BaseConfig BaseConfig { get; set; } = new();
        public List<double>? GenerationData { get; set; }
        public List<double>? SelfConsumptionData { get; set; }
        public List<double>? MarketPrice { get; set; }
    }
    public class ImportExportConfig
    {
        public BaseConfig BaseConfig { get; set; } = new();
        public string? GenerationDataFile { get; set; }
        public string? SelfConsumptionDataFile { get; set; }
        public string? MarketPriceFile { get; set; }
    }


    public class TransformerDto
    {
        public double PowerKVA { get; set; }
        public double PowerFactor { get; set; }
        public decimal Price { get; set; }
    }

    public class BatteryDto
    {
        public double Power { get; set; }
        public double Capacity { get; set; }
        public decimal Price { get; set; }
        public int Cycles { get; set; }
    }
}

