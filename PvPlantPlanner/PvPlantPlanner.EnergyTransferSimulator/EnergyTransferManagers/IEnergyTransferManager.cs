using PvPlantPlanner.Common.DomainTypes;
using PvPlantPlanner.Common.Enums;
using PvPlantPlanner.Common.Results;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.PowerGrids;
using PvPlantPlanner.EnergyModels.PowerPlants;

namespace PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers
{
    public interface IEnergyTransferManager
    {
        PvCalculatedData CalculatedData { get; }
        IPowerPlant SolarPlant { get; }
        IPowerGrid PowerGrid { get; }
        IBatteryStorage? EnergyStorage { get; set;  }
        List<double> FeedInPriorityPrice { get; }
        List<double> MinBatteryDischargePrice { get; }

        void ExecuteEnergyTransferForHour(int hour);
        void ResetCalculatedData();
    }
}
