using PvPlantPlanner.Common.CoreTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.EnergyModels.PowerPlants
{
    public class SolarPowerPlant : IPowerPlant
    {
        public double InstalledPower { get; }
        public uint InvestmentCost { get; }
        public double[] HourlyEnergyOutput { get; }

        private double[] HourlyEnergyProduction { get; }
        private HourlyValue<double> SelfConsumptionEnergy { get; }

        public SolarPowerPlant(
            double installedPower,
            uint investmentCost,
            double[] hourlyEnergyProduction,
            HourlyValue<double> selfConsumptionEnergy)
        {
            if (hourlyEnergyProduction == null)
                throw new ArgumentNullException(nameof(hourlyEnergyProduction), "Hourly energy production array of SolarPowerPlant cannot be null.");
            if (selfConsumptionEnergy == null)
                throw new ArgumentNullException(nameof(selfConsumptionEnergy), "Self-consumption energy value of SolarPowerPlant cannot be null.");
            if (selfConsumptionEnergy.Length != 1 && selfConsumptionEnergy.Length != hourlyEnergyProduction.Length)
                throw new ArgumentException($"Invalid length of self-consumption energy values: {selfConsumptionEnergy.Length}.");

            InstalledPower = installedPower;
            InvestmentCost = investmentCost;
            HourlyEnergyProduction = hourlyEnergyProduction;
            SelfConsumptionEnergy = selfConsumptionEnergy;
            HourlyEnergyOutput = CalculateHourlyEnergyOutput();
        }

        private double[] CalculateHourlyEnergyOutput()
        {
            var result = new double[HourlyEnergyProduction.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = HourlyEnergyProduction[i] - SelfConsumptionEnergy[i];
            }
            return result;
        }
    }


}
