using PvPlantPlanner.Common.Config;
using PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers;
using PvPlantPlanner.EnergyTransferSimulator.EnergyTransferSimulator;

namespace PvPlantPlanner.Tests.AnnualEnergyTransferSimulatorTests
{
    public class CreateSimulationVariantsTests
    {
        private double Tolerance { get; } = 0.0001;

        private AnnualEnergyTransferSimulator Simulator { get; } = new AnnualEnergyTransferSimulator();

        [Test]
        public void FirstTest()
        {
            var config = new CalculationConfig
            {
                BaseConfig = new BaseConfig
                {
                    InstalledPower = 1000,
                    MaxApprovedPower = 1200,
                    ConstructionPrice = 50000,
                    MaxGridPower = 1500,
                    SelfConsumptionFactor = 0.8,
                    ElectricityPrice = 0.12,
                    TradingCommission = 0.01,
                    FixedPrice = null,
                    NegativePrice = null,
                    MaxBatteryPower = 50,

                    SelectedBatteries = new List<BatteryDto>
                    {
                        new BatteryDto
                        {
                            Id = 1,
                            Power = 10.0,
                            Capacity = 20.0,
                            Price = 3000,
                            Cycles = 5000
                        }
                    },

                    SelectedTransformers = new List<TransformerDto>
                    {
                        new TransformerDto
                        {
                            PowerKVA = 15,
                            PowerFactor = 0.9,
                            Price = 1200
                        }
                    }
                },

                GenerationData = new List<double> { 1.2, 1.3, 1.5, 1.4 },
                SelfConsumptionData = new List<double> { 0.9, 1.0, 1.1, 1.05 },
                MarketPrice = new List<double> { 0.10, 0.11, 0.09, 0.12 },
                MinEnergySellingPrice = new List<double> { 0.05 },
                MinBatteryEnergySellingPrice = new List<double> { 0.03 }
            };


            Simulator.ConfigureSimulator(config);

            Assert.That(Simulator.InputCalculationData.Count, Is.EqualTo(5));
            Assert.That(Simulator.InputCalculationData[0].RatedStoragePower, Is.EqualTo(10.0).Within(Tolerance));
            Assert.That(Simulator.InputCalculationData[1].RatedStoragePower, Is.EqualTo(20.0).Within(Tolerance));
            Assert.That(Simulator.InputCalculationData[2].RatedStoragePower, Is.EqualTo(30.0).Within(Tolerance));
            Assert.That(Simulator.InputCalculationData[3].RatedStoragePower, Is.EqualTo(40.0).Within(Tolerance));
            Assert.That(Simulator.InputCalculationData[4].RatedStoragePower, Is.EqualTo(50.0).Within(Tolerance));
            Assert.That(Simulator.InputCalculationData[0].SelectedTransformers.Count, Is.EqualTo(1));
            Assert.That(Simulator.InputCalculationData[1].SelectedTransformers.Count, Is.EqualTo(2));
            Assert.That(Simulator.InputCalculationData[2].SelectedTransformers.Count, Is.EqualTo(3));
            Assert.That(Simulator.InputCalculationData[3].SelectedTransformers.Count, Is.EqualTo(3));
            Assert.That(Simulator.InputCalculationData[4].SelectedTransformers.Count, Is.EqualTo(4));
        }
    }
}
