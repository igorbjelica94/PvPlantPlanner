using NUnit.Framework;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.BatteryModules;
using static PvPlantPlanner.Tests.Helpers.BatteryModuleHelper;

namespace PvPlantPlanner.Tests
{
    public class BatteryStorageDischargingTests
    {
        private double Tolerance { get;  } = 0.0001;
        private int DefaultCost { get;  } = 1000;
        private int DefaultMaxCycleCount { get;  } = 5000;

        [Test]
        public void DischargeOverload_IdenticalModules_FullyCharged()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount)
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 100);
            ChargeModuleWithEnergy(modules[1], 100);

            // Act
            var result = batteryStorage.TryDischarge(60);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(20).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
        }

        [Test]
        public void DischargeOverload_IdenticalModules_PartiallyCharged()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount)
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 60);
            ChargeModuleWithEnergy(modules[1], 60);

            // Act
            var result = batteryStorage.TryDischarge(130);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(20).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(50).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(50).Within(Tolerance));
        }

        [Test]
        public void DischargeOverload_DifferentModules_FullyCharged()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount)
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 100);
            ChargeModuleWithEnergy(modules[1], 100);

            // Act
            var result = batteryStorage.TryDischarge(60);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(30).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(80).Within(Tolerance));
        }

        [Test]
        public void DischargeOverload_IdenticalModules_EmptyState()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount)
            };
            var batteryStorage = new BatteryStorage(modules);

            // Act
            var result = batteryStorage.TryDischarge(100);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(0));
        }

        [Test]
        public void DischargeOverload_IdenticalModules_SomeFullyChargedSomeEmptyState()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount)
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 100);

            // Act
            var result = batteryStorage.TryDischarge(70);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(10).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(0).Within(Tolerance));
        }

        [Test]
        public void DischargeOverload_SamePowerDifferentCapacity_SomeAlmostFullySomePartiallyCharged()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 80);
            ChargeModuleWithEnergy(modules[1], 80);
            ChargeModuleWithEnergy(modules[2], 80);
            ChargeModuleWithEnergy(modules[3], 80);

            // Act
            var result = batteryStorage.TryDischarge(120);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(40).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(70).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(70).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(70).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(70).Within(Tolerance));
        }

        [Test]
        public void DischargeOverload_DifferentModules_SomeAlmostEmptySomePartiallyCharged()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 50);
            ChargeModuleWithEnergy(modules[1], 50);
            ChargeModuleWithEnergy(modules[2], 10);
            ChargeModuleWithEnergy(modules[3], 10);

            // Act
            var result = batteryStorage.TryDischarge(80);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(60).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(30).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(30).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(0).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(0).Within(Tolerance));
        }

        [Test]
        public void DischargeOverload_DifferentModules_MixedChargeLevels()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 120);
            ChargeModuleWithEnergy(modules[1], 40);
            ChargeModuleWithEnergy(modules[2], 20);
            ChargeModuleWithEnergy(modules[3], 0);

            // Act
            var result = batteryStorage.TryDischarge(100);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(40).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(110).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(20).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(10).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(0).Within(Tolerance));
        }

        [Test]
        public void DischargeOverload_DifferentModules_MostlyAlmostEmptyState()
        {
            // Arrange
            var modules = new List<IBatteryModule>
            {
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 200,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 10,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
                new BatteryModule(
                    ratedPower: 20,
                    ratedCapacity: 100,
                    investmentCost: DefaultCost,
                    maxCycleCount: DefaultMaxCycleCount),
            };
            var batteryStorage = new BatteryStorage(modules);
            ChargeModuleWithEnergy(modules[0], 105);
            ChargeModuleWithEnergy(modules[1], 10);
            ChargeModuleWithEnergy(modules[2], 5);
            ChargeModuleWithEnergy(modules[3], 0);

            // Act
            var result = batteryStorage.TryDischarge(60);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.DischargedEnergy, Is.EqualTo(25).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(95).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(0).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(0).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(0).Within(Tolerance));
        }
    }
}


