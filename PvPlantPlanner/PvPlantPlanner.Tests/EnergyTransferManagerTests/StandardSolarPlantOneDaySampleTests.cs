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
        private int SolarInvestmentCost { get; } = 750; // EUR/kWp
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

        // Battery Storage Parameters
        private int InvestemntCostP50C100 { get; } = 40000;
        private int MaxCycleCountP50C100 { get; } = 6000;

        // Calculation Parameters
        private List<double> FeedInPriorityPrice { get; } = new List<double> { 0.05 }; // EUR/kWh
        private List<double> MinBatteryDischargePrice { get; } = new List<double> { 0.095 }; // EUR/kWh

        [Test]
        public void SmallProduction_NotUsedStorage_StaticMarketPrice()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.0,
                50.0, 60.0, 70.0, 100.0, 70.0, 70.0, 50.0, 50.0,
                20.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, FeedInEnrgyPrice, ExportEnergyPrice);

            // Create Battery Storage
            double ratedPower = 50;
            double ratedCapacity = 100;
            var batteryModule = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
            var modules = new List<IBatteryModule>() { batteryModule };
            var batteryStorage = new BatteryStorage(modules);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, 0, FeedInPriorityPrice, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(28.00).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(45.00).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(450.00).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(280.00).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(0.00).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(0));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(0.00).Within(Tolerance));
        }

        [Test]
        public void StandardProduction_OneSmallModuleStorage_StaticMarketPrice()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 40.0, 90.0,
                150.0, 200.0, 240.0, 280.0, 240.0, 220.0, 200.0, 150.0,
                80.0, 30.0, 10.0, 5.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, FeedInEnrgyPrice, ExportEnergyPrice);

            // Create Battery Storage
            double ratedPower = 50;
            double ratedCapacity = 100;
            var batteryModule = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
            var modules = new List<IBatteryModule>() { batteryModule };
            var batteryStorage = new BatteryStorage(modules);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, 0, FeedInPriorityPrice, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(136.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(33.5).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(335.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(1360.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(100.00).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(6));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(200.00).Within(Tolerance));
        }

        [Test]
        public void StandardProduction_TwoSmallModuleStorage_StaticMarketPrice()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 40.0, 90.0,
                150.0, 200.0, 240.0, 280.0, 240.0, 220.0, 200.0, 150.0,
                80.0, 30.0, 10.0, 5.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, FeedInEnrgyPrice, ExportEnergyPrice);

            // Create Battery Storage
            double ratedPower = 50;
            double ratedCapacity = 100;
            var batteryModule1 = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
            var batteryModule2 = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
            var modules = new List<IBatteryModule>() { batteryModule1, batteryModule2 };
            var batteryStorage = new BatteryStorage(modules);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, 0, FeedInPriorityPrice, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(146.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(33.5).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(335.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(1460.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(200.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(6));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(100.0).Within(Tolerance));
        }

        [Test]
        public void StandardProduction_TwoSmallModuleStorage_DynamicMarketPrice()
        {
            // Arrange
            // Create Solar Plant
            double[] hourlyEnergyProduction = new double[24] {
                0.0, 0.0, 0.0, 0.0, 0.0, 10.0, 40.0, 90.0,
                150.0, 200.0, 240.0, 280.0, 240.0, 220.0, 200.0, 150.0,
                80.0, 30.0, 10.0, 5.0, 0.0, 0.0, 0.0, 0.0
            };
            var solarPowerPlant = new SolarPowerPlant(SolarInstaledPower, SolarInvestmentCost, hourlyEnergyProduction, SelfConsuptionEnergy);

            // CreatePower Grid
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, HourlyFeedInEnrgyPrice, ExportEnergyPrice);

            // Create Battery Storage
            double ratedPower = 50;
            double ratedCapacity = 100;
            var batteryModule1 = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
            var batteryModule2 = new BatteryModule(ratedPower, ratedCapacity, InvestemntCostP50C100, MaxCycleCountP50C100);
            var modules = new List<IBatteryModule>() { batteryModule1, batteryModule2 };
            var batteryStorage = new BatteryStorage(modules);

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, 0, FeedInPriorityPrice, MinBatteryDischargePrice);

            // Act
            for (int hour = 0; hour < 24; hour++)
            {
                energyTransferManager.ExecuteEnergyTransferForHour(hour);
            }

            // Assert
            Assert.That(energyTransferManager.CalculatedData.EnergySalesRevenue, Is.EqualTo(52.8).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.EnergyPurchaseCost, Is.EqualTo(31.5).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromGrid, Is.EqualTo(315.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyToGrid, Is.EqualTo(1400.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualEnergyFromBattery, Is.EqualTo(180.0).Within(Tolerance));
            Assert.That(energyTransferManager.CalculatedData.AnnualFullPowerHours, Is.EqualTo(6));
            Assert.That(energyTransferManager.CalculatedData.AnnualRejectedEnergy, Is.EqualTo(140.0).Within(Tolerance));
        }

        [Test]
        public void HighProduction_FiveSmallModuleStorage_DynamicMarketPrice()
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

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, 0, FeedInPriorityPrice, MinBatteryDischargePrice);

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
            HourlyValue<double> hourlyFeedInEnrgyPrice = new double[] {
                0.12, 0.10, 0.10, 0.11, 0.10, 0.10, 0.045, 0.08,
                0.07, 0.03, -0.01, -0.02, 0.00, 0.00, 0.01, 0.02,
                0.1, 0.11, 0.14, 0.15, 0.09, 0.16, 0.15, 0.13
            };
            var powerGrid = new PowerGrid(ApprovedFeedInPower, AllowedExportPower, hourlyFeedInEnrgyPrice, ExportEnergyPrice);

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

            var energyTransferManager = new EnergyTransferManager(solarPowerPlant, powerGrid, batteryStorage, 0, FeedInPriorityPrice, MinBatteryDischargePrice);

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
    }
}
