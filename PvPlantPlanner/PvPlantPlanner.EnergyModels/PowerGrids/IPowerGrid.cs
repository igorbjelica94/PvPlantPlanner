using PvPlantPlanner.Common.CoreTypes;

namespace PvPlantPlanner.EnergyModels.PowerGrids
{
    public interface IPowerGrid
    {
        double ApprovedFeedInPower { get; }
        double AllowedExportPower { get; }
        HourlyValue<double> HourlyFeedInEnergyPrice { get; }
        double ExportEnergyPrice { get; }
    }
}
