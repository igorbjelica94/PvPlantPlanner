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
        private readonly double _approvedFeedInPower;
        private readonly double _allowedExportPower;
        private readonly HourlyValue<double> _hourlyFeedInEnergyPrice;
        private readonly double _exportEnergyPrice;

        public PowerGrid(
            double approvedFeedInPower,
            double allowedExportPower,
            HourlyValue<double> hourlyFeedInEnergyPrice,
            double exportEnergyPrice)
        {
            _approvedFeedInPower = approvedFeedInPower;
            _allowedExportPower = allowedExportPower;
            _hourlyFeedInEnergyPrice = hourlyFeedInEnergyPrice ??
                throw new ArgumentNullException(nameof(hourlyFeedInEnergyPrice), "Hourly feed-in energy price must be provided and cannot be null.");
            _exportEnergyPrice = exportEnergyPrice;
        }

        public double ApprovedFeedInPower => _approvedFeedInPower;
        public double AllowedExportPower => _allowedExportPower;
        public HourlyValue<double> HourlyFeedInEnergyPrice => _hourlyFeedInEnergyPrice;
        public double ExportEnergyPrice => _exportEnergyPrice;
    }


}
