using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Iks_Admin;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("HaveIksChatColors")] public bool HaveIksChatColors { get; set; } = true;
    [JsonPropertyName("ServerId")] public string ServerId { get; set; } = "A"; // Просто указать букву СИНТАКСИС ВАЖЕН
    [JsonPropertyName("BanOnAllServers")] public bool BanOnAllServers { get; set; } = false; // Если false то бан будет только на том серевере где server_id как в бане
    [JsonPropertyName("ServerName")] public string ServerName { get; set; } = "Test Server"; // Название сервера
    [JsonPropertyName("Host")] public string Host { get; set; } = "localhost";
    [JsonPropertyName("Port")] public int Port { get; set; } = 3306;
    [JsonPropertyName("Name")] public string Name { get; set; } = "dbname";
    [JsonPropertyName("Login")] public string Login { get; set; } = "dblogin";
    [JsonPropertyName("Password")] public string Password { get; set; } = "dbpassword";

    [JsonPropertyName("KickReasons")] public string[] KickReasons { get; set; } = new[] {
        "Afk",
        "Admin ignore",
        "$Own Reason" // start with $
    };
    [JsonPropertyName("MuteReason")] public Reason[] MuteReason { get; set; } = new[] {
        new Reason("Sound pad", 0),
        new Reason("Voice mod", 0),
        new Reason("Other", null),
        new Reason("Own reason", -1)
    };
    [JsonPropertyName("GagReason")] public Reason[] GagReason { get; set; } = new[] {
        new Reason("Flood", 0),
        new Reason("Other", null),
        new Reason("Own reason", -1)
    };

    [JsonPropertyName("BanReasons")] public Reason[] BanReasons { get; set; } = new[]
    {
        new Reason("Cheats", 0),
        new Reason("BunnyHop", null),
        new Reason("Own reason", -1)
    };
    
    [JsonPropertyName("AdminMenuOptionsFlags")]
    public Dictionary<string, string> AdminMenuOptionsFlags { get; set; } = new()
    {
        // Если хотя бы один из флагов есть у админа, то пункт будет отображатся. Если доступ нужен для любого флага, посто удалите пункт из списка
        {"blocks", "bgmuz"}, 
        {"players", "skz"}, 
        {"adminManage" , "z"},

        {"ban", "bz"},
        {"unban" , "uz"},
        {"kick" , "kz"},
        {"mute" , "mz"},
        {"unmute" , "mz"},
        {"gag" , "gz"},
        {"switchteam" , "sz"},
        {"changeteam" , "sz"},
        {"ungag" , "gz"},
        {"slay" , "sz"}
    };
    [JsonPropertyName("Flags")]
    public Dictionary<string, string> Flags { get; set; } = new()
    {
        {"adminManage" , "z"},
        
        {"ban" , "b"},
        {"unban" , "u"},
        {"kick" , "k"},
        {"mute" , "m"},
        {"unmute" , "m"},
        {"gag" , "g"},
        {"ungag" , "g"},
        {"slay" , "s"},
        {"hide" , "s"},
        {"switchteam" , "s"},
        {"changeteam" , "s"}
    };

    // [JsonPropertyName("Times")] public int[] Times { get; set; } = new[] { 120, 60, 30, 15, 0 };
    [JsonPropertyName("Times")] public Dictionary<string, int> Times { get; set; } = new() { // [1.1.6]
        {"1 year", 525600},
        {"1 month", 43200},
        {"1 week", 10080},
        {"1 day", 1440},
        {"1 hour", 60},
        {"30 minutes", 30},
        {"Own time", -1}
    };

}