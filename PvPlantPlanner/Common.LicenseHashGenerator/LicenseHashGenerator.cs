using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Common.LicenseHashGenerator
{
    public class LicenseHashGenerator
    {
        private const string licenseFile = "license.txt";
        private const string licenseHashFile = "license_hash.txt";

        public void GenerateLicensgeHashKey()
        {
            try
            {
                // 1. Učitaj license key iz license.txt
                if (!File.Exists(licenseFile))
                {
                    Console.WriteLine($"{licenseFile} nije pronađen!");
                    return;
                }

                string licenseKey = File.ReadAllText(licenseFile).Trim();

                // 2. Dohvati MachineGuid iz registry-ja
                string machineId = GetMachineGuid();
                if (string.IsNullOrEmpty(machineId))
                {
                    Console.WriteLine("Ne mogu da dohvatim MachineGuid!");
                    return;
                }

                // 3. Generiši hash (SHA256)
                string combined = licenseKey + machineId;
                string hash = ComputeSha256Hash(combined);

                // 4. Snimi hash u novi fajl
                File.WriteAllText(licenseHashFile, hash);

                Console.WriteLine("Hash je generisan i snimljen u: " + licenseHashFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška: " + ex.Message);
            }
        }

        public bool CheckLicensgeHashKey()
        {
            try
            {
                // 1. Učitaj license key iz license.txt
                if (!File.Exists(licenseFile)) throw new Exception("Fajl sa licencom nije pronađen.");

                string licenseKey = File.ReadAllText(licenseFile).Trim();

                // 2. Dohvati MachineGuid iz registry-ja
                string machineId = GetMachineGuid();
                if (string.IsNullOrEmpty(machineId)) throw new Exception("Uuid masine nije moguce dohvatiti.");

                // 3. Generiši hash (SHA256)
                string combined = licenseKey + machineId;
                string newHash = ComputeSha256Hash(combined);

                // 4. Dohvati postojeći hash iz fajla (ako postoji)
                string existingHash = "";
                if (File.Exists(licenseHashFile))
                {
                    existingHash = File.ReadAllText(licenseHashFile).Trim();
                }

                // 5. Uporedi nove hash sa postojećim
                if (existingHash != newHash) throw new Exception("Vasa licenca nije ispravna!");

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private string GetMachineGuid()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        object guid = key.GetValue("MachineGuid");
                        if (guid != null)
                            return guid.ToString();
                    }
                }
            }
            catch { }
            return "";
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
