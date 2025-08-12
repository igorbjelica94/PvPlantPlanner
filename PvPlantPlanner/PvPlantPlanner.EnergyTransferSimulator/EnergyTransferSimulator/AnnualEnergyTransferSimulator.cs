using DocumentFormat.OpenXml.Presentation;
using PvPlantPlanner.Common.Config;
using PvPlantPlanner.Common.CoreTypes;
using PvPlantPlanner.Common.DomainTypes;
using PvPlantPlanner.Common.Enums;
using PvPlantPlanner.EnergyModels.BatteryModules;
using PvPlantPlanner.EnergyModels.BatteryStorages;
using PvPlantPlanner.EnergyModels.DomainTypes;
using PvPlantPlanner.EnergyModels.PowerGrids;
using PvPlantPlanner.EnergyModels.PowerPlants;
using PvPlantPlanner.EnergyTransferSimulator.EnergyTransferManagers;
using PvPlantPlanner.Tools.ReportGenerator;
using System.Text.Json;
using static PvPlantPlanner.Common.Consts.TimeConstants;
using static PvPlantPlanner.Common.Helpers.MathHelper;

namespace PvPlantPlanner.EnergyTransferSimulator.EnergyTransferSimulator
{
    public class AnnualEnergyTransferSimulator : IEnergyTransferSimulator
    {
        internal List<PvSimulationVariant> InputCalculationData => _inputCalculationData; // For Testing purposes
        internal List<PvCalculatedData> OutputCalculationData => _outputCalculationData; // For Testing purposes

        private CalculationConfig? _configuration;
        private EnergyTransferManager? _energyTransferManager;
        private List<PvSimulationVariant> _inputCalculationData = new List<PvSimulationVariant>();
        private List<PvCalculatedData> _outputCalculationData = new List<PvCalculatedData>();
        private CalculationConstants _calculationConstants = JsonSerializer.Deserialize<CalculationConstants>(File.ReadAllText("Conf\\CalculationConstants.json")) 
            ?? throw new ArgumentNullException(nameof(_calculationConstants), "Fajl Conf\\CalculationConstants.json nije uspesno serijalizovan.");

        private CalculationConfig Configuration => _configuration ?? throw new InvalidOperationException("Konfiguracija proracuna je prazna {{null}}.");
        private EnergyTransferManager EnergyTransferManager => _energyTransferManager ?? throw new InvalidOperationException("Menadzer za simulaciju transfera energije ne postoji {{nul}}.");

        public void ConfigureSimulator(CalculationConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config), "Primljena konfiguracija za proracun je prazna {{null}}.");

            _configuration = config;
            _inputCalculationData.Clear();
            _outputCalculationData.Clear();

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

            if (_configuration.BaseConfig.FixedPrice == null)
            {
                _energyTransferManager = new EnergyTransferManager(
                    solarPlant: solarPlant,
                    powerGrid: powerGrid,
                    feedInPriorityPrice: new List<double>(_configuration.MinEnergySellingPrice ?? throw new ArgumentNullException(nameof(_configuration.MinEnergySellingPrice), "Podaci o cenama aktivacije prodaje elektricne energije ne postoje u konfiguraciji.")),
                    minBatteryDischargePrice: new List<double>(_configuration.MinBatteryEnergySellingPrice ?? throw new ArgumentNullException(nameof(_configuration.MinBatteryEnergySellingPrice), "Podaci o cenama aktivacije praznjenja baterija ne postoje u konfiguraciji.")));
            }
            else
            {
                _energyTransferManager = new EnergyTransferManager(
                    solarPlant: solarPlant,
                    powerGrid: powerGrid,
                    feedInPriorityPrice: Enumerable.Repeat(_configuration.BaseConfig.FixedPrice.Value, NumberOfMonth).ToList(),
                    minBatteryDischargePrice: Enumerable.Repeat(_configuration.BaseConfig.FixedPrice.Value, NumberOfMonth).ToList());
            }

            CreateSimulationVariants();
        }

        public void StartSimulation(ExcelReportOption option = ExcelReportOption.Generate)
        {
            var savedFeedInPriorityPrices = EnergyTransferManager.FeedInPriorityPrice;

            SimulateFullEnergyTransferToGridWithoutStorage();
            SimulateNonNegativePriceEnergyTransferToGridWithoutStorage();
            SimulateAverageMonthlyPriceEnergyTransferToGridWithoutStorage();

            EnergyTransferManager.ReplaceFeedInPriorityPrice(savedFeedInPriorityPrices);

            foreach (var inputVariant in _inputCalculationData)
            {
                EnergyTransferManager.ResetCalculatedData();
                EnergyTransferManager.EnergyStorage = CreateBatteryStorageFromSimulationVariant(inputVariant);
                for (int i = 0; i < 8760; i++)
                {
                    EnergyTransferManager.ExecuteEnergyTransferForHour(i);
                }
                _outputCalculationData.Add(EnergyTransferManager.CalculatedData.Clone());
            }

            if (option == ExcelReportOption.Generate)
            {
                var excelReportGen = new ExcelReportGenerator(_inputCalculationData, _outputCalculationData, Configuration);
                excelReportGen.GenerateReport();
            }
        }

        private HourlyValue<double> CreateSelfConsumptionEnergyParameter()
        {
            if (Configuration.BaseConfig.SelfConsumptionFactor == null)
            {
                return Configuration.SelfConsumptionData?.ToArray() ?? throw new ArgumentNullException(nameof(_configuration.SelfConsumptionData), "Podaci o satnoj sopstvenoj potrosnji elektrane ne postoje u konfiguraciji.");
            }
            else
            {
                return Configuration.BaseConfig.SelfConsumptionFactor * Configuration.BaseConfig.MaxGridPower;
            }
        }

        private HourlyValue<double> CreateHourlyFeedInEnergyPriceParameter()
        {
            if (Configuration.BaseConfig.FixedPrice == null)
            {
                return RecalculateHourlyFeedInEnergyPriceWithTradingCommission();
            }
            else
            {
                return CalculateFixedHourlyFeedInEnergyPrice();
            }
        }

        private HourlyValue<double> RecalculateHourlyFeedInEnergyPriceWithTradingCommission()
        {
            if (Configuration.BaseConfig.TradingCommission == null) throw new ArgumentNullException(nameof(_configuration.BaseConfig.TradingCommission), "Podatak o trgovackoj proviziji kada je elektrana na berzi ne postoji u konfiguraciji.");

            double effectivePriceFactor = 1 - (Configuration.BaseConfig.TradingCommission.Value / 100);
            var recalculatedPrices = new double[Configuration.MarketPrice.Count];
            for (int i = 0; i < Configuration.MarketPrice.Count; i++)
            {
                recalculatedPrices[i] = Configuration.MarketPrice[i] * effectivePriceFactor;
            }
            return recalculatedPrices;
        }

        private HourlyValue<double> CalculateFixedHourlyFeedInEnergyPrice()
        {
            if (Configuration.BaseConfig.FixedPrice == null) throw new ArgumentNullException(nameof(_configuration.BaseConfig.NegativePrice), "Podatak o fiksnoj ceni elektricne energije ne postoji u konfiguraciji.");
            if (Configuration.BaseConfig.NegativePrice == null) throw new ArgumentNullException(nameof(_configuration.BaseConfig.NegativePrice), "Podatak o ceni elektricne energije pri negativnoj berzanskoj ceni ne postoji u konfiguraciji.");

            var recalculatedPrices = new double[Configuration.MarketPrice.Count];
            for (int i = 0; i < Configuration.MarketPrice.Count; i++)
            {
                recalculatedPrices[i] = IsGreaterThanZero(Configuration.MarketPrice[i]) ? Configuration.BaseConfig.FixedPrice.Value : Configuration.BaseConfig.NegativePrice.Value;
            }
            return recalculatedPrices;
        }

        private void CreateSimulationVariants()
        {
            var alreadyExist = new HashSet<string>();
            var possibleVariants = new Queue<(List<BatteryDto> OneVariant, double TotalPower)>();

            possibleVariants.Enqueue((new List<BatteryDto>(), 0.0)); // Initialization of possible variants

            while (possibleVariants.Count > 0)
            {
                var (currentVariant, currentPower) = possibleVariants.Dequeue();

                foreach (var battery in Configuration.BaseConfig.SelectedBatteries)
                {
                    double newPower = currentPower + battery.Power;
                    if (newPower > Configuration.BaseConfig.MaxBatteryPower)
                        continue;

                    var newVariant = new List<BatteryDto>(currentVariant);
                    newVariant.Add(battery);
                    var variantKey = string.Join("+", newVariant.GroupBy(b => b.No).OrderBy(g => g.Key).Select(g => $"{g.Count()}x{g.Key}"));

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
                throw new ArgumentNullException(nameof(transformerStation), $"Algoritam nije uspeo da nadje odgovarajucu kombinaciju transformatora za baterijski sistem snage [{simulationVariant.RatedStoragePower}] i kapaciteta [{simulationVariant.RatedStorageCapacity}].");

            simulationVariant.SelectedTransformers = transformerStation;

            return simulationVariant;
        }

        private List<TransformerDto>? GetLowestCostTransformerStation(double minPower)
        {
            var bestCombination = (Transformers: (List<TransformerDto>?)null, TotalPrice: double.MaxValue);
            var possibleStations = new Queue<List<TransformerDto>>();
            var alreadyExist = new HashSet<string>();

            possibleStations.Enqueue(new List<TransformerDto>());

            while (possibleStations.Count > 0)
            {
                var currentCombination = possibleStations.Dequeue();

                double currentTotalPower = currentCombination.Sum(t => t.PowerKW);
                int currentTotalPrice = currentCombination.Sum(t => t.Price);

                if (currentTotalPower >= minPower)
                {
                    if (currentTotalPrice < bestCombination.TotalPrice)
                        bestCombination = (new List<TransformerDto>(currentCombination), currentTotalPrice);

                    continue;
                }

                if (currentCombination.Count >= _calculationConstants.MaxTransformersPerBatteryStorage)
                    continue;

                foreach (var t in Configuration.BaseConfig.SelectedTransformers)
                {
                    var nextCombination = new List<TransformerDto>(currentCombination);
                    nextCombination.Add(t);

                    var key = string.Join("+", nextCombination.GroupBy(x => x.No).OrderBy(g => g.Key).Select(g => $"{g.Count()}x{g.Key}"));

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
                var batteryModule = new BatteryModule(battery.Power, battery.Capacity, battery.Price, battery.Cycles);
                batteryModules.Add(batteryModule);
            }

            return new BatteryStorage(batteryModules);
        }

        private void SimulateFullEnergyTransferToGridWithoutStorage()
        {
            EnergyTransferManager.ResetCalculatedData();
            EnergyTransferManager.ReplaceFeedInPriorityPrice(Enumerable.Repeat(Double.MinValue, NumberOfMonth).ToList());
            for (int i = 0; i < 8760; i++)
            {
                EnergyTransferManager.ExecuteEnergyTransferForHour(i);
            }
            _outputCalculationData.Add(EnergyTransferManager.CalculatedData.Clone());
        }

        private void SimulateNonNegativePriceEnergyTransferToGridWithoutStorage()
        {
            EnergyTransferManager.ResetCalculatedData();
            EnergyTransferManager.ReplaceFeedInPriorityPrice(Enumerable.Repeat(0.0, NumberOfMonth).ToList());
            for (int i = 0; i < 8760; i++)
            {
                EnergyTransferManager.ExecuteEnergyTransferForHour(i);
            }
            _outputCalculationData.Add(EnergyTransferManager.CalculatedData.Clone());
        }

        private void SimulateAverageMonthlyPriceEnergyTransferToGridWithoutStorage()
        {
            var savedFeedInEnergyPraces = EnergyTransferManager.PowerGrid.HourlyFeedInEnergyPrice;

            EnergyTransferManager.ResetCalculatedData();
            EnergyTransferManager.ReplaceFeedInEnergyPrice(CalculateAverageMonthlyElectricityPrices());
            for (int i = 0; i < 8760; i++)
            {
                EnergyTransferManager.ExecuteEnergyTransferForHour(i);
            }
            _outputCalculationData.Add(EnergyTransferManager.CalculatedData.Clone());

            EnergyTransferManager.ReplaceFeedInEnergyPrice(savedFeedInEnergyPraces);
        }

        private HourlyValue<double> CalculateAverageMonthlyElectricityPrices()
        {
            double[] monthlyAverages = new double[NumberOfMonth];

            for (int month = 0; month < NumberOfMonth; month++)
            {
                int startDay = (month == 0) ? 0 : MonthBounds[month - 1];
                int endDay = MonthBounds[month];
                int daysInMonth = endDay - startDay;

                int startHour = startDay * HoursPerDay;
                int endHour = endDay * HoursPerDay;

                double sum = 0;
                int count = 0;
                for (int h = startHour; h < endHour; h++)
                {
                    sum += Configuration.MarketPrice[h];
                    count++;
                }

                monthlyAverages[month] = sum / count;
            }

            double[] newMonthlyAverages = new double[HoursInYear];
            for (int month = 0; month < NumberOfMonth; month++)
            {
                int startDay = (month == 0) ? 0 : MonthBounds[month - 1];
                int endDay = MonthBounds[month];

                int startHour = startDay * HoursPerDay;
                int endHour = endDay * HoursPerDay;

                double avg = monthlyAverages[month];
                for (int h = startHour; h < endHour; h++)
                {
                    newMonthlyAverages[h] = avg;
                }
            }
            return new HourlyValue<double>(newMonthlyAverages);
        }
    }
}
