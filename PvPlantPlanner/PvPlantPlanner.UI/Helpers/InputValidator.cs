namespace PvPlantPlanner.UI.Helpers
{
    public static class InputValidator
    {
        public static bool ValidateInputs(MainWindow window, out string errorMessage)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(window.InstalledPowerTextBox.Text))
                errors.Add("Polje 'Instalisana snaga elektrane' nije popunjeno.");
            else if (!double.TryParse(window.InstalledPowerTextBox.Text, out double installedPower) || installedPower <= 0)
                errors.Add("Instalisana snaga elektrane mora biti pozitivan broj.");

            if (string.IsNullOrWhiteSpace(window.MaxApprovedPowerTextBox.Text))
                errors.Add("Polje 'Maksimalna odobrena snaga' nije popunjeno.");
            else if (!double.TryParse(window.MaxApprovedPowerTextBox.Text, out double maxApprovedPower) || maxApprovedPower <= 0)
                errors.Add("Maksimalna odobrena snaga mora biti pozitivan broj.");

            if (string.IsNullOrWhiteSpace(window.ConstructionPriceTextBox.Text))
                errors.Add("Polje 'Cena izgradnje' nije popunjeno.");
            else if (!double.TryParse(window.ConstructionPriceTextBox.Text, out double constructionPrice) || constructionPrice <= 0)
                errors.Add("Cena izgradnje mora biti pozitivan broj.");

            if (string.IsNullOrWhiteSpace(window.MaxGridPowerTextBox.Text))
                errors.Add("Polje 'Maksimalna snaga iz mreže' nije popunjeno.");
            else if (!double.TryParse(window.MaxGridPowerTextBox.Text, out double maxGridPower) || maxGridPower <= 0)
                errors.Add("Maksimalna snaga iz mreže mora biti pozitivan broj.");

            if (window.SelfConsumptionFactorRadioButton.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(window.SelfConsumptionFactorTextBox.Text))
                    errors.Add("Polje 'Faktor sopstvene potrošnje' nije popunjeno.");
                else if (!double.TryParse(window.SelfConsumptionFactorTextBox.Text, out double factor) || factor <= 0 || factor > 1)
                    errors.Add("Faktor sopstvene potrošnje mora biti broj između 0 i 1.");
            }
            else if (window.SelfConsumptionFactorRadioButton.IsChecked != true)
            {
                if (window.selfConsumptionData is null || !window.selfConsumptionData.Any())
                    errors.Add("Niste učitali podatke o satnoj sopstvenoj potrošnji elektrane.");
            }

            if (string.IsNullOrWhiteSpace(window.ElectricityPriceTextBox.Text))
                errors.Add("Polje 'Cena električne energije preuzete iz mreže' nije popunjeno.");

            if (window.FixedPriceRadioButton.IsChecked != true)
            {
                if (string.IsNullOrWhiteSpace(window.TradingCommissionTextBox.Text))
                    errors.Add("Polje 'Trgovačka provizija' nije popunjeno.");
                else if (!double.TryParse(window.TradingCommissionTextBox.Text, out double commission) || commission < 0 || commission > 100)
                    errors.Add("Trgovačka provizija mora biti broj između 0 i 100.");

                if (window.minEnergySellingPrices is null || window.minBatteryEnergySellingPrices is null || !window.minEnergySellingPrices.Any() || !window.minBatteryEnergySellingPrices.Any())
                    errors.Add("Niste učitali cenu aktivacije prodaje energije i pražnjenja baterija.");

            }
            else if (window.FixedPriceRadioButton.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(window.FixedPriceTextBox.Text))
                    errors.Add("Polje 'Fiksna cena' nije popunjeno.");
                else if (!double.TryParse(window.FixedPriceTextBox.Text, out double fixedPrice) || fixedPrice <= 0)
                    errors.Add("Fiksna cena mora biti pozitivan broj.");

                if (string.IsNullOrWhiteSpace(window.NegativePriceTextBox.Text))
                    errors.Add("Polje 'Cena pri negativnoj berzanskoj ceni' nije popunjeno.");
            }

            if (string.IsNullOrWhiteSpace(window.MaxBatteryPowerTextBox.Text))
                errors.Add("Polje 'Maksimalna instalisana snaga baterijskog sistema' nije popunjeno.");
            else if (!double.TryParse(window.MaxBatteryPowerTextBox.Text, out double maxBatteryPower) || maxBatteryPower < 0)
                errors.Add("Maksimalna instalisana snaga baterijskog sistema ne sme biti negativna.");

            if (window.SelectedBatteries != null && window.SelectedBatteries.Count > 0)
            {
                if (window.SelectedTransformers == null || window.SelectedTransformers.Count == 0)
                {
                    errors.Add("Niste dodali nijedan transformator.");
                }
            }

            if (window.generationData is null || !window.generationData.Any())
                errors.Add("Niste učitali podatke o satnoj proizvodnji elektrane.");

            if (window.marketPriceData is null || !window.marketPriceData.Any())
                errors.Add("Niste učitali podatke o ceni električne energije na berzi.");

            errorMessage = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}
