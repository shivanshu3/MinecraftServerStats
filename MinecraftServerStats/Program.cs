using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MinecraftServerStats
{
    class MinecraftUserStats
    {
        public TimeSpan PlayTime { get; set; }
        public long NumDeaths { get; set; }
    }

    class Program
    {
        private static readonly string StatsDirectory = @"C:\Users\shiva\Downloads\stats";
        private static readonly string UuidMapFile = @"C:\Users\shiva\Downloads\stats\uuid_map.txt";

        private static Dictionary<string, string> GetUuidUsernameMap()
        {
            return File.ReadAllLines(UuidMapFile)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToDictionary(line => line.Split(':')[0], line => line.Split(':')[1], StringComparer.OrdinalIgnoreCase);
        }

        private static dynamic ReadStatsFileAsJson(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject(jsonText);
        }

        private static MinecraftUserStats ParseStatsFile(string filePath)
        {
            var statsJson = ReadStatsFileAsJson(filePath);

            // Minecraft ticks to seconds
            var playTimeSec = (double) (statsJson.stats["minecraft:custom"]["minecraft:play_time"].Value) / 20;
            var numDeaths = statsJson.stats["minecraft:custom"]["minecraft:deaths"];

            var result = new MinecraftUserStats();
            result.PlayTime = TimeSpan.FromSeconds(playTimeSec);
            result.NumDeaths = numDeaths == null ? 0 : numDeaths.Value;

            return result;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Processing...");

            var uuidMap = GetUuidUsernameMap();
            var usernameStatsMap = new Dictionary<string, MinecraftUserStats>(StringComparer.OrdinalIgnoreCase);

            foreach (var (uuid, username) in uuidMap)
            {
                var statsFilePath = Path.Combine(StatsDirectory, uuid) + ".json";
                var stats = ParseStatsFile(statsFilePath);

                usernameStatsMap.Add(username, stats);
            }

            foreach (var (username, stats) in usernameStatsMap)
            {
                // var value = stats.PlayTime.TotalHours;
                var value = stats.NumDeaths;
                Console.WriteLine($"{username},{value}");
            }
        }
    }
}
