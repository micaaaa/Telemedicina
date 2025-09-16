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
            string processName;

            switch (tipJedinice)
            {
                case TipJedinice.URGENTNA:
                    unitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "UrgentnaPomoc", "bin", "Debug", "UrgentnaPomoc.exe");
                    processName = "UrgentnaPomoc"; 
                    break;
                case TipJedinice.DIJAGNOSTICKA:
                    unitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Dijagnostika", "bin", "Debug", "Dijagnostika.exe");
                    processName = "Dijagnostika";
                    break;
                case TipJedinice.TERAPEUTSKA:
                    unitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Terapija", "bin", "Debug", "Terapija.exe");
                    processName = "Terapija";
                    break;
                default:
                    throw new ArgumentException($"Nepoznat tip jedinice: {tipJedinice}", nameof(tipJedinice));
            }

            if (Process.GetProcessesByName(processName).Length > 0)
            {
                Console.WriteLine($"Jedinica {tipJedinice} je već pokrenuta.");
                return;
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
                WorkingDirectory = workingDir,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            Console.WriteLine($"Pokrenuta jedinica tipa {tipJedinice}.");
        }
    }
  
}