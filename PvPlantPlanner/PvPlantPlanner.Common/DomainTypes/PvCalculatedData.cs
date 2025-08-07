
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
        public PvCalculatedData Clone()
        {
            return new PvCalculatedData
            {
                EnergySalesRevenue = this.EnergySalesRevenue,
                EnergyPurchaseCost = this.EnergyPurchaseCost,
                AnnualEnergyFromGrid = this.AnnualEnergyFromGrid,
                AnnualEnergyToGrid = this.AnnualEnergyToGrid,
                AnnualEnergyFromBattery = this.AnnualEnergyFromBattery,
                AnnualFullPowerHours = this.AnnualFullPowerHours
            };
        }
    }
}
