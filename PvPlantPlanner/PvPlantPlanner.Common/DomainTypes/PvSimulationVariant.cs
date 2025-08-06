
using PvPlantPlanner.Common.Config;

namespace PvPlantPlanner.Common.DomainTypes
{
    public class PvSimulationVariant
    {
        public List<BatteryDto> SelectedBatteryModuls { get; set; } = new List<BatteryDto>();
        public double RatedStoragePower { get; set; }
        public double RatedStorageCapacity { get; set; }
        public List<TransformerDto> SelectedTransformers { get; set; } = new List<TransformerDto>();
    }
}
