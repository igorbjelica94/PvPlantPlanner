using NUnit.Framework;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.BatteryModules;
using static PvPlantPlanner.Tests.Helpers.BatteryModuleHelper;

namespace PvPlantPlanner.Tests
{
    public class BatteryStorageChargingTests
    {
        private double Tolerance { get;  } = 0.0001;
        private int DefaultCost { get;  } = 1000;
        private int DefaultMaxCycleCount { get;  } = 5000;

        [Test]
        public void ChargeOverflow_IdenticalModules_EmptyState()
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
            var result = batteryStorage.TryCharge(60);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(20).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(10).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(10).Within(Tolerance));
        }

        [Test]
        public void ChargeOverflow_IdenticalModules_PartiallyCharged()
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
            ChargeModuleWithEnergy(modules[0], 30);
            ChargeModuleWithEnergy(modules[1], 30);

            // Act
            var result = batteryStorage.TryCharge(130);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(20).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(40).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(40).Within(Tolerance));
        }

        [Test]
        public void ChargeOverflow_DifferentModules_EmptyState()
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

            // Act
            var result = batteryStorage.TryCharge(60);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(30).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(10).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(20).Within(Tolerance));
        }

        [Test]
        public void ChargeOverflow_IdenticalModules_FullyCharged()
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
            var result = batteryStorage.TryCharge(100);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(0));
        }

        [Test]
        public void ChargeOverflow_IdenticalModules_SomeFullyChargedSomeEmptyState()
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
            var result = batteryStorage.TryCharge(130);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(10).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(100).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(10).Within(Tolerance));
        }

        [Test]
        public void NominalCharge_SamePowerDifferentCapacity_SomeAlmostFullyChargedSomePartiallyCharged()
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
            var result = batteryStorage.TryCharge(40);

            // Assert
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.ChargedEnergy, Is.EqualTo(40).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
        }

        [Test]
        public void ChargeOverflow_SamePowerDifferentCapacity_SomeAlmostFullyChargedSomePartiallyCharged()
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
            ChargeModuleWithEnergy(modules[0], 90);
            ChargeModuleWithEnergy(modules[1], 90);
            ChargeModuleWithEnergy(modules[2], 90);
            ChargeModuleWithEnergy(modules[3], 90);

            // Act
            var result = batteryStorage.TryCharge(120);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(60).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(110).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(110).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(100).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(100).Within(Tolerance));
        }

        [Test]
        public void ChargeOverflow_DifferentModules_MixedChargeLevels()
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
            ChargeModuleWithEnergy(modules[0], 80);
            ChargeModuleWithEnergy(modules[1], 160);
            ChargeModuleWithEnergy(modules[2], 80);
            ChargeModuleWithEnergy(modules[3], 100);

            // Act
            var result = batteryStorage.TryCharge(100);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(40).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(180).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(90).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(100).Within(Tolerance));
        }

        [Test]
        public void ChargeOverflow_DifferentModules_MostlyAlmostFullyCharged()
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
            ChargeModuleWithEnergy(modules[0], 95);
            ChargeModuleWithEnergy(modules[1], 190);
            ChargeModuleWithEnergy(modules[2], 95);
            ChargeModuleWithEnergy(modules[3], 100);

            // Act
            var result = batteryStorage.TryCharge(70);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ChargedEnergy, Is.EqualTo(25).Within(Tolerance));
            Assert.That(modules[0].CurrentCapacity, Is.EqualTo(105).Within(Tolerance));
            Assert.That(modules[1].CurrentCapacity, Is.EqualTo(200).Within(Tolerance));
            Assert.That(modules[2].CurrentCapacity, Is.EqualTo(100).Within(Tolerance));
            Assert.That(modules[3].CurrentCapacity, Is.EqualTo(100).Within(Tolerance));
        }
    }
}


