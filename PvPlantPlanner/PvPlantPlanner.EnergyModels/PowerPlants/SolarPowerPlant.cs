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
        private readonly double _installedPower;
        private readonly int _investmentCost;
        private readonly double[] _hourlyEnergyProduction;
        private readonly HourlyValue<double> _selfConsumptionEnergy;
        private readonly double[] _hourlyEnergyOutput;

        public SolarPowerPlant(
            double installedPower,
            int investmentCost,
            double[] hourlyEnergyProduction,
            HourlyValue<double> selfConsumptionEnergy)
        {
            if (hourlyEnergyProduction == null)
                throw new ArgumentNullException(nameof(hourlyEnergyProduction), "Hourly energy production array of SolarPowerPlant cannot be null.");
            if (selfConsumptionEnergy == null)
                throw new ArgumentNullException(nameof(selfConsumptionEnergy), "Self-consumption energy value of SolarPowerPlant cannot be null.");
            if (selfConsumptionEnergy.Length != 1 && selfConsumptionEnergy.Length != hourlyEnergyProduction.Length)
                throw new ArgumentException($"Invalid length of self-consumption energy values: {selfConsumptionEnergy.Length}.");

            _installedPower = installedPower;
            _investmentCost = investmentCost;
            _hourlyEnergyProduction = hourlyEnergyProduction;
            _selfConsumptionEnergy = selfConsumptionEnergy;
            _hourlyEnergyOutput = CalculateHourlyEnergyOutput();
        }

        public double InstalledPower => _installedPower;
        public int InvestmentCost => _investmentCost;
        public double[] HourlyEnergyOutput => _hourlyEnergyOutput;

        private double[] CalculateHourlyEnergyOutput()
        {
            var result = new double[_hourlyEnergyProduction.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _hourlyEnergyProduction[i] - _selfConsumptionEnergy[i];
            }
            return result;
        }
    }


}
