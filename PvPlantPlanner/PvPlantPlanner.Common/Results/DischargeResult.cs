using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.Common.Results
{
    public class DischargeResult
    {
        public bool IsSuccessful { get; }
        public double DischargedEnergy { get; }

        public DischargeResult(bool isSuccessful, double dischargedEnergy)
        {
            if (double.IsNaN(dischargedEnergy) || double.IsInfinity(dischargedEnergy) || dischargedEnergy < 0)
                throw new ArgumentOutOfRangeException(nameof(dischargedEnergy), "Discharged energy must be a non-negative, finite number.");

            IsSuccessful = isSuccessful;
            DischargedEnergy = dischargedEnergy;
        }

        public static DischargeResult Success(double dischargedEnergy) => new DischargeResult(true, dischargedEnergy);
        public static DischargeResult PartialSuccess(double dischargedEnergy) => new DischargeResult(false, dischargedEnergy);
        public static DischargeResult Failure() => new DischargeResult(false, 0.0);
    }

}
