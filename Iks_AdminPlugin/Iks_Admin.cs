using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Iks_Admin_Api;
using Iks_Admin.Commands;
using Iks_Admin.Menus;
using Microsoft.Extensions.Localization;
using MySqlConnector;
using Microsoft.Extensions.Logging;

namespace Iks_Admin;

public class Iks_Admin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "Iks_Admin";
    public override string ModuleVersion => "2.0";
    public override string ModuleAuthor => "iks";

    // This instance
    public static AdminApi adminApi = null!;
    public static PluginCapability<IIks_Admin_Api> _pluginCapability { get; } =
        new PluginCapability<IIks_Admin_Api>("iksadmin:core");

    public static BaseCommands BaseCommand;
    public static Messages Messages;
    public static AdminMenu AdminMenu;
    public static BaseFuntions BaseFunc;
    public static List<ControllerParams> OfflinePlayers = new ();
    public static List<ControllerParams> OnlinePlayers = new ();
    
    public override void Load(bool hotReload)
    {
        
        
        //Register commands
        
        AddCommand("css_admin", "open admin menu", BaseCommand.OnAdminCmd);
        AddCommand("css_adminadd", "add admin", BaseCommand.OnAdminaddCmd);
        AddCommand("css_admindel", "open admin menu", BaseCommand.OnAdmindelCmd);
        
        AddCommand("css_slay", "slay the player", BaseCommand.OnSlayCmd);
        AddCommand("css_ban", "ban the player", BaseCommand.OnBanCmd);
        AddCommand("css_unban", "unban the player", BaseCommand.OnUnBanCmd);
        AddCommand("css_gag", "gag the player", BaseCommand.OnGagCmd);
        AddCommand("css_ungag", "ungag the player", BaseCommand.OnUnGagCmd);
        AddCommand("css_mute", "mute the player", BaseCommand.OnMuteCmd);
        AddCommand("css_unmute", "unmute the player", BaseCommand.OnUnMuteCmd);
        AddCommand("css_hide", "hide the admin", BaseCommand.OnHideCmd);
        AddCommand("css_kick", "kick the player", BaseCommand.OnKickCmd);
        AddCommand("css_changeteam", "Change player team", BaseCommand.OnChangeteamCmd);
        AddCommand("css_switchteam", "Switch team player", BaseCommand.OnSwitchteamCmd);
        
        AddCommand("css_reload_admins", "reload admin list", (player, info) =>
        {
            if (adminApi.HaveFlag(player, "z"))
            {
                adminApi.ReloadAdmins();
            }
        });
        
        AddCommand("css_reload_infractions", "---", (player, info) =>
        {
            if (adminApi.HaveFlag(player, "z"))
            {
                string arg = info.GetArg(1);
                Task.Run(async () =>
                {
                    await ReloadPlayerInfractions(arg);
                });
            }
        });

        //Register Messages
        
        adminApi.OnAddBan += Messages.SERVER_Ban;
        adminApi.OnAddBan += Messages.NOTIFY_Ban;
        adminApi.OnUnBan += Messages.SERVER_UnBan;
        adminApi.OnUnBan += Messages.NOTIFY_UnBan;

        adminApi.OnAddGag += Messages.NOTIFY_Gag;
        adminApi.OnAddGag += Messages.SERVER_Gag;
        adminApi.OnUnGag += Messages.NOTIFY_UnGag;
        adminApi.OnUnGag += Messages.SERVER_UnGag;
        
        adminApi.OnAddMute += Messages.NOTIFY_Mute;
        adminApi.OnAddMute += Messages.SERVER_Mute;
        adminApi.OnUnMute += Messages.NOTIFY_UnMute;
        adminApi.OnUnMute += Messages.SERVER_UnMute;
        
        adminApi.OnKick += Messages.SERVER_Kick;
        adminApi.OnSlay += Messages.SERVER_Slay;
        
        adminApi.OnAdminAdd += Messages.NOTIFY_OnAdminAdd;
        adminApi.OnAdminDel += Messages.NOTIFY_OnAdminDel;
        
        
        // Other
        AddCommandListener("say", OnSay);
        AddCommandListener("say_team", OnSay);
        
        // Set and delete punishments
        AddTimer(3, () =>
        {
            var muted = adminApi.OnlineMutedPlayers;
            var gagged = adminApi.OnlineGaggedPlayers;
            var admins = adminApi.Admins;
            if (admins == null) return;

            foreach (var admin in admins!.ToList())
            {
                if (admin.End != 0 && admin.End < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    adminApi.Admins!.Remove(admin);
                    Logger.LogInformation($"{admin} has been removed from Admins (Not DB)!");
                    Logger.LogInformation("reason: ENDTIME");
                }
            }

            foreach (var mute in muted.ToList())
            {
                if (mute.BanTimeEnd < DateTimeOffset.UtcNow.ToUnixTimeSeconds() && mute.BanTime != 0)
                {
                    adminApi.OnlineMutedPlayers.Remove(mute);
                }
            }
            
            foreach (var gag in gagged.ToList())
            {
                if (gag.BanTimeEnd < DateTimeOffset.UtcNow.ToUnixTimeSeconds() && gag.BanTime != 0)
                {
                    adminApi.OnlineGaggedPlayers.Remove(gag);
                }
            }

            var players = XHelper.GetOnlinePlayers();
            foreach (var player in players)
            {
                if (!XHelper.IsControllerValid(player)) continue;
                if (adminApi.OnlineMutedPlayers.FirstOrDefault(x => x.Sid == player.SteamID.ToString()) != null)
                {
                    player.VoiceFlags = VoiceFlags.Muted;
                }
                else
                {
                    player.VoiceFlags = VoiceFlags.Normal;
                }
            }
            
            
        }, TimerFlags.REPEAT);
    }
    
    
    public HookResult OnSay(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null) return HookResult.Continue;
        var isGagged = adminApi.OnlineGaggedPlayers.FirstOrDefault(x => x.Sid == controller.SteamID.ToString());

        if (adminApi.NextSayActions.ContainsKey(controller) && info.GetArg(1).StartsWith("!"))
        {
            var action = adminApi.NextSayActions[controller];
            action.Invoke(controller, info.GetArg(1).Remove(0, 1));
            adminApi.NextSayActions.Remove(controller);
        }
        
        if (isGagged != null)
        {
            if (info.GetArg(1).StartsWith("!"))
            {
                controller.ExecuteClientCommandFromServer(info.GetArg(1).Remove(0, 1));
                controller.ExecuteClientCommandFromServer("css_" +info.GetArg(1).Remove(0, 1));
                controller.ExecuteClientCommandFromServer("mm_" +info.GetArg(1).Remove(0, 1));
                
                return HookResult.Handled;
            }
            adminApi.SendMessage(controller, Localizer["TARGET_ChatWhenGagged"]);

            return HookResult.Handled;
        }
        return HookResult.Continue;
    }

    public PluginConfig Config { get; set; }
    public static PluginConfig ConfigNow;

    public void OnConfigParsed(PluginConfig config)
    {
        config = ConfigManager.Load<PluginConfig>("Iks_Admin");
        Config = config;
        ConfigNow = config;
        adminApi = new AdminApi(Config, Localizer, this);
        AdminMenu = new AdminMenu(Localizer);
        BaseCommand = new BaseCommands(Localizer, Config, adminApi);
        Messages = new Messages(Localizer, Config);
        BaseFunc = new BaseFuntions(Localizer);
        adminApi.OnAdminsGet += (admins) =>
        {
            Logger.LogInformation("Admins Getted!");
        };
        Capabilities.RegisterPluginCapability(_pluginCapability, () => adminApi);
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        adminApi.Ready();
        Logger.LogInformation("Plugin Ready!");
    }

    public async Task ReloadPlayerInfractions(string sid)
    {
        var banned = await adminApi.IsBanned(sid);
        if (banned != null)
        {
            BaseFuntions.KickPlayer(sid);
            await adminApi.IsBannedPlayerConnected(sid);
            Logger.LogInformation($"Block connection!");
            Logger.LogInformation($"Name: {banned.Name}");
            Logger.LogInformation($"Steamid: {banned.Sid}");
            Logger.LogInformation($"Reason: {banned.BanReason}");
            Logger.LogInformation($"Created: {banned.BanCreated}");
            Logger.LogInformation($"BannedBy: {banned.AdminSid}");
        }

        var gagged = await adminApi.IsGagged(sid);
        if (gagged != null)
        {
            if (OnlinePlayers.FirstOrDefault(x => x.SteamID == sid) != null)
                adminApi.OnlineGaggedPlayers.Add(gagged);
        }
            
        var muted = await adminApi.IsMuted(sid);
        if (muted != null) 
        {
            if (OnlinePlayers.FirstOrDefault(x => x.SteamID == sid) != null)
                adminApi.OnlineMutedPlayers.Add(muted);
        }

        var adminOnServer = adminApi.AllAdmins!.FirstOrDefault(x => x.SteamId == sid);
        var adminOnServer2 = adminApi.Admins!.FirstOrDefault(x => x.SteamId == sid);
        if (adminOnServer != null)
        {
            adminApi.AllAdmins!.Remove(adminOnServer);
            adminApi.Admins!.Remove(adminOnServer2);
        }

        var adminFromDB = await adminApi.GetAdminBySid(sid);
        if (adminFromDB == null) return;
        adminApi.AllAdmins!.Add(adminFromDB);
        if (adminFromDB.ServerId.Contains(Config.ServerId) || adminFromDB.ServerId.Trim() == "")
        {
            adminApi.Admins!.Add(adminFromDB);
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerConnected(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!XHelper.IsControllerValid(player)) return HookResult.Continue;
        string sid = player.SteamID.ToString();
        var cp = XHelper.GetControllerParams(player);
        OfflinePlayers.Remove(cp);
        OnlinePlayers.Add(cp);
        Task.Run(async () =>
        {
            await ReloadPlayerInfractions(sid);
        });
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerChangeTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        var player = @event.Userid;
        BaseCommand.HidenPlayers.Remove(player);
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!XHelper.IsControllerValid(player)) return HookResult.Continue;
        string sid = player.SteamID.ToString();
        var cp = XHelper.GetControllerParams(player);
        BaseCommand.HidenPlayers.Remove(player);
        OfflinePlayers.Add(cp);
        OnlinePlayers.Remove(cp);
        if (OfflinePlayers.Count > 50)
        {
            OfflinePlayers.RemoveAt(1);
        }
        
        var gagged = adminApi.OnlineGaggedPlayers.FirstOrDefault(x => x.Sid == sid);
        if (gagged != null)
        {
            adminApi.OnlineGaggedPlayers.Remove(gagged);
        }
        
        var muted = adminApi.OnlineMutedPlayers.FirstOrDefault(x => x.Sid == sid);
        if (muted != null)
        {
            adminApi.OnlineMutedPlayers.Remove(muted);
        }

        return HookResult.Continue;
    }
    
   
}

public class AdminApi : IIks_Admin_Api
{
    public List<Admin>? Admins { get; set; }
    public List<Admin>? AllAdmins { get; set; }
    
    /// <summary>
    /// Нужно для меню с возможностью ввести свои параметры
    /// </summary>
    public Dictionary<CCSPlayerController, Action<CCSPlayerController, string>> NextSayActions { get; set; } = new();
    public List<ModuleMenuOption>? BlocksMenuOptions { get; set; } = new List<ModuleMenuOption>();
    public List<ModuleMenuOption>? PlayersMenuOptions { get; set; } = new List<ModuleMenuOption>();
    public List<ModuleMenuOption>? ServerMenuOptions { get; set; } = new List<ModuleMenuOption>();
    public List<ModuleMenuOption>? ModulesMenuOptions { get; set; } = new List<ModuleMenuOption>();
    public List<ModuleMenuOption>? OtherMenuOptions { get; set; } = new List<ModuleMenuOption>();
    public List<ModuleMenuOption>? BaseMenuOptions { get; set; } = new List<ModuleMenuOption>();
    public IStringLocalizer Localizer { get; }
    public List<BannedPlayer> OnlineMutedPlayers { get; set; } = new List<BannedPlayer>();
    public List<BannedPlayer> OnlineGaggedPlayers { get; set; } = new List<BannedPlayer>();
    public PluginConfig Config;
    public BasePlugin _plugin { get; }

    public string ServerId { get; }
    public string DbConnectionString { get; }
    public string ServerName { get; }
    public AdminApi(PluginConfig config, IStringLocalizer localizer, BasePlugin plugin)
    {
        _plugin = plugin;
        Config = config;
        Localizer = localizer;
        ServerId = config.ServerId;
        ServerName = config.ServerName;
        DbConnectionString = "Server=" + config.Host + ";Database=" + config.Name
                             + ";port=" + config.Port + ";User Id=" + config.Login + ";password=" + config.Password;
        
        string sql =
            "CREATE TABLE IF NOT EXISTS `iks_admins` ( `id` INT NOT NULL AUTO_INCREMENT , `sid` VARCHAR(32) NOT NULL , `name` VARCHAR(32) NOT NULL , `flags` VARCHAR(32) NOT NULL , `immunity` INT NOT NULL, `group_id` INT NOT NULL DEFAULT '-1' ,`end` INT NOT NULL , `server_id` VARCHAR(64) NOT NULL , PRIMARY KEY (`id`)) ENGINE = InnoDB;";
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                connection.Open();
                var comm = new MySqlCommand(sql, connection);
                comm.ExecuteNonQuery();
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        sql = "CREATE TABLE IF NOT EXISTS `iks_bans` ( `id` INT NOT NULL AUTO_INCREMENT , `name` VARCHAR(32) NOT NULL ,`sid` VARCHAR(32) NOT NULL, `ip` VARCHAR(32) NULL , `adminsid` VARCHAR(32) NOT NULL , `created` INT NOT NULL , `time` INT NOT NULL , `end` INT NOT NULL , `reason` VARCHAR(255) NOT NULL, `BanType` INT(1) NOT NULL DEFAULT '0', `Unbanned` INT(1) NOT NULL DEFAULT '0', `UnbannedBy` VARCHAR(32) NULL , `server_id` VARCHAR(1) NOT NULL DEFAULT '', PRIMARY KEY (`id`)) ENGINE = InnoDB;";
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                connection.Open();
                var comm = new MySqlCommand(sql, connection);
                comm.ExecuteNonQuery();
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        sql = "CREATE TABLE IF NOT EXISTS `iks_mutes` ( `id` INT NOT NULL AUTO_INCREMENT , `name` VARCHAR(32) NOT NULL , `sid` VARCHAR(32) NOT NULL , `adminsid` VARCHAR(32) NOT NULL , `created` INT NOT NULL , `time` INT NOT NULL , `end` INT NOT NULL , `reason` VARCHAR(255) NOT NULL, `Unbanned` INT(1) NOT NULL DEFAULT '0', `UnbannedBy` VARCHAR(32) NULL, `server_id` VARCHAR(1) NOT NULL DEFAULT '', PRIMARY KEY (`id`)) ENGINE = InnoDB;";
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                connection.Open();
                var comm = new MySqlCommand(sql, connection);
                comm.ExecuteNonQuery();
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        sql = "CREATE TABLE IF NOT EXISTS `iks_gags` ( `id` INT NOT NULL AUTO_INCREMENT , `name` VARCHAR(32) NOT NULL , `sid` VARCHAR(32) NOT NULL , `adminsid` VARCHAR(32) NOT NULL , `created` INT NOT NULL , `time` INT NOT NULL , `end` INT NOT NULL , `reason` VARCHAR(255) NOT NULL, `Unbanned` INT(1) NOT NULL DEFAULT '0', `UnbannedBy` VARCHAR(32) NULL , `server_id` VARCHAR(1) NOT NULL DEFAULT '', PRIMARY KEY (`id`)) ENGINE = InnoDB;";
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                connection.Open();
                var comm = new MySqlCommand(sql, connection);
                comm.ExecuteNonQuery();
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }
        sql = "CREATE TABLE IF NOT EXISTS `iks_groups` ( `id` INT NOT NULL AUTO_INCREMENT , `flags` VARCHAR(32) NOT NULL , `name` VARCHAR(32) NOT NULL , `immunity` INT NOT NULL , PRIMARY KEY (`id`)) ENGINE = InnoDB;";
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                connection.Open();
                var comm = new MySqlCommand(sql, connection);
                comm.ExecuteNonQuery();
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        ReloadAdmins();
    }
    
    public void HaveNotAccess(CCSPlayerController controller)
    {
        controller.PrintToChat($" {Localizer["PluginTag"]} {Localizer["HaveNotAccess"]}");
    }

    public void HaveNotAccess(CommandInfo info)
    {
        info.ReplyToCommand($" {Localizer["PluginTag"]} {Localizer["NOTIFY_HaveNotAccess"]}");
    }

    public void SendMessage(string message)
    {
        Server.NextFrame(() =>
        {
            foreach (var str in XHelper.SeparateString(message))
            {
                Server.PrintToChatAll($" {Localizer["PluginTag"]} {str}");
            }
        });
    }

    public void SendMessage(string sid, string message)
    {
        Server.NextFrame(() =>
        {
            var player = XHelper.GetPlayerFromArg("#" + sid);
            foreach (var str in XHelper.SeparateString(message))
            {
                if (player == null)
                {
                    _plugin.Logger.LogInformation(str);
                } else
                    player.PrintToChat($" {Localizer["PluginTag"]} {str}");
            }
        });
    }

    public void SendMessage(CCSPlayerController target, string message)
    {
        foreach (var str in XHelper.SeparateString(message))
        {
            target.PrintToChat($" {Localizer["PluginTag"]} {str}");
        }
    }

    public void SendMessage(CommandInfo info, string message)
    {
        foreach (var str in XHelper.SeparateString(message))
        {
            info.ReplyToCommand($" {Localizer["PluginTag"]} {str}");
        }
    }
    public void SendMessage(CommandInfo info,CCSPlayerController? controller, string message)
    {
        foreach (var str in XHelper.SeparateString(message))
        {
            if (controller == null)
            {
                _plugin.Logger.LogInformation(str);
                continue;
            }
            info.ReplyToCommand($" {Localizer["PluginTag"]} {str}");
        }
    }
    public bool HaveFlag(CCSPlayerController? controller, string flag, bool z = true)
    {
        if (controller == null) return true;
        var admin = GetAdmin(controller);
        if (admin == null) return false;
        if (!Iks_Admin.ConfigNow.Flags.ContainsKey(flag)) return true;
        if (z) return admin.Flags.Contains(Iks_Admin.ConfigNow.Flags[flag]) || admin.Flags.Contains("z");
        return admin.Flags.Contains(Iks_Admin.ConfigNow.Flags[flag]);
    }

    public bool HaveFlag(string steamid, string flag, bool z = true)
    {
        var admin = GetAdmin(steamid);
        if (admin == null) return false;
        if (z) return admin.Flags.Contains(flag) || admin.Flags.Contains("z");
        return admin.Flags.Contains(flag);
    }

    public bool HaveOptionAccess(Admin admin, string option)
    {
        PluginConfig cfg = Iks_Admin.ConfigNow;
        if (!cfg.AdminMenuOptionsFlags.ContainsKey(option))
        {
            return true;
        }
        
        foreach (var cflag in admin.Flags.ToCharArray())
        {
            string flag = cflag.ToString();
            if (cfg.AdminMenuOptionsFlags[option].Contains(flag))
            {
                return true;
            }
            
        }

        return false;
    }

    public Admin? GetAdmin(CCSPlayerController controller)
    {
        return Admins.FirstOrDefault(x => x.SteamId == controller.SteamID.ToString());
    }

    public Admin? GetAdmin(string steamid)
    {
        return Admins.FirstOrDefault(x => x.SteamId == steamid);
    }

    public async Task<Admin?> GetAdminBySid(string sid)
    {
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("start getting");
                string sql = @$"SELECT * FROM iks_admins WHERE sid='{sid}'";
                var comm = new MySqlCommand(sql, connection);
                var res = await comm.ExecuteReaderAsync();
                string name = "";
                sid = "";
                string flags = "";
                int immunity = -1;
                int end = 0;
                int? group_id = null;
                string group_name = "";
                string server = "A";
                while (await res.ReadAsync())
                {
                    name = res.GetString("name");
                    server = res.GetString("server_id");
                    sid = res.GetString("sid");
                    flags = res.GetString("flags");
                    immunity = res.GetInt32("immunity");
                    end = res.GetInt32("end");
                    group_id = res.GetInt32("group_id");
                    if (group_id != -1)
                    {
                        try
                        {
                            using (var connection2 = new MySqlConnection(DbConnectionString))
                            {
                                connection2.Open();
                                string sql2 = $"SELECT * FROM iks_groups WHERE id={group_id}";
                                var comm2 = new MySqlCommand(sql2, connection2);

                                var res2 = await comm2.ExecuteReaderAsync();
                                Console.WriteLine("4");

                                while (await res2.ReadAsync())
                                {
                                    group_name = res2.GetString("name");
                                    if (flags.Trim() == "")
                                    {
                                        flags = res2.GetString("flags");
                                    }
                                    if (immunity == -1)
                                    {
                                        immunity = res2.GetInt32("immunity");
                                    }
                                }
                            }
                        }
                        catch (MySqlException ex)
                        {
                            _plugin.Logger.LogError($"DB ERROR: {ex}");
                        }
                    }
                    Admin admin = new Admin(name, sid, flags, immunity, end, group_name, group_id, server);
                    return admin;
                }
            }

        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        return null;
    }

    public async Task<List<Admin>?> GetServerAdmins()
    {
        List<Admin>? admins = new List<Admin>();

        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {

                await connection.OpenAsync();
                string sql = @"SELECT * FROM iks_admins";
                var comm = new MySqlCommand(sql, connection);
                var res = await comm.ExecuteReaderAsync();
                string name = "";
                string sid = "";
                string flags = "";
                int immunity = -1;
                int end = 0;
                int? group_id = null;
                string group_name = "";
                string server = "A";

                while (await res.ReadAsync())
                {
                    name = res.GetString("name");
                    server = res.GetString("server_id");
                    sid = res.GetString("sid");
                    flags = res.GetString("flags");
                    immunity = res.GetInt32("immunity");
                    end = res.GetInt32("end");
                    group_id = res.GetInt32("group_id");
                    if (group_id != -1)
                    {
                        try
                        {
                            using (var connection2 = new MySqlConnection(DbConnectionString))
                            {
                                connection2.Open();
                                string sql2 = $"SELECT * FROM iks_groups WHERE id={group_id}";
                                var comm2 = new MySqlCommand(sql2, connection2);

                                var res2 = await comm2.ExecuteReaderAsync();

                                while (await res2.ReadAsync())
                                {
                                    group_name = res2.GetString("name");
                                    if (flags.Trim() == "")
                                    {
                                        flags = res2.GetString("flags");
                                    }
                                    if (immunity == -1)
                                    {
                                        immunity = res2.GetInt32("immunity");
                                    }
                                }


                            }
                        }
                        catch (MySqlException ex)
                        {
                            _plugin.Logger.LogError($"DB ERROR: {ex}");
                        }
                    }
                    Admin admin = new Admin(name, sid, flags, immunity, end, group_name, group_id, server);
                    if (admin.ServerId.Contains(ServerId) || admin.ServerId.Trim() == "")
                    {
                        admins.Add(admin);
                    }
                }
            }

        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        return admins;
    }
    
    public async Task<List<Admin>?> GetAllAdmins()
    {
        List<Admin>? admins = new List<Admin>();

        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {

                await connection.OpenAsync();
                string sql = @"SELECT * FROM iks_admins";
                var comm = new MySqlCommand(sql, connection);
                var res = await comm.ExecuteReaderAsync();
                string name = "";
                string sid = "";
                string flags = "";
                int immunity = -1;
                int end = 0;
                int? group_id = null;
                string group_name = "";
                string server = "A";

                while (await res.ReadAsync())
                {
                    name = res.GetString("name");
                    server = res.GetString("server_id");
                    sid = res.GetString("sid");
                    flags = res.GetString("flags");
                    immunity = res.GetInt32("immunity");
                    end = res.GetInt32("end");
                    group_id = res.GetInt32("group_id");
                    if (group_id != -1)
                    {
                        try
                        {
                            using (var connection2 = new MySqlConnection(DbConnectionString))
                            {
                                connection2.Open();
                                string sql2 = $"SELECT * FROM iks_groups WHERE id={group_id}";
                                var comm2 = new MySqlCommand(sql2, connection2);

                                var res2 = await comm2.ExecuteReaderAsync();

                                while (await res2.ReadAsync())
                                {
                                    group_name = res2.GetString("name");
                                    if (flags.Trim() == "")
                                    {
                                        flags = res2.GetString("flags");
                                    }
                                    if (immunity == -1)
                                    {
                                        immunity = res2.GetInt32("immunity");
                                    }
                                }
                            }
                        }
                        catch (MySqlException ex)
                        {
                            _plugin.Logger.LogError($"DB ERROR: {ex}");
                        }
                    }
                    Admin admin = new Admin(name, sid, flags, immunity, end, group_name, group_id, server);
                    admins.Add(admin);
                }
            }

        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        return admins;
    }
    public void ReloadAdmins()
    {
        Task.Run(async () =>
        {
            Admins = await GetServerAdmins();
            OnAdminsGet?.Invoke(Admins);
            AllAdmins = await GetAllAdmins();
            OnAllAdminsGet?.Invoke(AllAdmins!);
        });
    }

    public void KickPlayer(CCSPlayerController? admin, string sid, string reason)
    {
        Server.NextFrame(() =>
        {
            var target = XHelper.GetPlayerFromArg($"#{sid}");
            if (target == null) return;
            Server.ExecuteCommand("kickid " + target.UserId);
            OnKick?.Invoke(admin, target, reason);
        });
    }

    public async Task AddAdmin(string adminsid, string sid, string name, string flags, int immunity, int group_id, long end,
        string server_id)
    {

        var AdminExists = await GetAdminBySid(sid);
        if (AdminExists != null)
        {
            await DelAdmin(adminsid, sid);
        }
        name = XHelper.RemoveDangerSimbols(name);
        string group_name = "Undefined";
        using (var connection2 = new MySqlConnection(DbConnectionString))
        {
            connection2.Open();
            string sql2 = $"SELECT * FROM iks_groups WHERE id={group_id}";
            var comm2 = new MySqlCommand(sql2, connection2);

            var res2 = await comm2.ExecuteReaderAsync();

            while (await res2.ReadAsync())
            {
                group_name = res2.GetString("name");
                if (flags.Trim() == "")
                {
                    flags = res2.GetString("flags");
                }
                if (immunity == -1)
                {
                    immunity = res2.GetInt32("immunity");
                }
            }
        }
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                await connection.OpenAsync();
                string sql = $@"INSERT INTO iks_admins (`sid`, `name`, `flags`, `immunity`, `group_id`, `end`, `server_id`) VALUES ('{sid}', '{name}', '{flags}', '{immunity}', '{group_id}', '{end}', '{server_id}')";
                var comm = new MySqlCommand(sql, connection);

                await comm.ExecuteNonQueryAsync();
                var admin = new Admin(name, sid, flags, immunity, (int)end, group_name, group_id, server_id);
                OnAdminAdd?.Invoke(admin, adminsid);
                ReloadAdmins();
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }
        
    }

    public async Task DelAdmin(string adminsid, string sid)
    {
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                await connection.OpenAsync();
                string sql = $@"DELETE FROM iks_admins WHERE sid='{sid}'";
                var comm = new MySqlCommand(sql, connection);
                var admin = await GetAdminBySid(sid);
                

                await comm.ExecuteNonQueryAsync();
                if (admin != null)
                {
                    _plugin.Logger.LogInformation($"Admin {sid} has been deleted by {adminsid}");
                    OnAdminDel?.Invoke(admin, adminsid);
                }
                ReloadAdmins();
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }
    }


    public void AddBan(string name, string sid, string ip, string adminsid, int time, string reason, int banType = 0)
    {
        Task.Run(async () =>
        {
            var ban = await IsBanned(sid);

            if (ban != null)
            {
                SendMessage(adminsid, "Player alredy banned");
                return;
            }
            
            reason = XHelper.RemoveDangerSimbols(reason);
            name = XHelper.RemoveDangerSimbols(name);
            sid = sid.Replace("#", "");
            if (ip.Split(":").Length > 0)
                ip = ip.Split(":")[0];
            try
            {
                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    await connection.OpenAsync();
                    string sql = $"INSERT INTO iks_bans (`name`, `sid`, `ip`, `adminsid`, `created`, `time`, `end`, `reason`) VALUES ('{name}', '{sid}', '{ip}', '{adminsid}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}', '{time * 60}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60}', '{reason}')";

                    if (Config != null)
                    {
                        if (!Config.BanOnAllServers)
                        {
                            sql = $"INSERT INTO iks_bans (`name`, `sid`, `ip`, `adminsid`, `created`, `time`, `end`, `reason`, `server_id`) VALUES ('{name}', '{sid}', '{ip}', '{adminsid}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}', '{time * 60}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60}', '{reason}', '{Config.ServerId}')";
                        }
                    }


                    var comm = new MySqlCommand(sql, connection);

                    await comm.ExecuteNonQueryAsync();

                    var NewBan = await IsBanned(sid);
                    OnAddBan?.Invoke(sid, adminsid, NewBan);
                }
            }
            catch (MySqlException ex)
            {
                _plugin.Logger.LogError($"DB ERROR: {ex}");
            }
        });

        var player = XHelper.GetPlayerFromArg($"#{sid}");
        if (player != null)
        {
            SendMessage($"kickid {player.UserId}");
            Server.ExecuteCommand($"kickid {player.UserId}");
        }
    }

    public void AddMute(string name, string sid, string adminsid, int time, string reason)
    {
        Task.Run(async () =>
        {
            reason = XHelper.RemoveDangerSimbols(reason);
            name = XHelper.RemoveDangerSimbols(name);
            sid = sid.Replace("#", "");
            var ban = await IsMuted(sid);

            if (ban != null)
            {
                SendMessage(adminsid, "Player alredy muted!");
                return;
            }
            try
            {
                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    await connection.OpenAsync();
                    string sql = $"INSERT INTO iks_mutes (`name`, `sid`, `adminsid`, `created`, `time`, `end`, `reason`) VALUES ('{name}', '{sid}', '{adminsid}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}', '{time * 60}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60}', '{reason}')";

                    if (Config != null)
                    {
                        if (!Config.BanOnAllServers)
                        {
                            sql = $"INSERT INTO iks_mutes (`name`, `sid`, `adminsid`, `created`, `time`, `end`, `reason`, `server_id`) VALUES ('{name}', '{sid}', '{adminsid}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}', '{time * 60}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60}', '{reason}', '{Config.ServerId}')";
                        }
                    }

                    var comm = new MySqlCommand(sql, connection);

                    await comm.ExecuteNonQueryAsync();
                    var nowBan = new BannedPlayer(name, sid, "", reason, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(), time * 60,
                        (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60), adminsid, 0, "", 0);
                    if (Iks_Admin.OnlinePlayers.FirstOrDefault(x => x.SteamID == sid) != null)
                    {
                        OnlineMutedPlayers.Add(nowBan);
                    }

                    var NewMute = await IsMuted(sid);
                    OnAddMute?.Invoke(sid, adminsid, NewMute);
                }
            }
            catch (MySqlException ex)
            {
                _plugin.Logger.LogError($"DB ERROR: {ex}");
            }
        });
    }

    public void AddGag(string name, string sid, string adminsid, int time, string reason)
    {
        Task.Run(async () =>
        {
            reason = XHelper.RemoveDangerSimbols(reason);
            name = XHelper.RemoveDangerSimbols(name);
            sid = sid.Replace("#", "");
            var ban = await IsGagged(sid);

            if (ban != null)
            {
                SendMessage(adminsid, "Player alredy gagged!");
                return;
            }
            try
            {
                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    await connection.OpenAsync();
                    string sql = $"INSERT INTO iks_gags (`name`, `sid`, `adminsid`, `created`, `time`, `end`, `reason`) VALUES ('{name}', '{sid}', '{adminsid}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}', '{time * 60}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60}', '{reason}')";

                    if (Config != null)
                    {
                        if (!Config.BanOnAllServers)
                        {
                            sql = $"INSERT INTO iks_gags (`name`, `sid`, `adminsid`, `created`, `time`, `end`, `reason`, `server_id`) VALUES ('{name}', '{sid}', '{adminsid}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}', '{time * 60}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60}', '{reason}', '{Config.ServerId}')";
                        }
                    }

                    var comm = new MySqlCommand(sql, connection);

                    await comm.ExecuteNonQueryAsync();

                    var nowBan = new BannedPlayer(name, sid, "", reason, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(), time * 60,
                        (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60), adminsid, 0, "", 0);
                    if (Iks_Admin.OnlinePlayers.FirstOrDefault(x => x.SteamID == sid) != null)
                    {
                        OnlineGaggedPlayers.Add(nowBan);
                    }
                    var NewMute = await IsGagged(sid);
                    OnAddGag?.Invoke(sid, adminsid, NewMute);
                }
            }
            catch (MySqlException ex)
            {
                _plugin.Logger.LogError($"DB ERROR: {ex}");
            }
        });
    }

    public void UnBanPlayer(string arg, string adminsid)
    {
        Task.Run(async () =>
        {
            var ban = await IsBanned(arg);

            if (ban == null)
            {
                SendMessage(adminsid, "Player not banned!");
                return;
            }
            
            arg = arg.Replace("#", "");
            if (arg == null || arg.ToLower() == "undefined")
            {
                return;
            }
            try
            {
                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    await connection.OpenAsync();
                    string sql = $"UPDATE iks_bans SET `Unbanned` = 1, `UnbannedBy` = '{adminsid}' WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND `Unbanned` = 0";
                    if (Config != null)
                    {
                        if (!Config.BanOnAllServers)
                        {
                            sql = $"UPDATE iks_bans SET `Unbanned` = 1, `UnbannedBy` = '{adminsid}' WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND `Unbanned` = 0 AND `server_id`='{Config.ServerId}'";
                        }
                    }
                    var comm = new MySqlCommand(sql, connection);

                    await comm.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                _plugin.Logger.LogError($"DB ERROR: {ex}");
            }
            try
            {
                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    await connection.OpenAsync();
                    string sql = $"UPDATE iks_bans SET `Unbanned` = 1, `UnbannedBy` = '{adminsid}' WHERE ip='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned = 0 AND BanType=1";
                    if (Config != null)
                    {
                        if (!Config.BanOnAllServers)
                        {
                            sql = $"UPDATE iks_bans SET `Unbanned` = 1, `UnbannedBy` = '{adminsid}' WHERE ip='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned = 0 AND BanType=1 AND `server_id`='{Config.ServerId}'";
                        }
                    }
                    var comm = new MySqlCommand(sql, connection);

                    await comm.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                _plugin.Logger.LogError($"DB ERROR: {ex}");
            }
            OnUnBan?.Invoke(arg, adminsid, ban);
        });
    }
    
    public string GetAdminName(string adminsid)
    {
        string adminName = adminsid.ToLower() == "console" ? "CONSOLE" :
            GetAdmin(adminsid) != null ? GetAdmin(adminsid).Name :
            XHelper.GetPlayerFromArg($"#{adminsid}") != null ? XHelper.GetPlayerFromArg($"#{adminsid}").PlayerName :
            "Undefined";
        return adminName;

    }
    
    public string GetTimeKey(int time)
    {
        var key = Iks_Admin.ConfigNow.Times.FirstOrDefault(x => x.Value == time).Key;
        if (key == null)
        {
            return $"{time}{Localizer["HELPER_Min"]}";
        }

        return key;
    }

    public void UnMutePlayer(string arg, string adminsid)
    {
        Task.Run(async () =>
        {
            string sid = arg.Replace("#", "");
            
            var ban = await IsMuted(arg);

            if (ban == null)
            {
                SendMessage(adminsid, "Player not muted!");
                return;
            }
            
            try
            {
                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    await connection.OpenAsync();
                    string sql = $"UPDATE iks_mutes SET Unbanned=1, UnbannedBy='{adminsid}' WHERE sid='{sid}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND `Unbanned` = 0";

                    if (Config != null)
                    {
                        if (!Config.BanOnAllServers)
                        {
                            sql = $"UPDATE iks_mutes SET Unbanned=1, UnbannedBy='{adminsid}' WHERE sid='{sid}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND `Unbanned` = 0 AND `server_id` = '{Config.ServerId}'";
                        }
                    }

                    var comm = new MySqlCommand(sql, connection);

                    await comm.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                _plugin.Logger.LogError($"DB ERROR: {ex}");
            }
            OnUnMute?.Invoke(arg, adminsid, ban);
            var getGag = OnlineMutedPlayers.FirstOrDefault(x => x.Sid == ban.Sid);

            if (getGag != null)
            {
                OnlineMutedPlayers.Remove(getGag);
                _plugin.Logger.LogInformation($"Unmute player {getGag.Name}");
            }
        });
    }

    public void UnGagPlayer(string arg, string adminsid)
    {
        Task.Run(async () =>
        {
            string sid = arg.Replace("#", "");
            
            var ban = await IsGagged(arg);

            if (ban == null)
            {
                SendMessage(adminsid, "Player not gagged!");
                return;
            }
            
            try
            {
                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    await connection.OpenAsync();
                    string sql = $"UPDATE iks_gags SET `Unbanned` = 1, `UnbannedBy` = '{adminsid}' WHERE sid='{sid}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND `Unbanned` = 0";

                    if (Config != null)
                    {
                        if (!Config.BanOnAllServers)
                        {
                            sql = $"UPDATE iks_gags SET Unbanned=1, UnbannedBy='{adminsid}' WHERE sid='{sid}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND `Unbanned` = 0 AND `server_id` = '{Config.ServerId}'";
                        }
                    }

                    var comm = new MySqlCommand(sql, connection);

                    await comm.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                _plugin.Logger.LogError($"DB ERROR: {ex}");
            }
            OnUnGag?.Invoke(arg, adminsid, ban);

            var getGag = OnlineGaggedPlayers.FirstOrDefault(x => x.Sid == ban.Sid);

            if (getGag != null)
            {
                OnlineGaggedPlayers.Remove(getGag);
                _plugin.Logger.LogInformation($"Ungag player {getGag.Name}");
            }
        });
    }

    public async Task<BannedPlayer?> IsBanned(string arg)
    {
        if (arg == null || arg.ToLower() == "undefined")
        {
            return null;
        }
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                await connection.OpenAsync();
                string sql = $"SELECT * FROM iks_bans WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned=0";

                if (Config != null)
                {
                    if (!Config.BanOnAllServers)
                    {
                        sql = $"SELECT * FROM iks_bans WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned=0 AND server_id='{Config.ServerId}'";
                    }
                }

                var comm = new MySqlCommand(sql, connection);
                MySqlDataReader reader = await comm.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string Name = reader.GetString("name");
                    string Sid = reader.GetString("sid");
                    string Ip = reader.GetString("ip");
                    string BanReason = reader.GetString("reason");
                    int BanCreated = reader.GetInt32("created");
                    int BanTime = reader.GetInt32("time");
                    int BanTimeEnd = reader.GetInt32("end");
                    string AdminSid = reader.GetString("adminsid");
                    int Unbanned = reader.GetInt32("Unbanned");
                    int BanType = reader.GetInt32("BanType");
                    string UnbannedBy = "";
                    if (Unbanned == 1)
                    {
                        UnbannedBy = reader.GetString("UnbannedBy");
                    }
                    BannedPlayer player = new BannedPlayer(
                        Name,
                        Sid,
                        Ip,
                        BanReason,
                        BanCreated,
                        BanTime,
                        BanTimeEnd,
                        AdminSid,
                        Unbanned,
                        UnbannedBy,
                        BanType
                        );


                    return player;
                }
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                await connection.OpenAsync();
                string sql = $"SELECT * FROM iks_bans WHERE ip='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned=0";
                var comm = new MySqlCommand(sql, connection);
                MySqlDataReader reader = await comm.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string Name = reader.GetString("name");
                    string Sid = reader.GetString("sid");
                    string Ip = reader.GetString("ip");
                    string BanReason = reader.GetString("reason");
                    int BanCreated = reader.GetInt32("created");
                    int BanTime = reader.GetInt32("time");
                    int BanTimeEnd = reader.GetInt32("end");
                    string AdminSid = reader.GetString("adminsid");
                    int Unbanned = reader.GetInt32("Unbanned");
                    int BanType = reader.GetInt32("BanType");
                    string UnbannedBy = "";
                    if (Unbanned == 1)
                    {
                        UnbannedBy = reader.GetString("UnbannedBy");
                    }
                    BannedPlayer player = new BannedPlayer(
                        Name,
                        Sid,
                        Ip,
                        BanReason,
                        BanCreated,
                        BanTime,
                        BanTimeEnd,
                        AdminSid,
                        Unbanned,
                        UnbannedBy,
                        BanType
                        );

                    return player;
                }
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        return null;
    }

    public async Task<BannedPlayer?> IsGagged(string arg)
    {
        if (arg == null || arg.ToLower() == "undefined")
        {
            return null;
        }
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                await connection.OpenAsync();
                string sql = $"SELECT * FROM iks_gags WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned=0";

                if (Config != null)
                {
                    if (!Config.BanOnAllServers)
                    {
                        sql = $"SELECT * FROM iks_gags WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned=0 AND server_id='{Config.ServerId}'";
                    }
                }

                var comm = new MySqlCommand(sql, connection);
                MySqlDataReader reader = await comm.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string Name = reader.GetString("name");
                    string Sid = reader.GetString("sid");
                    string Ip = "Undefined";
                    string BanReason = reader.GetString("reason");
                    int BanCreated = reader.GetInt32("created");
                    int BanTime = reader.GetInt32("time");
                    int BanTimeEnd = reader.GetInt32("end");
                    string AdminSid = reader.GetString("adminsid");
                    int Unbanned = reader.GetInt32("Unbanned");
                    int BanType = 0;
                    string UnbannedBy = "";
                    if (Unbanned == 1)
                    {
                        UnbannedBy = reader.GetString("UnbannedBy");
                    }

                    BannedPlayer player = new BannedPlayer(
                        Name,
                        Sid,
                        Ip,
                        BanReason,
                        BanCreated,
                        BanTime,
                        BanTimeEnd,
                        AdminSid,
                        Unbanned,
                        UnbannedBy,
                        BanType
                        );
                    return player;
                }
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        return null;
    }

    public async Task<BannedPlayer?> IsMuted(string arg)
    {
        if (arg == null || arg.ToLower() == "undefined")
        {
            return null;
        }
        try
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                await connection.OpenAsync();
                string sql = $"SELECT * FROM iks_mutes WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned=0";

                if (Config != null)
                {
                    if (!Config.BanOnAllServers)
                    {
                        sql = $"SELECT * FROM iks_mutes WHERE sid='{arg}' AND (end>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} OR time=0) AND Unbanned=0 AND server_id='{Config.ServerId}'";
                    }
                }

                var comm = new MySqlCommand(sql, connection);
                MySqlDataReader reader = await comm.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string Name = reader.GetString("name");
                    string Sid = reader.GetString("sid");
                    string Ip = "Undefined";
                    string BanReason = reader.GetString("reason");
                    int BanCreated = reader.GetInt32("created");
                    int BanTime = reader.GetInt32("time");
                    int BanTimeEnd = reader.GetInt32("end");
                    string AdminSid = reader.GetString("adminsid");
                    int Unbanned = reader.GetInt32("Unbanned");
                    int BanType = 0;
                    string UnbannedBy = "";
                    if (Unbanned == 1)
                    {
                        UnbannedBy = reader.GetString("UnbannedBy");
                    }
                    BannedPlayer player = new BannedPlayer(
                        Name,
                        Sid,
                        Ip,
                        BanReason,
                        BanCreated,
                        BanTime,
                        BanTimeEnd,
                        AdminSid,
                        Unbanned,
                        UnbannedBy,
                        BanType
                        );

                    return player;
                }
            }
        }
        catch (MySqlException ex)
        {
            _plugin.Logger.LogError($"DB ERROR: {ex}");
        }

        return null;
    }


    public event Action<Admin, string>? OnAdminAdd;
    public event Action<Admin, string>? OnAdminDel;
    public event Action<string, string, BannedPlayer>? OnAddBan;

    public event Action<string, string, BannedPlayer>? OnAddMute;
    public event Action<string, string, BannedPlayer>? OnAddGag;
    public event Action<string, string, BannedPlayer>? OnUnBan;
    public event Action<string, string, BannedPlayer>? OnUnMute;
    public event Action<string, string, BannedPlayer>? OnUnGag;

    public event Action<CCSPlayerController?, CCSPlayerController, string>? OnKick;
    public event Action<CCSPlayerController>? OnMenuOpened;
    public event Action<CCSPlayerController?, CCSPlayerController>? OnSlay;
    public event Action<List<Admin>?>? OnAdminsGet;
    public event Action<List<Admin>>? OnAllAdminsGet;
    public event Action? OnReady;
    public event Action<string>? OnLogMessage;
    public event Action<string>? OnSendMessageToServer;
    public void LogMessage(string message)
    {
        _plugin.Logger.LogInformation(message);
        OnLogMessage?.Invoke(message);
    }

    public void SendMessageToServer(string message)
    {
        foreach (var str in XHelper.SeparateString(message))
        {
            Server.PrintToChatAll($" {Localizer["PluginTag"]} {str}");
        }
        OnSendMessageToServer?.Invoke(message);
    }

    public event Action<CCSPlayerController?, string, string, CCSPlayerController?>? OnCmdUsage;

    public void CommandUsed(CCSPlayerController? caller, string command, string status, CCSPlayerController? target = null)
    {
        OnCmdUsage?.Invoke(caller, command, status, target);
    }

    public event Action<BannedPlayer>? OnBannedPlayerConnected;
    public event Action<BannedPlayer>? OnGaggedPlayerConnected;
    public event Action<BannedPlayer>? OnMutedPlayerConnected;

    public async Task<BannedPlayer?> IsBannedPlayerConnected(string steamid)
    {
        var ban = await IsBanned(steamid);
        if (ban != null) OnBannedPlayerConnected?.Invoke(ban);
        return ban;
    }
    public async Task<BannedPlayer?> IsGaggedPlayerConnected(string steamid)
    {
        var ban = await IsGagged(steamid);
        if (ban != null) OnGaggedPlayerConnected?.Invoke(ban);
        return ban;
    }
    public async Task<BannedPlayer?> IsMutedPlayerConnected(string steamid)
    {
        var ban = await IsMuted(steamid);
        if (ban != null) OnMutedPlayerConnected?.Invoke(ban);
        return ban;
    }
    public void Ready()
    {
        OnReady?.Invoke();
    }
    
    public void Slay(CCSPlayerController? admin, CCSPlayerController target)
    {
        OnSlay?.Invoke(admin, target);
    }

}
