using PvPlantPlanner.Common.CoreTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.EnergyModels.PowerPlants
{
    public interface IPowerPlant
    {
        double InstalledPower { get; }
        double[] HourlyEnergyOutput { get; }
        uint InvestmentCost { get; }
    }
}
