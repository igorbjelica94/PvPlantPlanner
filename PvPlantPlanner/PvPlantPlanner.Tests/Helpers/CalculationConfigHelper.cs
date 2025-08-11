using PvPlantPlanner.Common.Config;

namespace PvPlantPlanner.Tests.Helpers
{
    internal static class CalculationConfigHelper
    {
        public static CalculationConfig DeepCopyCalculationConfig(CalculationConfig source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var copy = new CalculationConfig();

            if (source.BaseConfig != null)
            {
                var baseCopy = new BaseConfig
                {
                    InstalledPower = source.BaseConfig.InstalledPower,
                    MaxApprovedPower = source.BaseConfig.MaxApprovedPower,
                    ConstructionPrice = source.BaseConfig.ConstructionPrice,
                    MaxGridPower = source.BaseConfig.MaxGridPower,
                    SelfConsumptionFactor = source.BaseConfig.SelfConsumptionFactor,
                    ElectricityPrice = source.BaseConfig.ElectricityPrice,
                    TradingCommission = source.BaseConfig.TradingCommission,
                    FixedPrice = source.BaseConfig.FixedPrice,
                    NegativePrice = source.BaseConfig.NegativePrice,
                    MaxBatteryPower = source.BaseConfig.MaxBatteryPower,
                    SelectedBatteries = source.BaseConfig.SelectedBatteries != null
                        ? source.BaseConfig.SelectedBatteries.Select(b => new BatteryDto
                        {
                            No = b.No,
                            Power = b.Power,
                            Capacity = b.Capacity,
                            Price = b.Price,
                            Cycles = b.Cycles
                        }).ToList()
                        : new List<BatteryDto>(),

                    SelectedTransformers = source.BaseConfig.SelectedTransformers != null
                        ? source.BaseConfig.SelectedTransformers.Select(t => new TransformerDto
                        {
                            No = t.No,
                            PowerKVA = t.PowerKVA,
                            PowerFactor = t.PowerFactor,
                            Price = t.Price
                        }).ToList()
                        : new List<TransformerDto>()
                };

                copy.BaseConfig = baseCopy;
            }
            else
            {
                copy.BaseConfig = null!;
            }

            copy.GenerationData = source.GenerationData != null
                ? new List<double>(source.GenerationData)
                : null!;

            copy.SelfConsumptionData = source.SelfConsumptionData != null
                ? new List<double>(source.SelfConsumptionData)
                : null;

            copy.MarketPrice = source.MarketPrice != null
                ? new List<double>(source.MarketPrice)
                : null!;

            copy.MinEnergySellingPrice = source.MinEnergySellingPrice != null
                ? new List<double>(source.MinEnergySellingPrice)
                : null;

            copy.MinBatteryEnergySellingPrice = source.MinBatteryEnergySellingPrice != null
                ? new List<double>(source.MinBatteryEnergySellingPrice)
                : null;

            return copy;
        }
    }
}
