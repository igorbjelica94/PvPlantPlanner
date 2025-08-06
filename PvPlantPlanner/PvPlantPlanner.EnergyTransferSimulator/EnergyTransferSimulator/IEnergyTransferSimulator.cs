using PvPlantPlanner.Common.Config;
using PvPlantPlanner.Common.Results;

namespace PvPlantPlanner.EnergyTransferSimulator.EnergyTransferSimulator
{
    public interface IEnergyTransferSimulator
    {
        void ConfigureSimulator(CalculationConfig config);
        void StartSimulation();
    }
}
