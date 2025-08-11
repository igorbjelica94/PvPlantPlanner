using DocumentFormat.OpenXml.Drawing;
using PvPlantPlanner.Common.Config;
using PvPlantPlanner.Common.CoreTypes;
using PvPlantPlanner.Common.Enums;
using PvPlantPlanner.EnergyModels.BatteryModules;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.PowerGrids;
using PvPlantPlanner.EnergyModels.PowerPlants;
using PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers;
using PvPlantPlanner.EnergyTransferSimulator.EnergyTransferSimulator;
using static PvPlantPlanner.Tests.Helpers.CalculationConfigHelper;
using static PvPlantPlanner.Common.Consts.TimeConstants;

namespace PvPlantPlanner.Tests.AnnualEnergyTransferSimulatorTests
{
    public class StandardSolarPlantOneDaySampleTests
    {
        private static Random RandomGen { get; } = new Random();
        private static double Tolerance { get; } = 0.0001;

        private CalculationConfig DefaultConfiguration = new CalculationConfig
        {
            BaseConfig = new BaseConfig
            {
                // Solar Plant Parameters
                InstalledPower = 500, // kWp
                ConstructionPrice = 750, // EUR/kWp
                // Power Grid Parameters
                MaxApprovedPower = 150, // kW
                MaxGridPower = 150, // kW
                ElectricityPrice = 0.1, // EUR/kWh
                MaxBatteryPower = 50, // kW
                SelectedBatteries = new List<BatteryDto>(),
                SelectedTransformers = new List<TransformerDto>
                {
                    Transformer10kVA,
                    Transformer15kVA,
                    Transformer50kVA
                }
            }
        };

        // Solar Plant Parameters
        private double SelfConsuptionFactor { get; } = 0.2;
        private HourlyValue<double> SelfConsuptionEnergy { get; } = 30; // kWh
        private List<double> SmallEnergyProduction { get; } = Enumerable.Range(0, 8760)  // kWh
                    .Select(i => i >= 24 && i < 48
                        ? new double[] { 
                            0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.0,
                            50.0, 60.0, 70.0, 100.0, 70.0, 70.0, 50.0, 50.0,
                            20.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }[i - 24]
                        : 30.0)
                    .ToList();
        private List<double> StandardEnergyProduction { get; } = Enumerable.Range(0, 8760)  // kWh
                    .Select(i => i >= 24 && i < 48
                        ? new double[] {
                            0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 40.0, 90.0,
                            150.0, 200.0, 240.0, 280.0, 240.0, 220.0, 200.0, 150.0,
                            80.0, 30.0, 10.0, 5.0, 0.0, 0.0, 0.0, 0.0 }[i - 24]
                        : 30.0)
                    .ToList();
        private List<double> HighEnergyProduction { get; } = Enumerable.Range(0, 8760)  // kWh
                    .Select(i => i >= 24 && i < 48
                        ? new double[] {
                            0.0, 0.0, 0.0, 0.0, 5.0, 40.0, 120.0, 220.0,
                            400.0, 480.0, 500.0, 500.0, 160.0, 500.0, 480.0, 400.0,
                            300.0, 180.0, 80.0, 20.0, 0.0, 0.0, 0.0, 0.0 }[i - 24]
                        : 30.0)
                    .ToList();
        private List<double> DynamicHighEnergyProduction { get; } = Enumerable.Range(0, 8760)  // kWh
                    .Select(i => i >= 24 && i < 48
                        ? new double[] {
                            0.0, 0.0, 0.0, 0.0, 5.0, 40.0, 120.0, 220.0,
                            200.0, 480.0, 500.0, 500.0, 160.0, 500.0, 480.0, 150.0,
                            140.0, 190.0, 80.0, 20.0, 0.0, 0.0, 0.0, 0.0}[i - 24]
                        : 30.0)
                    .ToList();

        // Power Grid Parameters
        private double ZeroTradingCommission { get; } = 0.0; // kW
        private double FeedInEnrgyPrice { get; } = 0.1; // EUR/kWh
        private double NegativeFeedInEnrgyPrice { get; } = 0.05; // EUR/kWh
        private List<double> HourlyFeedInEnrgyPrice { get; } = Enumerable.Range(0, 8760) // EUR/kWh
                    .Select(i => i >= 24 && i < 48
                        ? new double[] {
                            0.12, 0.10, 0.10, 0.11, 0.10, 0.10, 0.09, 0.08,
                            0.07, 0.05, 0.01, 0.00, 0.00, 0.00, 0.01, 0.02,
                            0.08, 0.11, 0.14, 0.15, 0.17, 0.16, 0.15, 0.13}[i - 24]
                        : RandomGen.NextDouble())
                    .ToList();
        private List<double> HourlyFeedInEnrgyPriceWithNegative { get;  } = Enumerable.Range(0, 8760) // EUR/kWh
                    .Select(i => i >= 24 && i < 49
                        ? new double[] {
                            0.12, 0.10, 0.10, 0.11, 0.10, 0.10, 0.045, 0.08,
                            0.07, 0.03, -0.01, -0.02, 0.00, 0.00, 0.01, 0.02,
                            0.1, 0.11, 0.14, 0.15, 0.09, 0.16, 0.15, 0.13,
                            0.1}[i - 24]
                        : RandomGen.NextDouble())
                    .ToList();

        // Battery Storage Parameters
        private BatteryDto BatteryModulP50C100 { get; } = new BatteryDto{ 
            No = 1, Power = 50.0, Capacity = 100.0, Price = 40000, Cycles = 6000 };

        // Transformer Parameters
        private static TransformerDto Transformer10kVA { get; } = new TransformerDto{ 
            No = 1, PowerKVA = 10, PowerFactor = 0.9, Price = 800};
        private static TransformerDto Transformer15kVA { get; } = new TransformerDto{ 
            No = 2, PowerKVA = 15, PowerFactor = 0.9, Price = 1500};
        private static TransformerDto Transformer50kVA { get; } = new TransformerDto{ 
            No = 3, PowerKVA = 50, PowerFactor = 0.9, Price = 3000};

        // Calculation Parameters
        private List<double> FeedInPriorityPrice { get; } = Enumerable.Repeat(0.05, NumberOfMonth).ToList(); // EUR/kWh
        private List<double> SellEnergyAlways { get; } = Enumerable.Repeat(double.MinValue, NumberOfMonth).ToList(); // EUR/kWh
        private List<double> SellWhenNoNegativePrice { get; } = Enumerable.Repeat(0.0, NumberOfMonth).ToList(); // EUR/kWh
        private List<double> MinBatteryDischargePrice { get; } = Enumerable.Repeat(0.095, NumberOfMonth).ToList(); // EUR/kWh

        private AnnualEnergyTransferSimulator Simulator { get; set; }

        [SetUp]
        public void Setup()
        {
            Simulator = new AnnualEnergyTransferSimulator();
        }

        [Test]
        public void SmallProduction_NotUsedStorage_StaticMarketPrice()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.BaseConfig.FixedPrice = FeedInEnrgyPrice;
            config.BaseConfig.NegativePrice = FeedInEnrgyPrice;
            config.BaseConfig.SelectedBatteries.Add(BatteryModulP50C100);
            config.GenerationData = SmallEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(Simulator.OutputCalculationData[3].EnergySalesRevenue, Is.EqualTo(28.00).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].EnergyPurchaseCost, Is.EqualTo(45.00).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualEnergyFromGrid, Is.EqualTo(450.00).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualEnergyToGrid, Is.EqualTo(280.00).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualEnergyFromBattery, Is.EqualTo(0.00).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualFullPowerHours, Is.EqualTo(0));
                Assert.That(Simulator.OutputCalculationData[3].AnnualRejectedEnergy, Is.EqualTo(0.00).Within(Tolerance));
            });
        }

        [Test]
        public void StandardProduction_OneSmallModuleStorage_StaticMarketPrice()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.BaseConfig.FixedPrice = FeedInEnrgyPrice;
            config.BaseConfig.NegativePrice = FeedInEnrgyPrice;
            config.BaseConfig.SelectedBatteries.Add(BatteryModulP50C100);
            config.GenerationData = StandardEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(Simulator.OutputCalculationData[3].EnergySalesRevenue, Is.EqualTo(136.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].EnergyPurchaseCost, Is.EqualTo(33.5).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualEnergyFromGrid, Is.EqualTo(335.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualEnergyToGrid, Is.EqualTo(1360.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualEnergyFromBattery, Is.EqualTo(100.00).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[3].AnnualFullPowerHours, Is.EqualTo(6));
                Assert.That(Simulator.OutputCalculationData[3].AnnualRejectedEnergy, Is.EqualTo(200.00).Within(Tolerance));
            });
        }

        [Test]
        public void StandardProduction_TwoSmallModuleStorage_StaticMarketPrice()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.BaseConfig.FixedPrice = FeedInEnrgyPrice;
            config.BaseConfig.NegativePrice = FeedInEnrgyPrice;
            config.BaseConfig.MaxBatteryPower = 100;
            config.BaseConfig.SelectedBatteries.Add(BatteryModulP50C100);
            config.GenerationData = StandardEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(Simulator.OutputCalculationData[4].EnergySalesRevenue, Is.EqualTo(146.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].EnergyPurchaseCost, Is.EqualTo(33.5).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualEnergyFromGrid, Is.EqualTo(335.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualEnergyToGrid, Is.EqualTo(1460.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualEnergyFromBattery, Is.EqualTo(200.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualFullPowerHours, Is.EqualTo(6));
                Assert.That(Simulator.OutputCalculationData[4].AnnualRejectedEnergy, Is.EqualTo(100.0).Within(Tolerance));
            });
        }

        [Test]
        public void StandardProduction_TwoSmallModuleStorage_DynamicMarketPrice()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.BaseConfig.MaxBatteryPower = 100;
            config.BaseConfig.SelectedBatteries.Add(BatteryModulP50C100);
            config.GenerationData = StandardEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPrice;
            config.BaseConfig.TradingCommission = 0;
            config.MinEnergySellingPrice = FeedInPriorityPrice;
            config.MinBatteryEnergySellingPrice = MinBatteryDischargePrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(Simulator.OutputCalculationData[4].EnergySalesRevenue, Is.EqualTo(52.8).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].EnergyPurchaseCost, Is.EqualTo(31.5).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualEnergyFromGrid, Is.EqualTo(315.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualEnergyToGrid, Is.EqualTo(1400.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualEnergyFromBattery, Is.EqualTo(180.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[4].AnnualFullPowerHours, Is.EqualTo(6));
                Assert.That(Simulator.OutputCalculationData[4].AnnualRejectedEnergy, Is.EqualTo(140.0).Within(Tolerance));
            });
        }

        [Test]
        public void HighProduction_FiveSmallModuleStorage_DynamicMarketPrice()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.BaseConfig.MaxBatteryPower = 250;
            config.BaseConfig.SelectedBatteries.Add(BatteryModulP50C100);
            config.GenerationData = HighEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPrice;
            config.BaseConfig.TradingCommission = 0;
            config.MinEnergySellingPrice = FeedInPriorityPrice;
            config.MinBatteryEnergySellingPrice = MinBatteryDischargePrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(Simulator.OutputCalculationData[7].EnergySalesRevenue, Is.EqualTo(147.4).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].EnergyPurchaseCost, Is.EqualTo(20.5).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualEnergyFromGrid, Is.EqualTo(205.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualEnergyToGrid, Is.EqualTo(2210.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualEnergyFromBattery, Is.EqualTo(430.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualFullPowerHours, Is.EqualTo(10));
                Assert.That(Simulator.OutputCalculationData[7].AnnualRejectedEnergy, Is.EqualTo(1660.0).Within(Tolerance));
            });
        }

        [Test]
        public void DynamicHighProduction_FiveSmallModuleStorage_DynamicMarketPriceWithNegative()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.BaseConfig.MaxBatteryPower = 250;
            config.BaseConfig.SelectedBatteries.Add(BatteryModulP50C100);
            config.GenerationData = DynamicHighEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPriceWithNegative;
            config.BaseConfig.TradingCommission = 0;
            config.MinEnergySellingPrice = FeedInPriorityPrice;
            config.MinBatteryEnergySellingPrice = MinBatteryDischargePrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(Simulator.OutputCalculationData[7].EnergySalesRevenue, Is.EqualTo(133.9).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].EnergyPurchaseCost, Is.EqualTo(23.5).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualEnergyFromGrid, Is.EqualTo(235.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualEnergyToGrid, Is.EqualTo(1790.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualEnergyFromBattery, Is.EqualTo(470.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[7].AnnualFullPowerHours, Is.EqualTo(8));
                Assert.That(Simulator.OutputCalculationData[7].AnnualRejectedEnergy, Is.EqualTo(1510.0).Within(Tolerance));
            });
        }

        [Test]
        public void SellingAlways_DynamicHighProduction_WithoutStorage_DynamicMarketPriceWithNegative()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.GenerationData = DynamicHighEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPriceWithNegative;
            config.BaseConfig.TradingCommission = 0;
            config.MinEnergySellingPrice = SellEnergyAlways;
            config.MinBatteryEnergySellingPrice = MinBatteryDischargePrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(Simulator.OutputCalculationData[0].EnergySalesRevenue, Is.EqualTo(65.95).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[0].EnergyPurchaseCost, Is.EqualTo(27.5).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[0].AnnualEnergyFromGrid, Is.EqualTo(275.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[0].AnnualEnergyToGrid, Is.EqualTo(1710.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[0].AnnualEnergyFromBattery, Is.EqualTo(0.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[0].AnnualFullPowerHours, Is.EqualTo(8));
                Assert.That(Simulator.OutputCalculationData[0].AnnualRejectedEnergy, Is.EqualTo(1630.0).Within(Tolerance));
            });
        }

        [Test]
        public void SellingWhenNoNegative_DynamicHighProduction_WithoutStorage_DynamicMarketPriceWithNegative()
        {
            // Arrange
            CalculationConfig config = DeepCopyCalculationConfig(DefaultConfiguration);
            config.BaseConfig.SelfConsumptionFactor = SelfConsuptionFactor;
            config.GenerationData = DynamicHighEnergyProduction;
            config.MarketPrice = HourlyFeedInEnrgyPriceWithNegative;
            config.BaseConfig.TradingCommission = 0;
            config.MinEnergySellingPrice = SellWhenNoNegativePrice;
            config.MinBatteryEnergySellingPrice = MinBatteryDischargePrice;

            // Act
            Simulator.ConfigureSimulator(config);
            Simulator.StartSimulation(ExcelReportOption.DoNotGenerate);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(Simulator.OutputCalculationData[1].EnergySalesRevenue, Is.EqualTo(70.45).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[1].EnergyPurchaseCost, Is.EqualTo(27.5).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[1].AnnualEnergyFromGrid, Is.EqualTo(275.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[1].AnnualEnergyToGrid, Is.EqualTo(1410.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[1].AnnualEnergyFromBattery, Is.EqualTo(0.0).Within(Tolerance));
                Assert.That(Simulator.OutputCalculationData[1].AnnualFullPowerHours, Is.EqualTo(8));
                Assert.That(Simulator.OutputCalculationData[1].AnnualRejectedEnergy, Is.EqualTo(1930.0).Within(Tolerance));
            });
        }
    }
}
