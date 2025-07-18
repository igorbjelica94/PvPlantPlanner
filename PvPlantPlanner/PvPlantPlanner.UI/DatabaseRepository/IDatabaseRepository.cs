using PvPlantPlanner.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPlantPlanner.UI.DatabaseRepo
{
    public interface IDatabaseRepository : IDisposable
    {
        #region Battery Operations
        int AddBattery(Battery battery);
        bool UpdateBattery(Battery battery);
        bool DeleteBattery(int id);
        Battery GetBattery(int id);
        IEnumerable<Battery> GetAllBatteries();
        #endregion

        #region Transformer Operations
        int AddTransformer(Transformer transformer);
        bool UpdateTransformer(Transformer transformer);
        bool DeleteTransformer(int id);
        Transformer GetTransformer(int id);
        IEnumerable<Transformer> GetAllTransformers();
        #endregion
    }
}
