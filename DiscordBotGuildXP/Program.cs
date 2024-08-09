using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

class Program
{
    private static readonly string token = "YOUR_BOT_TOKEN_HERE";
    private static readonly string baseDir = @"C:\Program Files (x86)\Steam\steamapps\common\Legion TD 2\Dev\Logs";
    private static DiscordSocketClient client = new DiscordSocketClient();

    static async Task Main(string[] args)
    {
        client.Log += Log;
        client.Ready += Ready;

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Block the program until it is closed
        await Task.Delay(-1);
    }

    private static Task Log(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private static async Task Ready()
    {
        Console.WriteLine("Bot is connected.");

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

        // Regex to find the displayName value
        var displayNameRegex = new Regex(@"""displayName"":""(.*?)""");

        // Regex to find the guildContributionThisGuild value
        var guildContributionRegex = new Regex(@"""guildContributionThisGuild"":(\d+)");

        // Prepare the message to be sent
        var message = string.Empty;

        foreach (var slowProp in slowPropsList)
        {
            var displayNameMatch = displayNameRegex.Match(slowProp);
            var guildContributionMatch = guildContributionRegex.Match(slowProp);

            if (displayNameMatch.Success)
            {
                message += $"Display Name: {displayNameMatch.Groups[1].Value}\n";
            }
            else
            {
                message += "Display Name not found in slowProp.\n";
            }

            if (guildContributionMatch.Success)
            {
                message += $"Guild Contribution This Guild: {guildContributionMatch.Groups[1].Value}\n";
            }
            else
            {
                message += "Guild Contribution This Guild not found in slowProp.\n";
            }

            message += "\n"; // Add an empty line between different slowProps
        }

        // Send the message to a specific channel
        var channel = client.GetChannel(123456789/*"YOUR_CHANNEL_ID_HERE"*/) as IMessageChannel;

        if (channel != null)
        {
            await channel.SendMessageAsync(message);
        }
    }
}
