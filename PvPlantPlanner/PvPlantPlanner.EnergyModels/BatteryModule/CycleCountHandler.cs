using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.EnergyModels.BatteryModule
{
    internal class CycleCountHandler
    {
        private readonly double _ratedCapacity;

        private int _currentCycleCount;
        private double _cycleProgress;

        public CycleCountHandler(double ratedCapacity)
        {
            if (ratedCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(ratedCapacity), "Rated capacity must be positive.");

            _ratedCapacity = ratedCapacity;
        }

        public int CurrentCycleCount => _currentCycleCount;
        public double CycleProgress => _cycleProgress;
        public double RatedCapacity => _ratedCapacity;

        public void UpdateCycleProgress(double energyDelta)
        {
            if (double.IsNaN(energyDelta) || double.IsInfinity(energyDelta))
                throw new ArgumentOutOfRangeException(nameof(energyDelta), "Energy delta must be a finite number.");

            energyDelta = Math.Abs(energyDelta); // We are interested in the magnitude of energy delta (whether it's positive or negative)

            _cycleProgress += energyDelta / (2 * _ratedCapacity); // One cycle = one charge + one discharge, so we multiply rated capacity by 2

            while (_cycleProgress >= 1.0)
            {
                _currentCycleCount++;
                _cycleProgress -= 1.0;
            }
        }
    }

}
