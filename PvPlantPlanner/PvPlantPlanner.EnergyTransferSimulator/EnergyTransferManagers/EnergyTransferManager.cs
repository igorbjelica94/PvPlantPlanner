using PvPlantPlanner.Common.DomainTypes;
using PvPlantPlanner.Common.Enums;
using PvPlantPlanner.Common.Helpers;
using PvPlantPlanner.Common.Results;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.PowerGrids;
using PvPlantPlanner.EnergyModels.PowerPlants;
using static PvPlantPlanner.Common.Helpers.MathHelper;

using RejectedEnergy = System.Double;

namespace PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers
{
    public class EnergyTransferManager : IEnergyTransferManager
    {
        private enum FeedInStrategyToGrid { NotAllowed, Allowed };

        public PvCalculatedData CalculatedData { get; } = new PvCalculatedData();
        public IPowerPlant SolarPlant { get; }
        public IPowerGrid PowerGrid { get; }
        public IBatteryStorage EnergyStorage { get; private set; }
        public TradingMode TradingMode { get; }
        public List<double> FeedInPriorityPrice { get; }
        public List<double> MinBatteryDischargePrice { get; }

        private int CurrentMonthIndex { get; set; } // 0-based

        public EnergyTransferManager(
            IPowerPlant solarPlant,
            IPowerGrid powerGrid,
            IBatteryStorage energyStorage,
            TradingMode tradingMode,
            List<double> feedInPriorityPrice,
            List<double> minBatteryDischargePrice)
        {
            SolarPlant = solarPlant ?? throw new ArgumentNullException(nameof(solarPlant));
            PowerGrid = powerGrid ?? throw new ArgumentNullException(nameof(powerGrid));
            EnergyStorage = energyStorage ?? throw new ArgumentNullException(nameof(energyStorage));
            TradingMode = tradingMode;
            FeedInPriorityPrice = feedInPriorityPrice ?? new List<double>();
            MinBatteryDischargePrice = minBatteryDischargePrice ?? new List<double>();
        }
        public void ResetCalculatedData()
        {
            CalculatedData.EnergySalesRevenue = 0;
            CalculatedData.EnergyPurchaseCost = 0;
            CalculatedData.AnnualEnergyFromGrid = 0;
            CalculatedData.AnnualEnergyToGrid = 0;
            CalculatedData.AnnualEnergyFromBattery = 0;
            CalculatedData.AnnualFullPowerHours = 0;
        }

        public bool ReplaceBatteryStorage(IBatteryStorage newBatteryStorage)
        {
            if (newBatteryStorage == null)
                return false;

            EnergyStorage = newBatteryStorage;
            return true;
        }

        public ResultStatus ExecuteEnergyTransferForHour(int hour)
        {
            CurrentMonthIndex = MathHelper.GetMonthIndexForHour(hour); // 0-based

            if (IsGreaterThanOrApproxEqual(PowerGrid.HourlyFeedInEnergyPrice[hour], FeedInPriorityPrice[CurrentMonthIndex]))
            {
                return PrioritizeTransferringEnergyToGrid(hour);
            }
            else if (IsLessThan(PowerGrid.HourlyFeedInEnergyPrice[hour], FeedInPriorityPrice[CurrentMonthIndex]) && IsGreaterThanOrEqualToZero(PowerGrid.HourlyFeedInEnergyPrice[hour]))
            {
                return PrioritizeStoringEnergyInBattery(hour, FeedInStrategyToGrid.Allowed);
            }
            else
            {
                return PrioritizeStoringEnergyInBattery(hour, FeedInStrategyToGrid.NotAllowed);
            }

        }

        #region Prioritize transferring energy to grid

        private ResultStatus PrioritizeTransferringEnergyToGrid(int hour)
        {
            if (IsGreaterThanOrApproxEqual(SolarPlant.HourlyEnergyOutput[hour], PowerGrid.ApprovedFeedInPower /* x 1h */))
            {
                TransferEnergyToGrid(PowerGrid.ApprovedFeedInPower /* x 1h */, hour);
                CalculatedData.AnnualFullPowerHours += 1;

                double energyToBattery = SolarPlant.HourlyEnergyOutput[hour] - PowerGrid.ApprovedFeedInPower /* x 1h */;
                RejectedEnergy rejEnergy = TransferPossibleEnergyFromSolarPlantToBatteryStorage(energyToBattery, hour);
                CalculatedData.AnnualRejectedEnergy += rejEnergy;
            }
            else if (IsLessThan(SolarPlant.HourlyEnergyOutput[hour], PowerGrid.ApprovedFeedInPower /* x 1h */) && IsGreaterThanOrEqualToZero(SolarPlant.HourlyEnergyOutput[hour]))
            {
                TransferEnergyToGrid(SolarPlant.HourlyEnergyOutput[hour], hour);

                double energyFromBattery = PowerGrid.ApprovedFeedInPower /* x 1h */ - SolarPlant.HourlyEnergyOutput[hour];
                TryTransferPossibleEnergyFromBatteryStorageToGrid(energyFromBattery, hour);
            }
            else
            {
                double selfConsuptionEnergy = Math.Abs(SolarPlant.HourlyEnergyOutput[hour]);
                HandlePlantSelfConsumptionDeficit(selfConsuptionEnergy, hour);

            }

            return ResultStatus.Ok();
        }

        private void TransferEnergyToGrid(double energy, int hour)
        {
            CalculatedData.AnnualEnergyToGrid += energy;
            CalculatedData.EnergySalesRevenue += energy * PowerGrid.HourlyFeedInEnergyPrice[hour];
        }

        private RejectedEnergy TransferPossibleEnergyFromSolarPlantToBatteryStorage(double energy, int hour)
        {
            if (IsLessThan(EnergyStorage.CurrentCapacity, EnergyStorage.RatedCapacity))
            {
                ChargeResult result = EnergyStorage.TryCharge(energy);
                energy -= result.ChargedEnergy;
            }
            return energy;
        }

        private void TryTransferPossibleEnergyFromBatteryStorageToGrid(double energy, int hour)
        {
            if (!IsGreaterThanZero(EnergyStorage.CurrentCapacity))
                return;

            if (IsLessThan(PowerGrid.HourlyFeedInEnergyPrice[hour], MinBatteryDischargePrice[CurrentMonthIndex]))
                return;

            DischargeResult result = EnergyStorage.TryDischarge(energy);
            CalculatedData.AnnualEnergyFromBattery += result.DischargedEnergy;
            TransferEnergyToGrid(result.DischargedEnergy, hour);
        }

        private void HandlePlantSelfConsumptionDeficit(double selfConsuptionEnergy, int hour)
        {
            DischargeResult result = TryTransferFullEnergyFromBatteryStorageToSolarPlant(selfConsuptionEnergy, hour);
            if (result.IsSuccessful)
            {
                TryTransferPossibleEnergyFromBatteryStorageToGrid(PowerGrid.ApprovedFeedInPower /* x 1h */, hour);
            }
            else
            {
                TransferEnergyFromGrid(selfConsuptionEnergy);
            }
        }

        private DischargeResult TryTransferFullEnergyFromBatteryStorageToSolarPlant(double energy, int hour) // but the battery storage must not be completely discharged
        {
            if (IsLessThan(PowerGrid.HourlyFeedInEnergyPrice[hour], MinBatteryDischargePrice[CurrentMonthIndex])) // when storage is on, it supplies grid (and plant), so this should be checked
                return DischargeResult.Failure();

            if (IsLessThanOrApproxEqual(EnergyStorage.CurrentCapacity, energy)) // do not use last kWh to supplie plant, in that case pull it from the grid
                return DischargeResult.Failure();

            DischargeResult result = EnergyStorage.TryDischarge(energy);
            if (result.IsSuccessful)
            {
                return result;
            }
            else
            { // Invalid scenario. The battery cannot simultaneously supply power to the grid and partially to the plant, while the plant is also being partially powered by the grid.
                ChargeResult chResult = EnergyStorage.TryCharge(result.DischargedEnergy);
                if (!IsApproximatelyEqual(chResult.ChargedEnergy, result.DischargedEnergy))
                    throw new Exception("TryTransferFullEnergyFromBatteryStorageToSolarPlant failed. Returning energy to the battery failed.");
                return DischargeResult.Failure();
            }
        }

        private void TransferEnergyFromGrid(double energy) // energy must be positive number
        {
            CalculatedData.AnnualEnergyFromGrid += energy;
            CalculatedData.EnergyPurchaseCost += energy * PowerGrid.ExportEnergyPrice;
        }

        #endregion Prioritize transferring energy to grid

        #region Prioritize storing energy in battery

        private ResultStatus PrioritizeStoringEnergyInBattery(int hour, FeedInStrategyToGrid feedInToGrid)
        {
            if (IsGreaterThanOrEqualToZero(SolarPlant.HourlyEnergyOutput[hour]))
            {
                if (IsGreaterThanOrApproxEqual(SolarPlant.HourlyEnergyOutput[hour], PowerGrid.ApprovedFeedInPower))
                {
                    CalculatedData.AnnualFullPowerHours += 1;
                }
                double notStoredEnergy = TransferPossibleEnergyFromSolarPlantToBatteryStorage(SolarPlant.HourlyEnergyOutput[hour], hour);

                double energyToGrid = 0;
                if (feedInToGrid == FeedInStrategyToGrid.Allowed)
                {
                    energyToGrid = Math.Min(PowerGrid.ApprovedFeedInPower, notStoredEnergy);
                    TransferEnergyToGrid(energyToGrid, hour);
                }
                CalculatedData.AnnualRejectedEnergy += (notStoredEnergy - energyToGrid);
            }
            else
            {
                double selfConsuptionEnergy = Math.Abs(SolarPlant.HourlyEnergyOutput[hour]);
                TransferEnergyFromGrid(selfConsuptionEnergy);
            }

            return ResultStatus.Ok();
        }

        #endregion Prioritize storing energy in battery
    }
}
