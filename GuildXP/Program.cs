using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuildXP
{
    internal class Program
    {
        static void Main()
        {
            string baseDir = @"C:\Program Files (x86)\Steam\steamapps\common\Legion TD 2\Dev\Logs";

            // Find the newest directory
            var newestDirectory = new DirectoryInfo(baseDir)
                .GetDirectories()
                .OrderByDescending(d => d.LastWriteTime)
                .FirstOrDefault();

            if (newestDirectory == null)
            {
                Console.WriteLine("No directories found.");
                return;
            }

            // Find the newest HTML file in the directory
            var newestFile = newestDirectory
                .GetFiles("*.html")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (newestFile == null)
            {
                Console.WriteLine("No HTML files found.");
                return;
            }

            // Read the content of the file
            string fileContent = File.ReadAllText(newestFile.FullName);

            // Regex to find the content between { and } after "Found slowProps:"
            var slowPropsRegex = new Regex(@"Found slowProps:\s*({.*?})");

            // Extract matches and store them in a list
            var slowPropsList = slowPropsRegex.Matches(fileContent)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();


            var displayNameRegex = new Regex(@"""displayName"":""(.*?)""");


            var guildContributionRegex = new Regex(@"""guildContributionThisGuild"":(\d+)");


            foreach (var slowProp in slowPropsList)
            {
                var displayNameMatch = displayNameRegex.Match(slowProp);
                var guildContributionMatch = guildContributionRegex.Match(slowProp);

                if (displayNameMatch.Success)
                {
                    Console.WriteLine($"{displayNameMatch.Groups[1].Value}");
                }
                else
                {
                    Console.WriteLine("Display Name not found in slowProp.");
                }

                if (guildContributionMatch.Success)
                {
                    Console.WriteLine($"{guildContributionMatch.Groups[1].Value}");
                }
                else
                {
                    Console.WriteLine("Guild Contribution This Guild not found in slowProp.");
                }

                Console.WriteLine();
            }

            Console.ReadKey();
        }

    }
}
