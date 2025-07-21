using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.EnergyModels.BatteryModules
{
    internal sealed class CycleCountHandler
    {
        public double RatedCapacity { get; }
        public int CurrentCycleCount { get; private set; }
        public double CycleProgress { get; private set; }

        public CycleCountHandler(double ratedCapacity)
        {
            if (ratedCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(ratedCapacity), "Rated capacity must be positive.");

            RatedCapacity = ratedCapacity;
        }

        public void UpdateCycleProgress(double energyDelta)
        {
            if (double.IsNaN(energyDelta) || double.IsInfinity(energyDelta))
                throw new ArgumentOutOfRangeException(nameof(energyDelta), "Energy delta must be a finite number.");

            energyDelta = Math.Abs(energyDelta); // We are interested in the magnitude of energy delta (whether it's positive or negative)

            CycleProgress += energyDelta / (2 * RatedCapacity); // One cycle = one charge + one discharge, so we multiply rated capacity by 2

            while (CycleProgress >= 1.0)
            {
                CurrentCycleCount++;
                CycleProgress -= 1.0;
            }
        }
    }

}
