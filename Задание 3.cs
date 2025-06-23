using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{

namespace LogNormalizer
{
    class Program
    {
        
        // Формат 1:
        // 10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'
        private static readonly Regex Fmt1 =
            new Regex(@"^(?<date>\d{2}\.\d{2}\.\d{4})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d{3})\s+(?<lvl>[A-Z]+)\s+(?<msg>.+)$",
                RegexOptions.Compiled);

        // Формат 2:
        // 2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Код устройства: ...
        private static readonly Regex Fmt2 =
            new Regex(@"^(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d{1,7})\|\s*(?<lvl>[A-Z]+)\|\d+\|(?<meth>[^|]+)\|\s*(?<msg>.+)$",
                RegexOptions.Compiled);

            
            private static readonly Dictionary<string, string> LevelMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
            { "INFO", "INFO" },
            { "INFORMATION", "INFO" },
            { "WARN", "WARN" },
            { "WARNING", "WARN" },
            { "ERROR", "ERROR" },
            { "DEBUG", "DEBUG" }
};


            static void Main(string[] args)
            {
                string inFile = args.Length > 0 ? args[0] : "in.log";
                string outFile = args.Length > 1 ? args[1] : "out.log";
                string badFile = "problems.txt";

                if (!File.Exists(inFile))
                {
                    Console.Error.WriteLine($"Файл «{inFile}» не найден.");
                    return;
                }

                int goodCnt = 0, badCnt = 0;

                using (StreamWriter writer = new StreamWriter(outFile))
                using (StreamWriter bad = new StreamWriter(badFile))
                {
                    foreach (string line in File.ReadLines(inFile))
                    {
                        string normalized;
                        if (TryParse(line, out normalized))
                        {
                            writer.WriteLine(normalized);
                            goodCnt++;
                        }
                        else
                        {
                            bad.WriteLine(line);
                            badCnt++;
                        }
                    }
                }

                Console.WriteLine($"Готово. Корректных: {goodCnt}, проблемных: {badCnt}");
                Console.WriteLine($"→ {outFile}");
                if (badCnt > 0)
                    Console.WriteLine($"→ {badFile}");
            }

            private static bool TryParse(string line, out string result)
        {
            result = null;

            Match m;

            // ────────── Попытка 1 — формат 1 ──────────
            m = Fmt1.Match(line);
            if (m.Success)
            {
                var date   = DateTime.ParseExact(m.Groups["date"].Value, "dd.MM.yyyy", null)
                                   .ToString("yyyy-MM-dd");
                var time   = m.Groups["time"].Value;
                var level  = NormalizeLevel(m.Groups["lvl"].Value);
                var method = "DEFAULT";
                var msg    = m.Groups["msg"].Value.Trim();

                result = $"{date}\t{time}\t{level}\t{method}\t{msg}";
                return true;
            }

            // ────────── Попытка №2 — формат 2 ──────────
            m = Fmt2.Match(line);
            if (m.Success)
            {
                var date   = m.Groups["date"].Value; 
                var time   = m.Groups["time"].Value;
                var level  = NormalizeLevel(m.Groups["lvl"].Value);
                var method = m.Groups["meth"].Value.Trim();
                var msg    = m.Groups["msg"].Value.Trim();

                result = $"{date}\t{time}\t{level}\t{method}\t{msg}";
                return true;
            }

            return false; // не распознано
        }

        private static string NormalizeLevel(string raw)
            => LevelMap.TryGetValue(raw, out var std) ? std : raw.ToUpperInvariant();
    }
}

    
}
