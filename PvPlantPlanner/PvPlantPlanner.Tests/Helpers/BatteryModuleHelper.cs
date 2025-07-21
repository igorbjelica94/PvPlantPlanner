using PvPlantPlanner.EnergyModels.BatteryModules;

namespace PvPlantPlanner.Tests.Helpers
{
    internal static class BatteryModuleHelper
    {
        public static void ChargeModuleWithEnergy(IBatteryModule batteryModule, double energy)
        {
            int numberOfFullCharges = (int)(energy / batteryModule.RatedPower);
            for (int i = 0; i < numberOfFullCharges; i++)
            {
                batteryModule.TryCharge(batteryModule.RatedPower /* x 1h */);
                energy -= batteryModule.RatedPower /* x 1h */;
            }
            batteryModule.TryCharge(energy);
        }
    }

}
