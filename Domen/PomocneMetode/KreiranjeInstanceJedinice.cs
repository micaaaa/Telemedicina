using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.PomocneMetode
{
    public class KreiranjeInstanceJedinice
    {


        public void PokreniJedinice(TipJedinice tipJedinice)
        {
            string unitPath;

            switch (tipJedinice)
            {
                case TipJedinice.URGENTNA:
                    unitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "UrgentnaPomoc", "bin", "Debug", "UrgentnaPomoc.exe");
                    break;
                case TipJedinice.DIJAGNOSTICKA:
                    unitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Dijagnostika", "bin", "Debug", "Dijagnostika.exe");
                    break;
                case TipJedinice.TERAPEUTSKA:
                    unitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Terapija", "bin", "Debug", "Terapija.exe");
                    break;
                default:
                    throw new ArgumentException($"Nepoznat tip jedinice: {tipJedinice}", nameof(tipJedinice));
            }

            if (!File.Exists(unitPath))
            {
                Console.WriteLine($"Ne mogu pronaći izvršni fajl na {unitPath}");
                return;
            }

            string workingDir = Path.GetDirectoryName(unitPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = unitPath,
                // Ako želiš da prosleđuješ argumente jedinici, dodaj ovde
                // Arguments = "1 7000", 
                WorkingDirectory = workingDir,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            Console.WriteLine($"Pokrenuta jedinica tipa {tipJedinice}.");
        }
    }
  
}