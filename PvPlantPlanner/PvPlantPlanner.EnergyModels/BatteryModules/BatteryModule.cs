using PvPlantPlanner.Common.Results;
using static PvPlantPlanner.Common.Helpers.MathHelper;


namespace PvPlantPlanner.EnergyModels.BatteryModules
{
    public class BatteryModule : IBatteryModule
    {
        private readonly double _ratedPower;
        private readonly double _ratedCapacity;
        private readonly int _investmentCost;
        private readonly int _maxCycleCount;
        private readonly CycleCountHandler _cycle;

        private double _currentCapacity;

        public BatteryModule(double ratedPower, double ratedCapacity, int investmentCost, int maxCycleCount)
        {
            _ratedPower = ratedPower;
            _ratedCapacity = ratedCapacity;
            _investmentCost = investmentCost;
            _maxCycleCount = maxCycleCount;
            _cycle = new CycleCountHandler(ratedCapacity);
        }

        public double RatedPower => _ratedPower;
        public double RatedCapacity => _ratedCapacity;
        public int InvestmentCost => _investmentCost;
        public int MaxCycleCount => _maxCycleCount;
        public double CurrentCapacity => _currentCapacity;
        public double RemainingCapacity => _ratedCapacity - _currentCapacity;
        public double CurrentCycleCount => _cycle.CurrentCycleCount;

        public ChargeResult TryCharge(double energy)
        {
            if (double.IsNaN(energy) || double.IsInfinity(energy) || energy < 0)
                throw new ArgumentOutOfRangeException(nameof(energy), "Charged energy must be a non-negative, finite number.");

            double chargedEnergy = Math.Min(energy, RemainingCapacity);

            _currentCapacity += chargedEnergy;
            _cycle.UpdateCycleProgress(chargedEnergy);

            if (ApproximatelyEqual(chargedEnergy, energy))
                return ChargeResult.Success(chargedEnergy);
            else
                return ChargeResult.PartialSuccess(chargedEnergy);
        }

        public DischargeResult TryDischarge(double energy)
        {
            if (double.IsNaN(energy) || double.IsInfinity(energy) || energy < 0)
                throw new ArgumentOutOfRangeException(nameof(energy), "Discharged energy must be a non-negative, finite number.");

            double dischargedEnergy = Math.Min(energy, _currentCapacity);

            _currentCapacity -= dischargedEnergy;
            _cycle.UpdateCycleProgress(dischargedEnergy);

            if (ApproximatelyEqual(dischargedEnergy, energy))
                return DischargeResult.Success(dischargedEnergy);
            else
                return DischargeResult.PartialSuccess(dischargedEnergy);
        }

        public ChargeResult CouldChargeWithEnergy(double energy)
        {
            if (double.IsNaN(energy) || double.IsInfinity(energy) || energy < 0)
                throw new ArgumentOutOfRangeException(nameof(energy), "Energy must be a non-negative, finite number.");

            if (energy <= RemainingCapacity)
            {
                return ChargeResult.Success(energy);
            }
            else
            {
                return ChargeResult.PartialSuccess(RemainingCapacity);
            }
        }
    }
}
