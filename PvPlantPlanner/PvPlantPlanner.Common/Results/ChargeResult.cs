using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.Common.Results
{
    public class ChargeResult
    {
        public bool IsSuccessful { get; }
        public double ChargedEnergy { get; }

        public ChargeResult(bool isSuccessful, double chargedEnergy)
        {
            if (double.IsNaN(chargedEnergy) || double.IsInfinity(chargedEnergy) || chargedEnergy < 0)
                throw new ArgumentOutOfRangeException(nameof(chargedEnergy), "Energija kojom se baterija napunila mora biti nenegativan, konačan broj.");

            IsSuccessful = isSuccessful;
            ChargedEnergy = chargedEnergy;
        }

        public static ChargeResult Success(double chargedEnergy) => new ChargeResult(true, chargedEnergy);
        public static ChargeResult PartialSuccess(double chargedEnergy) => new ChargeResult(false, chargedEnergy);
        public static ChargeResult Failure() => new ChargeResult(false, 0.0);
    }

}
