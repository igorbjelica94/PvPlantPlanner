using PvPlantPlanner.Common.Config;
using PvPlantPlanner.Common.CoreTypes;
using PvPlantPlanner.Common.DomainTypes;
using PvPlantPlanner.Common.Enums;
using PvPlantPlanner.Common.Exceptions;
using PvPlantPlanner.Common.Results;
using PvPlantPlanner.EnergyModels.BatteryModules;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.PowerGrids;
using PvPlantPlanner.EnergyModels.PowerPlants;
using PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers;
using PvPlantPlanner.Common.Exceptions;
using static PvPlantPlanner.Common.Consts.TimeConstants;
using static PvPlantPlanner.Common.Helpers.MathHelper;

namespace PvPlantPlanner.EnergyTransferSimulator.EnergyTransferSimulator
{
    public class AnnualEnergyTransferSimulator : IEnergyTransferSimulator
    {
        internal List<PvSimulationVariant> InputCalculationData => _inputCalculationData; // For Testing purposes
        internal List<PvCalculatedData> OutputCalculationData => _outputCalculationData; // For Testing purposes

        private bool _isConfigured = false;
        private CalculationConfig? _configuration;
        private EnergyTransferManager? _energyTransferManager;
        private List<PvSimulationVariant> _inputCalculationData = new List<PvSimulationVariant>();
        private List<PvCalculatedData> _outputCalculationData = new List<PvCalculatedData>();


        public void ConfigureSimulator(CalculationConfig config)
        {
            _configuration = config;

            var solarPlant = new SolarPowerPlant(
                installedPower: _configuration.BaseConfig.InstalledPower,
                investmentCost: _configuration.BaseConfig.ConstructionPrice,
                hourlyEnergyProduction: _configuration.GenerationData.ToArray(),
                selfConsumptionEnergy: CreateSelfConsumptionEnergyParameter());

            var powerGrid = new PowerGrid(
                approvedFeedInPower: _configuration.BaseConfig.MaxApprovedPower,
                allowedExportPower: _configuration.BaseConfig.MaxGridPower,
                hourlyFeedInEnergyPrice: CreateHourlyFeedInEnergyPriceParameter(),
                exportEnergyPrice: _configuration.BaseConfig.ElectricityPrice);

            CreateSimulationVariants();

            _energyTransferManager = new EnergyTransferManager(
                solarPlant: solarPlant,
                powerGrid: powerGrid,
                feedInPriorityPrice: new List<double>(_configuration.MinEnergySellingPrice ?? throw new ArgumentNullException()),
                minBatteryDischargePrice: new List<double>(_configuration.MinBatteryEnergySellingPrice ?? throw new ArgumentNullException()));

            _isConfigured = true;
        }

        public void StartSimulation()
        {
            if (!_isConfigured)
                throw new InvalidConfigurationException("Proracun nije dobro konfigurisan.");
            
            ArgumentNullException.ThrowIfNull(_energyTransferManager, nameof(_energyTransferManager));

            var feedInPriorityPrices = _energyTransferManager.FeedInPriorityPrice;

            SimulateFullEnergyTransferToGridWithoutStorage();
            SimulateNonNegativePriceEnergyTransferToGridWithoutStorage();

            _energyTransferManager.ReplaceFeedInPriorityPrice(feedInPriorityPrices);

            foreach (var inputVariant in _inputCalculationData)
            {
                _energyTransferManager.EnergyStorage = CreateBatteryStorageFromSimulationVariant(inputVariant);
                _energyTransferManager.ResetCalculatedData();
                for (int i = 0; i < 8760; i++)
                {
                    _energyTransferManager.ExecuteEnergyTransferForHour(i);
                }
                _outputCalculationData.Add(_energyTransferManager.CalculatedData);
            }
        }

        private HourlyValue<double> CreateSelfConsumptionEnergyParameter()
        {
            if (_configuration == null)
                throw new ArgumentNullException(nameof(_configuration), "Konfiguracija proracuna je prazna {{null}}.");

            if (_configuration.BaseConfig.SelfConsumptionFactor == null)
            {
                return _configuration.SelfConsumptionData?.ToArray() ?? throw new ArgumentNullException(nameof(_configuration.SelfConsumptionData), "Niz podataka sopstvene potrosnje elektrane ne postoji u konfiguraciji.");
            }
            else
            {
                return _configuration.BaseConfig.SelfConsumptionFactor * _configuration.BaseConfig.MaxGridPower;
            }
        }

        private HourlyValue<double> CreateHourlyFeedInEnergyPriceParameter()
        {
            if (_configuration == null)
                throw new ArgumentNullException(nameof(_configuration), "Konfiguracija proracuna je prazna {{null}}.");

            if (_configuration.BaseConfig.FixedPrice == null)
            {
                return _configuration.MarketPrice?.ToArray() ?? throw new ArgumentNullException(nameof(_configuration.MarketPrice), "Niz podataka cene elektricne energije na berzi ne postoji u konfiguraciji.");
            } else
            {
                for (int i = 0; i < _configuration.MarketPrice.Count; i++)
                {
                    _configuration.MarketPrice[i] = IsLessThanOrEqualToZero(_configuration.MarketPrice[i]) ? _configuration.BaseConfig.NegativePrice.Value : _configuration.BaseConfig.FixedPrice.Value;
                }
                return _configuration.MarketPrice.ToArray();
            }
        }

        private void CreateSimulationVariants()
        {
            var alreadyExist = new HashSet<string>();
            var possibleVariants = new Queue<(List<BatteryDto> OneVariant, double TotalPower)>();

            possibleVariants.Enqueue((new List<BatteryDto>(), 0.0)); // Initialization of possible variants

            while (possibleVariants.Count > 0)
            {
                var (currentVariant, currentPower) = possibleVariants.Dequeue();

                foreach (var battery in _configuration.BaseConfig.SelectedBatteries)
                {
                    double newPower = currentPower + battery.Power;
                    if (newPower > _configuration.BaseConfig.MaxBatteryPower)
                        continue;

                    var newVariant = new List<BatteryDto>(currentVariant);
                    newVariant.Add(battery);
                    var variantKey = string.Join("+", newVariant.GroupBy(b => b.Id).OrderBy(g => g.Key).Select(g => $"{g.Count()}x{g.Key}"));

                    if (alreadyExist.Add(variantKey))
                        _inputCalculationData.Add(CreateSimulationVariant(newVariant));

                    possibleVariants.Enqueue((newVariant, newPower));
                }
            }
        }

        private PvSimulationVariant CreateSimulationVariant(List<BatteryDto> batteries)
        {
            var simulationVariant = new PvSimulationVariant();

            simulationVariant.SelectedBatteryModuls = new List<BatteryDto>(batteries);
            simulationVariant.RatedStoragePower = batteries.Sum(b => b.Power);
            simulationVariant.RatedStorageCapacity = batteries.Sum(b => b.Capacity);

            var transformerStation = GetLowestCostTransformerStation(simulationVariant.RatedStoragePower);
            if (transformerStation == null)
                throw new ArgumentNullException();

            simulationVariant.SelectedTransformers = transformerStation;

            return simulationVariant;
        }

        private List<TransformerDto>? GetLowestCostTransformerStation(double minPower)
        {
            int skipAlgoritham = 10;
            var bestCombination = (Transformers: (List<TransformerDto>?)null, TotalPrice: double.MaxValue);
            var possibleStations = new Queue<List<TransformerDto>>();
            var alreadyExist = new HashSet<string>();

            possibleStations.Enqueue(new List<TransformerDto>());

            while (possibleStations.Count > 0)
            {
                var currentCombination = possibleStations.Dequeue();

                double currentTotalPower = currentCombination.Sum(t => t.PowerKW);
                double currentTotalPrice = (double)currentCombination.Sum(t => t.Price);

                if (currentTotalPower >= minPower)
                {
                    if (currentTotalPrice < bestCombination.TotalPrice)
                        bestCombination = (new List<TransformerDto>(currentCombination), currentTotalPrice);

                    continue;
                }

                if (currentCombination.Count >= skipAlgoritham)
                    continue;

                foreach (var t in _configuration?.BaseConfig.SelectedTransformers ?? throw new Exception())
                {
                    var nextCombination = new List<TransformerDto>(currentCombination) { t };

                    var key = string.Join("+", nextCombination.GroupBy(x => x.PowerKW).OrderBy(g => g.Key).Select(g => $"{g.Count()}x{g.Key:F2}"));

                    if (alreadyExist.Add(key))
                        possibleStations.Enqueue(nextCombination);
                }
            }
            return bestCombination.Transformers;

        }

        private BatteryStorage CreateBatteryStorageFromSimulationVariant(PvSimulationVariant simulationVariant)
        {
            List<IBatteryModule> batteryModules = new List<IBatteryModule>();

            foreach (var battery in simulationVariant.SelectedBatteryModuls)
            {
                var batteryModule = new BatteryModule(battery.Power, battery.Capacity, (int)battery.Price, battery.Cycles);
                batteryModules.Add(batteryModule);
            }

            return new BatteryStorage(batteryModules);
        }

        private void SimulateFullEnergyTransferToGridWithoutStorage()
        {
            if (_energyTransferManager == null)
                throw new ArgumentNullException(nameof(_energyTransferManager));

            _energyTransferManager.ResetCalculatedData();
            _energyTransferManager.ReplaceFeedInPriorityPrice(Enumerable.Repeat(Double.MinValue, NumberOfMonth).ToList());
            for (int i = 0; i < 8760; i++)
            {
                _energyTransferManager.ExecuteEnergyTransferForHour(i);
            }
            _outputCalculationData.Add(_energyTransferManager.CalculatedData);
        }

        private void SimulateNonNegativePriceEnergyTransferToGridWithoutStorage()
        {
            if (_energyTransferManager == null)
                throw new ArgumentNullException(nameof(_energyTransferManager));

            _energyTransferManager.ResetCalculatedData();
            _energyTransferManager.ReplaceFeedInPriorityPrice(Enumerable.Repeat(0.0, NumberOfMonth).ToList());
            for (int i = 0; i < 8760; i++)
            {
                _energyTransferManager.ExecuteEnergyTransferForHour(i);
            }
            _outputCalculationData.Add(_energyTransferManager.CalculatedData);
        }
    }
}
