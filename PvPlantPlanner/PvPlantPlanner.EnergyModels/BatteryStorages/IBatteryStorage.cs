using PvPlantPlanner.Common.Results;
using PvPlantPlanner.EnergyModels.BatteryModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.EnergyModels.BatteryStorages
{
    public interface IBatteryStorage
    {
        double RatedPower { get; }
        double RatedCapacity { get; }
        int InvestmentCost { get; }
        List<IBatteryModule> BatteryModules { get; }
        double CurrentCapacity { get; }
        double RemainingCapacity { get; }
        double AvailableChargePower { get; }
        double AvailableDishargePower { get; }

        ChargeResult TryCharge(double energy);
        DischargeResult TryDischarge(double energy);
    }

}
