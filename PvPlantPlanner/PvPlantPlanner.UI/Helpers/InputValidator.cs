using System.Collections.Generic;

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

            if (window.SelfConsumptionFactorRadioButton.IsChecked != true)
            {
                if (string.IsNullOrEmpty(window.StatusIcon_SelfConsumption.Text) || !window.StatusIcon_SelfConsumption.Text.Contains("✓"))
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

                if (string.IsNullOrWhiteSpace(window.MinSellingPriceTextBox.Text))
                    errors.Add("Polje 'Minimalna prodajna cena' nije popunjeno.");
                else if (!double.TryParse(window.MinSellingPriceTextBox.Text, out double minSellingPrice) || minSellingPrice < 0)
                    errors.Add("Minimalna prodajna cena ne sme biti negativna.");

                if (string.IsNullOrWhiteSpace(window.MinBatteryDischargePriceTextBox.Text))
                    errors.Add("Polje 'Minimalna cena za pražnjenje baterije' nije popunjeno.");
                else if (!double.TryParse(window.MinBatteryDischargePriceTextBox.Text, out double minDischargePrice) || minDischargePrice < 0)
                    errors.Add("Minimalna cena za pražnjenje baterije ne sme biti negativna.");
            }

            if (window.FixedPriceRadioButton.IsChecked == true)
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

            if (window.SelectedBatteries == null || window.SelectedBatteries.Count == 0)
                errors.Add("Niste dodali nijednu bateriju.");

            if (window.SelectedTransformers == null || window.SelectedTransformers.Count == 0)
                errors.Add("Niste dodali nijedan transformator.");

            if (string.IsNullOrEmpty(window.StatusIcon_P_Gen_Data.Text) || !window.StatusIcon_P_Gen_Data.Text.Contains("✓"))
                errors.Add("Niste učitali podatke o satnoj proizvodnji elektrane.");

            if (string.IsNullOrEmpty(window.StatusIcon_Market_Price.Text) || !window.StatusIcon_Market_Price.Text.Contains("✓"))
                errors.Add("Niste učitali podatke o ceni električne energije na berzi.");

            errorMessage = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}
