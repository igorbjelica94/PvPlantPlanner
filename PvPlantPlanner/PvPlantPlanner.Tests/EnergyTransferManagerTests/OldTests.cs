using PvPlantPlanner.Common.CoreTypes;
using PvPlantPlanner.EnergyModels.BatteryModules;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.PowerGrids;
using PvPlantPlanner.EnergyModels.PowerPlants;
using PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers;
using static PvPlantPlanner.Tests.Helpers.BatteryModuleHelper;

namespace PvPlantPlanner.Tests.EnergyTransferManagerTests
{
    public class StandardSolarPlantOneDaySampleTests
    {
        private double Tolerance { get; } = 0.0001;

        // Solar Plant Parameters
        private double SolarInstaledPower { get; } = 500; // kWp
        private uint SolarInvestmentCost { get; } = 750; // EUR/kWp
        private HourlyValue<double> SelfConsuptionEnergy { get; } = 30; // kWh

        // Power Grid Parameters
        private double ApprovedFeedInPower { get; } = 150; // kW
        private double AllowedExportPower { get; } = 150; // kW
        private double ExportEnergyPrice { get; } = 0.1; // EUR/kWh
        private double FeedInEnrgyPrice { get; } = 0.1; // EUR/kWh
        private HourlyValue<double> HourlyFeedInEnrgyPrice = new double[] {
                0.12, 0.10, 0.10, 0.11, 0.10, 0.10, 0.09, 0.08,
                0.07, 0.05, 0.01, 0.00, 0.00, 0.00, 0.01, 0.02,
                0.08, 0.11, 0.14, 0.15, 0.17, 0.16, 0.15, 0.13
            };
        private HourlyValue<double> HourlyFeedInEnrgyPriceWithNegative = new double[] {
                0.12, 0.10, 0.10, 0.11, 0.10, 0.10, 0.045, 0.08,
                0.07, 0.03, -0.01, -0.02, 0.00, 0.00, 0.01, 0.02,
                0.1, 0.11, 0.14, 0.15, 0.09, 0.16, 0.15, 0.13
            };

        // Battery Storage Parameters
        private int InvestemntCostP50C100 { get; } = 40000;
        private int MaxCycleCountP50C100 { get; } = 6000;

        // Calculation Parameters
        private List<double> FeedInPriorityPrice { get; } = new List<double> { 0.05 }; // EUR/kWh
        private List<double> SellEnergyAlways { get; } = new List<double> { double.MinValue }; // EUR/kWh
        private List<double> SellWhenNoNegativePrice { get; } = new List<double> { 0.0 }; // EUR/kWh
        private List<double> MinBatteryDischargePrice { get; } = new List<double> { 0.095 }; // EUR/kWh

        [Test]
        public void HighProduction_FiveSmallModuleStorageWithInitialState_DynamicMarketPrice()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 5.0, 40.0, 120.0, 220.0,
                400.0, 480.0, 500.0, 500.0, 160.0, 500.0, 480.0, 400.0,
                300.0, 180.0, 80.0, 20.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, HourlyFeedInEnrgyPrice, ExportEnergyPrice);

            // Create Battery Storage
            double ratedPower = 50;
            double ratedCapacity = 100;

            var modules = new List<IBatteryModule>();
            for (int i = 0; i < 5; i++)
            {
                var module = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
                ChargeModuleWithEnergy(module, 60);
                modules.Add(module);
            }
            var batteryStorage = new BatteryStorage(modules);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, FeedInPriorityPrice, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(174.4).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(14.5).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(145.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(2450.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(670.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(10));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(1660.0).Within(Tolerance));
        }

        [Test]
        public void DynamicHighProduction_FiveSmallModuleStorage_DynamicMarketPriceWithNegative()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 5.0, 40.0, 120.0, 220.0,
                200.0, 480.0, 500.0, 500.0, 160.0, 500.0, 480.0, 150.0,
                140.0, 190.0, 80.0, 20.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, HourlyFeedInEnrgyPriceWithNegative, ExportEnergyPrice);

            // Create Battery Storage
            double ratedPower = 50;
            double ratedCapacity = 100;

            var modules = new List<IBatteryModule>();
            for (int i = 0; i < 5; i++)
            {
                var module = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
                ChargeModuleWithEnergy(module, 20);
                modules.Add(module);
            }
            var batteryStorage = new BatteryStorage(modules);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, FeedInPriorityPrice, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(139.3).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(20.5).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(205.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(1830.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(510.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(8));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(1510.0).Within(Tolerance));
        }

        [Test]
        public void SellingAlways_DynamicHighProduction_WithoutStorage_DynamicMarketPriceWithNegative()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 5.0, 40.0, 120.0, 220.0,
                200.0, 480.0, 500.0, 500.0, 160.0, 500.0, 480.0, 150.0,
                140.0, 190.0, 80.0, 20.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, HourlyFeedInEnrgyPriceWithNegative, ExportEnergyPrice);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, SellEnergyAlways, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(65.95).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(27.5).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(275.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(1710.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(0.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(8));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(1630.0).Within(Tolerance));
        }

        [Test]
        public void SellingWhenNoNegative_DynamicHighProduction_WithoutStorage_DynamicMarketPriceWithNegative()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 5.0, 40.0, 120.0, 220.0,
                200.0, 480.0, 500.0, 500.0, 160.0, 500.0, 480.0, 150.0,
                140.0, 190.0, 80.0, 20.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, HourlyFeedInEnrgyPriceWithNegative, ExportEnergyPrice);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, SellWhenNoNegativePrice, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(70.45).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(27.5).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(275.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(1410.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(0.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(8));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(1930.0).Within(Tolerance));
        }
    }
}
