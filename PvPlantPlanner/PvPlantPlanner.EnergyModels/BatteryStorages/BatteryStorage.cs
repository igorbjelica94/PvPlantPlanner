using PvPlantPlanner.Common.Results;
using PvPlantPlanner.EnergyModels.BatteryModules;
using static PvPlantPlanner.Common.Helpers.MathHelper;

namespace PvPlantPlanner.EnergyModels.BatteryStorages
{
    public class BatteryStorage : IBatteryStorage
    {
        public double RatedPower { get; }
        public double RatedCapacity { get; }
        public int InvestmentCost { get; }
        public List<IBatteryModule> BatteryModules { get; }
        public double CurrentCapacity => BatteryModules.Sum(m => m.CurrentCapacity);
        public double RemainingCapacity => BatteryModules.Sum(m => m.RemainingCapacity);
        public double AvailableChargePower =>
            BatteryModules
                .Where(m => GreaterThanZero(m.RemainingCapacity))
                .Sum(m => m.RatedPower);

        public double AvailableDishargePower =>
            BatteryModules
                .Where(m => GreaterThanZero(m.CurrentCapacity))
                .Sum(m => m.RatedPower);

        private IEnumerable<IBatteryModule> ChargeableModules =>
            BatteryModules.Where(m => GreaterThanZero(m.RemainingCapacity)).OrderBy(m => m.TimeToFullCharge);
        private IEnumerable<IBatteryModule> DischargeableModules =>
            BatteryModules.Where(m => GreaterThanZero(m.CurrentCapacity)).OrderBy(m => m.TimeToFullDischarge);

        public BatteryStorage(List<IBatteryModule> batteryModules)
        {
            BatteryModules = batteryModules ?? throw new ArgumentNullException(nameof(batteryModules), "Battery modules collection cannot be null.");
            RatedPower = batteryModules.Sum(m => m.RatedPower);
            RatedCapacity = batteryModules.Sum(m => m.RatedCapacity);
            InvestmentCost = batteryModules.Sum(m => m.InvestmentCost);
        }

        public ChargeResult TryCharge(double energy)
        {
            if (ApproximatelyEqual(RemainingCapacity, 0))
                return ChargeResult.Failure();

            double energyToCharge = Math.Min(energy, AvailableChargePower /* x 1h */);

            ChargeResult chargeResult = HandleDistributionToModules(energyToCharge);

            double totalCharged = chargeResult.ChargedEnergy;
            if (totalCharged > energyToCharge && !ApproximatelyEqual(totalCharged, energyToCharge))
                throw new InvalidOperationException("Charging exceeded the allowed system capacity.");

            return ApproximatelyEqual(totalCharged, energy)
                ? ChargeResult.Success(energy)
                : ChargeResult.PartialSuccess(totalCharged);
        }

        public DischargeResult TryDischarge(double energy)
        {
            if (ApproximatelyEqual(CurrentCapacity, 0))
                return DischargeResult.Failure();

            double energyToDischarge = Math.Min(energy, AvailableDishargePower /* x 1h */);

            DischargeResult dischargeResult = HandleDistributionFromModules(energyToDischarge);

            double totalDischarged = dischargeResult.DischargedEnergy;
            if (totalDischarged > energyToDischarge && !ApproximatelyEqual(totalDischarged, energyToDischarge))
                throw new InvalidOperationException("Discharging exceeded the allowed system capacity.");

            return ApproximatelyEqual(totalDischarged, energy)
                ? DischargeResult.Success(energy)
                : DischargeResult.PartialSuccess(totalDischarged);
        }

        private ChargeResult HandleDistributionToModules(double energy)
        {
            var chargeableModules = ChargeableModules.ToList();
            if (chargeableModules.Count == 0) 
                return ChargeResult.Failure();

            double remainedToCharge = Math.Min(energy, AvailableChargePower /* x 1h */);
            double chargedEnergy = 0;
            for (int i = 0; i < chargeableModules.Count; i++)
            {
                var module = chargeableModules[i];
                double currentChargePower = chargeableModules.Sum(m => m.RatedPower);
                remainedToCharge = Math.Min(remainedToCharge, currentChargePower /* x 1h */);

                double share = module.RatedPower / currentChargePower;
                double energyForModule = share * remainedToCharge;
                // if the energy cannot be proportionally distributed across the modules, remove the module with the insufficient capacity
                if (energyForModule > module.RemainingCapacity && !ApproximatelyEqual(energyForModule, module.RemainingCapacity))
                {
                    chargeableModules.RemoveAt(i);
                    i--;
                    energyForModule = module.RemainingCapacity;
                    remainedToCharge -= energyForModule;
                }
                var chargeResult = module.TryCharge(energyForModule);
                chargedEnergy += chargeResult.ChargedEnergy;
            }

            return ApproximatelyEqual(chargedEnergy, energy)
                ? ChargeResult.Success(energy)
                : ChargeResult.PartialSuccess(chargedEnergy);
        }

        private DischargeResult HandleDistributionFromModules(double energy)
        {
            var dischargeableModules = DischargeableModules.ToList();
            if (dischargeableModules.Count == 0) 
                return DischargeResult.Failure();

            double remainedToDischarge = Math.Min(energy, AvailableDishargePower /* x 1h */);
            double dischargedEnergy = 0;
            for (int i = 0; i < dischargeableModules.Count; i++)
            {
                var module = dischargeableModules[i];
                double currentDischargePower = dischargeableModules.Sum(m => m.RatedPower);
                remainedToDischarge = Math.Min(remainedToDischarge, currentDischargePower /* x 1h */);

                double share = module.RatedPower / currentDischargePower;
                double energyFromModule = share * remainedToDischarge;
                // if the energy cannot be proportionally distributed from the modules, remove the module with the insufficient capacity
                if (energyFromModule > module.CurrentCapacity && !ApproximatelyEqual(energyFromModule, module.CurrentCapacity))
                {
                    dischargeableModules.RemoveAt(i);
                    i--;
                    energyFromModule = module.CurrentCapacity;
                    remainedToDischarge -= energyFromModule;
                }
                var dischargeResult = module.TryDischarge(energyFromModule);
                dischargedEnergy += dischargeResult.DischargedEnergy;
            }

            return ApproximatelyEqual(dischargedEnergy, energy)
                ? DischargeResult.Success(energy)
                : DischargeResult.PartialSuccess(dischargedEnergy);
        }
    }
}
