using PvPlantPlanner.Common.Results;

namespace PvPlantPlanner.EnergyModels.BatteryModules
{
    public interface IBatteryModule
    {
        double RatedPower { get; }
        double RatedCapacity { get; }
        int InvestmentCost { get; }
        int MaxCycleCount { get; }
        double CurrentCapacity { get; }
        double RemainingCapacity { get; }
        double CurrentCycleCount { get; }

        ChargeResult TryCharge(double energy);
        DischargeResult TryDischarge(double energy);
        ChargeResult CouldChargeWithEnergy(double energy);
    }

}
