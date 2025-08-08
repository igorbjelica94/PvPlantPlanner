using ClosedXML.Excel;
using PvPlantPlanner.Common.Config;
using PvPlantPlanner.Common.DomainTypes;
using PvPlantPlanner.EnergyModels.DomainTypes;
using System.Diagnostics;

namespace PvPlantPlanner.Tools.ReportGenerator
{
    public class ExcelReportGenerator
    {
        private readonly List<PvSimulationVariant> _inputs;
        private readonly List<PvCalculatedData> _outputs;
        private readonly CalculationConfig _configuration;

        private XLWorkbook _generatedExcel;

        public ExcelReportGenerator(List<PvSimulationVariant> inputs, List<PvCalculatedData> outputs, CalculationConfig configuration)
        {
            _inputs = inputs;
            _outputs = outputs;
            _configuration = configuration;
        }

        public void GenerateReport()
        {
            using (_generatedExcel = new XLWorkbook())
            {
                _generatedExcel.Worksheets.Add("Izvestaj");

                GenerateHeader();
                PopulateExcelWithBasicVariantsData();
                PopulateExcelWithBatteryVariantsData();

                var fileName = GetTempFileName();
                _generatedExcel.SaveAs(fileName);
                Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
            }
        }

        #region Generate Header

        private void GenerateHeader()
        {
            GenerateLogoPart();
            GenerateTitlePart();
            GenerateColumnHeaders();
        }

        private void GenerateLogoPart()
        {
            var woorksheet = _generatedExcel.Worksheet(1);

            var range = woorksheet.Range("A1:C2");
            range.Merge();

            range.Value = "{Logo}";
            foreach (var cell in range.Cells())
            {
                ApplyCommonDataStyle(cell, XLBorderStyleValues.Thick);
            }
        }

        private void GenerateTitlePart()
        {
            var woorksheet = _generatedExcel.Worksheet(1);

            var range = woorksheet.Range("D1:AB2");
            range.Merge();

            range.Value = new string('\t', 10) + "Tehno-ekonomski parametri solarne elektrane sa i bez baterijskog sistema različite snage i kapaciteta";
            foreach (var cell in range.Cells())
            {
                ApplyCommonDataStyle(cell, XLBorderStyleValues.Thick);
                var style = cell.Style;
                style.Font.Bold = true;
                style.Font.FontSize = 16;
                style.Fill.SetBackgroundColor(XLColor.FromArgb(191, 191, 191));
                style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            }

            woorksheet.Row(1).Height = 21;
            woorksheet.Row(2).Height = 21;
        }

        private void GenerateColumnHeaders()
        {
            var woorksheet = _generatedExcel.Worksheet(1);

            string jsonConfig = File.ReadAllText("Resources\\ColumnHeaders.json");
            var headersConfig = System.Text.Json.JsonSerializer.Deserialize<ColumnHeadersConfig>(jsonConfig);

            int row = 3;
            for (int i = 0; i < headersConfig.ColumnHeaders.Count; i++)
            {
                int col = i + 1;
                var cell = woorksheet.Cell(row, col);
                ApplyCommonDataStyle(cell, XLBorderStyleValues.Thick);
                var style = cell.Style;
                style.Alignment.WrapText = true;
                style.Fill.SetBackgroundColor(XLColor.FromArgb(191, 191, 191));

                cell.Value = headersConfig.ColumnHeaders[i].Name;
                woorksheet.Column(col).Width = headersConfig.ColumnHeaders[i].Width;
            }
            woorksheet.Row(row).Height = headersConfig.Height;

        }

        #endregion Generate Header

        #region Populate data

        private void PopulateExcelWithBasicVariantsData()
        {
            var ws = _generatedExcel.Worksheet(1);
            int currentRow = 4;

            for (int i = 0; i < 3 && i < _outputs.Count; i++)
            {
                var output = _outputs[i];
                var variantNameLabel = i == 0 ? "PV_all" :
                            i == 1 ? "PV_cut" :
                            "PV_avgmop";

                WriteCell(ws, currentRow, 1, variantNameLabel);
                WriteCell(ws, currentRow, 2, _configuration.BaseConfig.InstalledPower);
                WriteCell(ws, currentRow, 3, _configuration.BaseConfig.ConstructionPrice);
                var annualIncome = output.EnergySalesRevenue - output.EnergyPurchaseCost;
                WriteCell(ws, currentRow, 16, annualIncome);
                WriteCell(ws, currentRow, 17, output.EnergySalesRevenue);
                WriteCell(ws, currentRow, 18, output.EnergyPurchaseCost);
                WriteCell(ws, currentRow, 19, output.AnnualEnergyToGrid);
                WriteCell(ws, currentRow, 20, output.AnnualEnergyFromGrid);
                WriteCell(ws, currentRow, 21, output.AnnualEnergyFromBattery);
                WriteCell(ws, currentRow, 22, output.AnnualFullPowerHours);
                WriteCell(ws, currentRow, 25, output.AnnualRejectedEnergy);
                var capex = _configuration.BaseConfig.InstalledPower * _configuration.BaseConfig.ConstructionPrice;
                WriteCell(ws, currentRow, 26, capex);
                var roiAnnual = capex / annualIncome;
                WriteCell(ws, currentRow, 27, roiAnnual);
                var roiPercent = (annualIncome / capex) * 100;
                WriteCell(ws, currentRow, 28, roiPercent);

                currentRow++;
            }

            var rng = ws.Range(5, 1, 5, 28);
            rng.Style.Fill.SetBackgroundColor(XLColor.FromArgb(217, 217, 217));
        }

        private void PopulateExcelWithBatteryVariantsData()
        {
            if (_outputs.Count <= 3) return;
            var ws = _generatedExcel.Worksheet(1);
            int currentVariant = 1;
            int currentRow = 7;

            foreach (var output in _outputs.Skip(3))
            {
                int index = currentVariant - 1;
                WriteCell(ws, currentRow, 1, $"PVB {currentVariant}");
                WriteCell(ws, currentRow, 2, _configuration.BaseConfig.InstalledPower);
                WriteCell(ws, currentRow, 3, _configuration.BaseConfig.ConstructionPrice);
                WriteCell(ws, currentRow, 4, _inputs[index].RatedStoragePower);
                WriteCell(ws, currentRow, 5, _inputs[index].RatedStorageCapacity);

                // Batteries
                //var groupedBatteryTypes = _inputs[index].SelectedBatteryModuls.GroupBy(b => b.No);
                var groupedBatteryTypes = output.BatteryStorage.BatteryModules.GroupBy(m => new
                    {
                        m.RatedPower,
                        m.RatedCapacity,
                        m.InvestmentCost,
                        m.MaxCycleCount
                    });
                int batterySystemPrice = 0;
                int currentBatSubrow = 0;
                foreach (var group in groupedBatteryTypes)
                {
                    var first = group.First();
                    WriteCell(ws, currentRow + currentBatSubrow, 6, $"{first.RatedPower} kW - {first.RatedCapacity} kWh");
                    WriteCell(ws, currentRow + currentBatSubrow, 7, group.Count());
                    WriteCell(ws, currentRow + currentBatSubrow, 8, first.InvestmentCost);
                    WriteCell(ws, currentRow + currentBatSubrow, 23, first.CurrentCycleCount);
                    WriteCell(ws, currentRow + currentBatSubrow, 24, first.SocUtilization);
                    batterySystemPrice += group.Count() * first.InvestmentCost;
                    currentBatSubrow++;
                }
                WriteCell(ws, currentRow, 9, batterySystemPrice);

                // Transformers
                WriteCell(ws, currentRow, 10, _inputs[index].SelectedTransformers.Count());
                var groupedTransformerTypes = _inputs[index].SelectedTransformers.GroupBy(b => b.No);
                int transformerSystemPrice = 0;
                int currentTraSubrow = 0;
                foreach (var group in groupedTransformerTypes)
                {
                    var first = group.First();
                    WriteCell(ws, currentRow + currentTraSubrow, 11, first.PowerKVA);
                    WriteCell(ws, currentRow + currentTraSubrow, 12, group.Count());
                    WriteCell(ws, currentRow + currentTraSubrow, 13, first.Price);
                    transformerSystemPrice += group.Count() * first.Price;
                    currentTraSubrow++;
                }
                WriteCell(ws, currentRow, 14, transformerSystemPrice);
                var totalBatAndTraPrice = batterySystemPrice + transformerSystemPrice;
                WriteCell(ws, currentRow, 15, totalBatAndTraPrice);

                var annualIncome = output.EnergySalesRevenue - output.EnergyPurchaseCost;
                WriteCell(ws, currentRow, 16, annualIncome);
                WriteCell(ws, currentRow, 17, output.EnergySalesRevenue);
                WriteCell(ws, currentRow, 18, output.EnergyPurchaseCost);
                WriteCell(ws, currentRow, 19, output.AnnualEnergyToGrid);
                WriteCell(ws, currentRow, 20, output.AnnualEnergyFromGrid);
                WriteCell(ws, currentRow, 21, output.AnnualEnergyFromBattery);
                WriteCell(ws, currentRow, 22, output.AnnualFullPowerHours);
                WriteCell(ws, currentRow, 25, output.AnnualRejectedEnergy);
                var capex = _configuration.BaseConfig.InstalledPower * _configuration.BaseConfig.ConstructionPrice;
                WriteCell(ws, currentRow, 26, capex);
                var roiAnnual = capex / annualIncome;
                WriteCell(ws, currentRow, 27, roiAnnual);
                var roiPercent = (annualIncome / capex) * 100;
                WriteCell(ws, currentRow, 28, roiPercent);

                int subrows = Math.Max(currentBatSubrow, currentTraSubrow);
                if (currentVariant % 2 == 1)
                {
                    var rng = ws.Range(currentRow, 1, currentRow + (subrows > 0 ? subrows - 1 : 0), 28);
                    rng.Style.Fill.SetBackgroundColor(XLColor.FromArgb(217, 217, 217));
                }

                if (subrows > 0)
                {
                    currentRow += subrows;
                }
                else
                {
                    currentRow++;
                }
                currentVariant++;
            }
        }

        private void WriteCell(IXLWorksheet ws, int row, int col, object value)
        {
            var cell = ws.Cell(row, col);

            switch (value)
            {
                case double d:
                    cell.Value = Math.Round(d, 2);
                    break;
                case float f:
                    cell.Value = Math.Round(f, 2);
                    break;
                case decimal m:
                    cell.Value = Math.Round(m, 2);
                    break;
                case int i: cell.Value = i; break;
                case long l: cell.Value = l; break;
                case short s: cell.Value = s; break;
                case byte b: cell.Value = b; break;
                case uint ui: cell.Value = ui; break;
                case ulong ul: cell.Value = ul; break;
                case ushort us: cell.Value = us; break;
                case sbyte sb: cell.Value = sb; break;
                default:
                    cell.Value = value?.ToString() ?? "";
                    break;
            }


            ApplyCommonDataStyle(cell);
        }

        #endregion Populate data

        #region Format cells

        private void ApplyCommonDataStyle(IXLCell cell, ClosedXML.Excel.XLBorderStyleValues border = XLBorderStyleValues.Thin)
        {
            var style = cell.Style;

            // Font
            style.Font.FontName = "Cambria";

            // Alignment
            style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Border
            style.Border.OutsideBorder = border;
            style.Border.InsideBorder = border;
        }

        #endregion Format cells

        private string GetTempFileName()
        {
            return Path.Combine(Path.GetTempPath(), $"ReportCopy_{System.DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}
