using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Localization;

namespace Iks_Admin_Api;

public interface IIks_Admin_Api
{
    public List<Admin>? Admins { get; set; }
    public List<Admin>? AllAdmins { get; set; }
    public Dictionary<CCSPlayerController ,Action<CCSPlayerController, string>> NextSayActions { get; set; }

    public List<ModuleMenuOption>? BlocksMenuOptions { get; set; }
    public List<ModuleMenuOption>? PlayersMenuOptions { get; set; }
    public List<ModuleMenuOption>? ServerMenuOptions { get; set; }
    public List<ModuleMenuOption>? ModulesMenuOptions { get; set; }
    public List<ModuleMenuOption>? OtherMenuOptions { get; set; }
    public List<ModuleMenuOption>? BaseMenuOptions { get; set; }
    public string ServerId { get; }
    public string DbConnectionString { get; }
    public string ServerName { get; }
    public IStringLocalizer Localizer { get; }

    public List<BannedPlayer> OnlineMutedPlayers { get; set; }
    public List<BannedPlayer> OnlineGaggedPlayers { get; set; }
    
    public BasePlugin _plugin { get; }
    
    public void HaveNotAccess(CCSPlayerController controller);
    public void HaveNotAccess(CommandInfo info);

    public void SendMessage(string message);
    
    /// <summary>
    /// In Server.NextFrame
    /// </summary>
    public void SendMessage(string sid, string message);
    public void SendMessage(CCSPlayerController target, string  message);
    public void SendMessage(CommandInfo info, string  message);

    
    /// <summary>
    /// Return true if player have flag
    /// </summary>
    public bool HaveFlag(CCSPlayerController? controller, string flag, bool z = true);
    public bool HaveFlag(string steamid, string flag, bool z = true);
    public bool HaveOptionAccess(Admin admin, string option);
    
    /// <summary>
    /// Get Admin by Controller or Steamid
    /// </summary>
    public Admin? GetAdmin(CCSPlayerController controller);
    public Admin? GetAdmin(string steamid);

    /// <summary>
    /// Get Admins from database
    /// </summary>
    public Task<List<Admin>?> GetAllAdmins();
    public Task<Admin?> GetAdminBySid(string sid);
    public Task<List<Admin>?> GetServerAdmins();
    public void ReloadAdmins();
    public string GetAdminName(string adminsid);
    public string GetTimeKey(int time);
    
    // Base Functions
    public void KickPlayer(CCSPlayerController? admin, string sid, string reason);
    public Task AddAdmin(string adminsid, string sid, string name, string flags, int immunity, int group_id, long end, string server_id);
    public Task DelAdmin(string adminsid, string sid);
    public void AddBan(string name, string sid, string ip, string adminsid, int time, string reason, int banType = 0);
    public void AddMute(string name, string sid, string adminsid, int time, string reason);
    public void AddGag(string name, string sid, string adminsid, int time, string reason);
    public void UnBanPlayer(string arg, string adminsid);
    public void UnMutePlayer(string arg, string adminsid);
    public void UnGagPlayer(string arg, string adminsid);
    public Task<BannedPlayer?> IsBanned(string arg);
    public Task<BannedPlayer?> IsGagged(string arg);
    public Task<BannedPlayer?> IsMuted(string arg);

    
    // Base Functions event
    event Action<Admin, string>? OnAdminAdd; // Added admin, Added by sid
    event Action<Admin, string>? OnAdminDel; // Deleted admin, Deleted by sid
    event Action<string, string, BannedPlayer>? OnAddBan;
    event Action<string, string, BannedPlayer>? OnAddMute;
    event Action<string, string, BannedPlayer>? OnAddGag;
    event Action<string, string, BannedPlayer>? OnUnBan;
    event Action<string, string, BannedPlayer>? OnUnMute;
    event Action<string, string, BannedPlayer>? OnUnGag;

    event Action<CCSPlayerController?, CCSPlayerController>? OnSlay;
    event Action<CCSPlayerController?, CCSPlayerController, string>? OnKick;
    
    // Events
    event Action<CCSPlayerController>? OnMenuOpened;
    event Action<List<Admin>>? OnAdminsGet;
    event Action<List<Admin>>? OnAllAdminsGet;
    event Action? OnReady;
    
    event Action<string>? OnLogMessage;
    event Action<string>? OnSendMessageToServer;
    public void LogMessage(string message); 
    public void SendMessageToServer(string message); 
    
    /// <summary>
    /// Needs to handle command usage from modules and some commands like css_hide
    /// How to use it:
    /// CCSPlayerController -> caller
    /// string -> command string ( info.getCommandString )
    /// string -> status ( Some text action )
    /// CCSPlayerController -> target | null by default
    ///
    /// For example: css_hide
    /// if (hide on) -> CommandUsed(caller, info.getCommandString, HideOn)
    /// if (hide off) -> CommandUsed(caller, info.getCommandString, HideOff)
    ///
    /// pls use it like this ^^^
    /// </summary> 
    event Action<CCSPlayerController?, string, string, CCSPlayerController?> OnCmdUsage;

    public void CommandUsed(CCSPlayerController? caller, string command, string status, CCSPlayerController? target = null);
    
    event Action<BannedPlayer>? OnBannedPlayerConnected;
    event Action<BannedPlayer>? OnGaggedPlayerConnected;
    event Action<BannedPlayer>? OnMutedPlayerConnected;
    
    

}

public class Admin
{
    public string Name;
    public string SteamId;
    public string Flags;
    public int Immunity;
    public int End;
    public string GroupName;
    public int? GroupId;
    public string ServerId;

    public Admin(string name, string steamId, string flags, int immunity, int end, string groupName, int? groupId, string serverId) // For set Admin
    {
        Name = name;
        SteamId = steamId;
        Flags = flags;
        Immunity = immunity;
        End = end;
        GroupName = groupName;
        GroupId = groupId;
        ServerId = serverId;
    }
    
}

public class ControllerParams
{
    public string PlayerName;
    public string SteamID;
    public string IpAddress;


    public ControllerParams(string name, string sid, string ip)
    {
        PlayerName = name;
        SteamID = sid.ToString();
        IpAddress = ip;
    }
}

public abstract class ModuleMenuOption
{
    public Action<CCSPlayerController, ChatMenuOption> SelectAction { get; set; }
    public string OptionTitle { get; set; }
    public string OptionAccess { get; set; }
    public BasePlugin _plugin { get; set; }

    public ModuleMenuOption(
        BasePlugin plugin,
        string OptionAccess,
        string OptionTitle,
        Action<CCSPlayerController, ChatMenuOption> SelectAction
    )
    {
        _plugin = plugin;
        this.OptionTitle = OptionTitle;
        this.OptionAccess = OptionAccess;
        this.SelectAction = SelectAction;
    }
}

public class BannedPlayer
{
    public string Name = "";
    public string Sid = "";
    public string Ip = "";
    public string BanReason = "";
    public int BanCreated = 0;
    public int BanTime = 0;
    public int BanTimeEnd = 0;
    public string AdminSid = "";
    public bool Unbanned = false;
    public string UnbannedBy = "";
    public int BanType = 0;

    public BannedPlayer(string name, string sid, string ip, string banReason, int banCreated, int banTime, int banTimeEnd, string adminSid, int unbanned, string unbannedBy, int banType)
    {
        Name = name;
        Sid = sid;
        Ip = ip;
        BanReason = banReason;
        BanCreated = banCreated;
        BanTime = banTime;
        BanTimeEnd = banTimeEnd;
        AdminSid = adminSid;
        Unbanned = unbanned == 1;
        UnbannedBy = unbannedBy;
        BanType = banType;
    }
}