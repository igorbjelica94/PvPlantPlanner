using PvPlantPlanner.Common.CoreTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.EnergyModels.PowerGrids
{
    public class PowerGrid : IPowerGrid
    {
        public double ApprovedFeedInPower { get; }
        public double AllowedExportPower { get; }
        public HourlyValue<double> HourlyFeedInEnergyPrice { get; }
        public double ExportEnergyPrice { get; }

        public PowerGrid(
            double approvedFeedInPower,
            double allowedExportPower,
            HourlyValue<double> hourlyFeedInEnergyPrice,
            double exportEnergyPrice)
        {
            ApprovedFeedInPower = approvedFeedInPower;
            AllowedExportPower = allowedExportPower;
            HourlyFeedInEnergyPrice = hourlyFeedInEnergyPrice ??
                throw new ArgumentNullException(nameof(hourlyFeedInEnergyPrice), "Hourly feed-in energy price must be provided and cannot be null.");
            ExportEnergyPrice = exportEnergyPrice;
        }

    }


}
