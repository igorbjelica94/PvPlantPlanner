using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.Common.DomainTypes
{
    public class PvCalculatedData
    {
        public double EnergySalesRevenue { get; set; }
        public double EnergyPurchaseCost { get; set; }
        public double AnnualEnergyFromGrid { get; set; }
        public double AnnualEnergyToGrid { get; set; }
        public double AnnualEnergyFromBattery { get; set; }
        public int AnnualFullPowerHours { get; set; }
        public double AnnualRejectedEnergy { get; set; }
    }
}
