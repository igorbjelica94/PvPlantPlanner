using PvPlantPlanner.Common.Results;
using static PvPlantPlanner.Common.Helpers.MathHelper;


namespace PvPlantPlanner.EnergyModels.BatteryModules
{
    public class BatteryModule : IBatteryModule
    {
        public double RatedPower { get; }
        public double RatedCapacity { get; }
        public int InvestmentCost { get; }
        public int MaxCycleCount { get; }
        public double CurrentCapacity { get; private set; }
        public double RemainingCapacity => RatedCapacity - CurrentCapacity;
        public double CurrentCycleCount => Cycle.CurrentCycleCount;
        public double TimeToFullCharge => RemainingCapacity / RatedPower;
        public double TimeToFullDischarge => CurrentCapacity / RatedPower;

        private CycleCountHandler Cycle { get; set; }

        public BatteryModule(double ratedPower, double ratedCapacity, int investmentCost, int maxCycleCount)
        {
            RatedPower = ratedPower;
            RatedCapacity = ratedCapacity;
            InvestmentCost = investmentCost;
            MaxCycleCount = maxCycleCount;

            Cycle = new CycleCountHandler(ratedCapacity);
        }

        public ChargeResult TryCharge(double energy)
        {
            if (double.IsNaN(energy) || double.IsInfinity(energy) || energy < 0)
                throw new ArgumentOutOfRangeException(nameof(energy), "Charged energy must be a non-negative, finite number.");

            if (ApproximatelyEqual(CurrentCapacity, RatedCapacity))
            {
                CurrentCapacity = RatedCapacity;
                return ChargeResult.Failure();
            }

            double chargedEnergy = Math.Min(Math.Min(energy, RatedPower /* x 1h */), RemainingCapacity);

            CurrentCapacity += chargedEnergy;
            Cycle.UpdateCycleProgress(chargedEnergy);

            return ApproximatelyEqual(chargedEnergy, energy)
                ? ChargeResult.Success(chargedEnergy)
                : ChargeResult.PartialSuccess(chargedEnergy);
        }

        public DischargeResult TryDischarge(double energy)
        {
            if (double.IsNaN(energy) || double.IsInfinity(energy) || energy < 0)
                throw new ArgumentOutOfRangeException(nameof(energy), "Discharged energy must be a non-negative, finite number.");

            if (ApproximatelyEqual(CurrentCapacity, 0.0))
            {
                CurrentCapacity = 0.0;
                return DischargeResult.Failure();
            }

            double dischargedEnergy = Math.Min(energy, CurrentCapacity);

            CurrentCapacity -= dischargedEnergy;
            Cycle.UpdateCycleProgress(dischargedEnergy);

            return ApproximatelyEqual(dischargedEnergy, energy)
                ? DischargeResult.Success(dischargedEnergy)
                : DischargeResult.PartialSuccess(dischargedEnergy);
        }

        public ChargeResult CanChargeWithEnergy(double energy)
        {
            if (double.IsNaN(energy) || double.IsInfinity(energy) || energy < 0)
                throw new ArgumentOutOfRangeException(nameof(energy), "Energy must be a non-negative, finite number.");

            if (ApproximatelyEqual(CurrentCapacity, RatedCapacity))
            {
                CurrentCapacity = RatedCapacity;
                return ChargeResult.Failure();
            }

            return energy < RemainingCapacity || ApproximatelyEqual(energy, RemainingCapacity)
                ? ChargeResult.Success(energy)
                : ChargeResult.PartialSuccess(RemainingCapacity);
        }
    }
}
