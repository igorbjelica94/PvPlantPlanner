using PvPlantPlanner.Common.CoreTypes;
using PvPlantPlanner.Common.DomainTypes;
using PvPlantPlanner.Common.Enums;
using PvPlantPlanner.Common.Helpers;
using PvPlantPlanner.Common.Results;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.PowerGrids;
using PvPlantPlanner.EnergyModels.PowerPlants;
using System.ComponentModel;
using static PvPlantPlanner.Common.Helpers.MathHelper;

using RejectedEnergy = System.Double;

namespace PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers
{
    public class EnergyTransferManager : IEnergyTransferManager
    {
        private int _currentMonthIndex; // 0-based

        public PvCalculatedData CalculatedData { get; } = new PvCalculatedData();
        public IPowerPlant SolarPlant { get; }
        public IPowerGrid PowerGrid { get; }
        public IBatteryStorage? EnergyStorage { get; set; }
        public List<double> FeedInPriorityPrice { get; private set; }
        public List<double> MinBatteryDischargePrice { get; }


        public EnergyTransferManager(
            IPowerPlant solarPlant,
            IPowerGrid powerGrid,
            List<double> feedInPriorityPrice,
            List<double> minBatteryDischargePrice)
        {
            SolarPlant = solarPlant ?? throw new ArgumentNullException(nameof(solarPlant));
            PowerGrid = powerGrid ?? throw new ArgumentNullException(nameof(powerGrid));
            FeedInPriorityPrice = feedInPriorityPrice ?? new List<double>();
            MinBatteryDischargePrice = minBatteryDischargePrice ?? new List<double>();
        }

        public EnergyTransferManager(
            IPowerPlant solarPlant,
            IPowerGrid powerGrid,
            IBatteryStorage batteryStorage,
            List<double> feedInPriorityPrice,
            List<double> minBatteryDischargePrice)
        {
            SolarPlant = solarPlant ?? throw new ArgumentNullException(nameof(solarPlant));
            PowerGrid = powerGrid ?? throw new ArgumentNullException(nameof(powerGrid));
            EnergyStorage = batteryStorage;
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

        public void ReplaceFeedInPriorityPrice(List<double> feedInPriorityPlace)
        {
            if (feedInPriorityPlace.Count != 12) throw new ArgumentException("Broj cena za aktivaciju prodaje energije nije jednak 12.", nameof(feedInPriorityPlace));

            FeedInPriorityPrice = feedInPriorityPlace;
        }

        public void ReplaceFeedInEnergyPrice(HourlyValue<double> newFeedInPrice)
        {
            PowerGrid.ReplaceFeedInEnergyPrice(newFeedInPrice);
        }

        public void ExecuteEnergyTransferForHour(int hour)
        {
            _currentMonthIndex = MathHelper.GetMonthIndexForHour(hour); // 0-based

            if (IsGreaterThanOrApproxEqual(PowerGrid.HourlyFeedInEnergyPrice[hour], FeedInPriorityPrice[_currentMonthIndex]))
            {
                PrioritizeTransferringEnergyToGrid(hour);
            }
            else if (IsLessThan(PowerGrid.HourlyFeedInEnergyPrice[hour], FeedInPriorityPrice[_currentMonthIndex]) && IsGreaterThanOrEqualToZero(PowerGrid.HourlyFeedInEnergyPrice[hour]))
            {
                PrioritizeStoringEnergyInBattery(hour, FeedInStrategyToGrid.Allowed);
            }
            else
            {
                PrioritizeStoringEnergyInBattery(hour, FeedInStrategyToGrid.NotAllowed);
            }

        }

        #region Prioritize transferring energy to grid

        private void PrioritizeTransferringEnergyToGrid(int hour)
        {
            double approvedFeedInEnergy = PowerGrid.ApprovedFeedInPower /* x 1h */;

            if (IsGreaterThanOrApproxEqual(SolarPlant.HourlyEnergyOutput[hour], approvedFeedInEnergy))
            {
                CalculatedData.AnnualFullPowerHours += 1;
                TransferEnergyToGrid(approvedFeedInEnergy, hour);

                double energyToBattery = SolarPlant.HourlyEnergyOutput[hour] - approvedFeedInEnergy;
                RejectedEnergy rejEnergy = TransferPossibleEnergyFromSolarPlantToBatteryStorage(energyToBattery, hour);
                CalculatedData.AnnualRejectedEnergy += rejEnergy;
            }
            else if (IsLessThan(SolarPlant.HourlyEnergyOutput[hour], approvedFeedInEnergy) && IsGreaterThanOrEqualToZero(SolarPlant.HourlyEnergyOutput[hour]))
            {
                TransferEnergyToGrid(SolarPlant.HourlyEnergyOutput[hour], hour);

                double energyFromBattery = approvedFeedInEnergy - SolarPlant.HourlyEnergyOutput[hour];
                TryTransferPossibleEnergyFromBatteryStorageToGrid(energyFromBattery, hour);
            }
            else
            {
                HandlePlantSelfConsumptionDeficit(SolarPlant.HourlyEnergyOutput[hour], hour);
            }
        }

        private void TransferEnergyToGrid(double energy, int hour)
        {
            CalculatedData.AnnualEnergyToGrid += energy;
            CalculatedData.EnergySalesRevenue += energy * PowerGrid.HourlyFeedInEnergyPrice[hour];
        }

        private RejectedEnergy TransferPossibleEnergyFromSolarPlantToBatteryStorage(double energy, int hour)
        {
            if (EnergyStorage == null)
                return energy;

            if (IsLessThan(EnergyStorage.CurrentCapacity, EnergyStorage.RatedCapacity))
            {
                ChargeResult result = EnergyStorage.TryCharge(energy);
                energy -= result.ChargedEnergy;
            }
            return energy;
        }

        private void TryTransferPossibleEnergyFromBatteryStorageToGrid(double energy, int hour)
        {
            if (EnergyStorage == null)
                return;
            
            if (!IsGreaterThanZero(EnergyStorage.CurrentCapacity))
                return;

            if (IsLessThan(PowerGrid.HourlyFeedInEnergyPrice[hour], MinBatteryDischargePrice[_currentMonthIndex]))
                return;

            DischargeResult result = EnergyStorage.TryDischarge(energy);
            CalculatedData.AnnualEnergyFromBattery += result.DischargedEnergy;
            TransferEnergyToGrid(result.DischargedEnergy, hour);
        }

        private void HandlePlantSelfConsumptionDeficit(double selfConsuptionEnergy, int hour)
        {
            selfConsuptionEnergy = Math.Abs(selfConsuptionEnergy);

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
            if (EnergyStorage == null)
                return DischargeResult.Failure();

            if (IsLessThan(PowerGrid.HourlyFeedInEnergyPrice[hour], MinBatteryDischargePrice[_currentMonthIndex])) // when storage is on, it supplies grid (and plant), so this should be checked
                return DischargeResult.Failure();

            if (IsLessThanOrApproxEqual(EnergyStorage.CurrentCapacity, energy)) // do not use last kWh to supplie plant, in that case pull it from the grid
                return DischargeResult.Failure();

            DischargeResult result = EnergyStorage.TryDischarge(energy);
            if (result.IsSuccessful)
            {
                return result;
            }
            else
            {   // Invalid scenario. The battery cannot simultaneously supply power to the grid and partially to the plant, while the plant is also being partially powered by the grid.
                // Put energy back to the battery.
                ChargeResult chResult = EnergyStorage.TryCharge(result.DischargedEnergy);
                if (!IsApproximatelyEqual(chResult.ChargedEnergy, result.DischargedEnergy)) throw new InvalidOperationException("Neuspesno vracanje energije u bateriju prilikom neuspesnog praznjenja baterije.");
                return DischargeResult.Failure();
            }
        }

        private void TransferEnergyFromGrid(double energy)
        {
            energy = Math.Abs(energy);

            CalculatedData.AnnualEnergyFromGrid += energy;
            CalculatedData.EnergyPurchaseCost += energy * PowerGrid.ExportEnergyPrice;
        }

        #endregion Prioritize transferring energy to grid

        #region Prioritize storing energy in battery

        private void PrioritizeStoringEnergyInBattery(int hour, FeedInStrategyToGrid feedInToGrid)
        {
            if (IsGreaterThanOrEqualToZero(SolarPlant.HourlyEnergyOutput[hour]))
            {
                if (IsGreaterThanOrApproxEqual(SolarPlant.HourlyEnergyOutput[hour], PowerGrid.ApprovedFeedInPower /* x 1h */))
                    CalculatedData.AnnualFullPowerHours += 1;

                double notStoredEnergy = TransferPossibleEnergyFromSolarPlantToBatteryStorage(SolarPlant.HourlyEnergyOutput[hour], hour);

                double rejectedEnergy = notStoredEnergy;
                if (feedInToGrid == FeedInStrategyToGrid.Allowed)
                {
                    double energyToGrid = Math.Min(PowerGrid.ApprovedFeedInPower, notStoredEnergy);
                    TransferEnergyToGrid(energyToGrid, hour);
                    rejectedEnergy -= energyToGrid;
                }
                CalculatedData.AnnualRejectedEnergy += rejectedEnergy;
            }
            else
            {
                TransferEnergyFromGrid(SolarPlant.HourlyEnergyOutput[hour]);
            }
        }

        #endregion Prioritize storing energy in battery

        private enum FeedInStrategyToGrid { NotAllowed, Allowed };
    }
}
