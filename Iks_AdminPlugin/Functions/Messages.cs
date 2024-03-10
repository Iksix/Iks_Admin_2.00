using CounterStrikeSharp.API.Core;
using Iks_Admin_Api;
using Microsoft.Extensions.Localization;

namespace Iks_Admin;

public class Messages
{
    private IStringLocalizer Localizer;
    private PluginConfig Config;
    private AdminApi AdminApi = Iks_Admin.adminApi;

    public string GetAdminName(string adminsid)
    {
        string adminName = adminsid.ToLower() == "console" ? "CONSOLE" :
            AdminApi.GetAdmin(adminsid) != null ? AdminApi.GetAdmin(adminsid).Name :
            XHelper.GetPlayerFromArg($"#{adminsid}") != null ? XHelper.GetPlayerFromArg($"#{adminsid}").PlayerName :
            "Undefined";
        return adminName;

    }

    public string GetTimeKey(int time)
    {
        var key = Iks_Admin.ConfigNow.Times.FirstOrDefault(x => x.Value == time).Key;
        if (key == null)
        {
            return $"{time}{AdminApi.Localizer["HELPER_Min"]}";
        }

        return key;
    }

    public Messages(IStringLocalizer localizer, PluginConfig config)
    {
        Config = config;
        Localizer = localizer;
    }
    
    public void SERVER_Kick(CCSPlayerController? admin, CCSPlayerController target, string reason)
    {
        string adminsid = admin == null ? "CONSOLE" : admin.SteamID.ToString();
        AdminApi.SendMessage(Localizer["SERVER_OnKick"].ToString()
            .Replace("{name}", target.PlayerName)
            .Replace("{sid}", target.SteamID.ToString())
            .Replace("{reason}", reason)
            .Replace("{adminsid}", adminsid)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    public void SERVER_Slay(CCSPlayerController? admin, CCSPlayerController target)
    {
        string adminsid = admin == null ? "CONSOLE" : admin.SteamID.ToString();
        AdminApi.SendMessage(Localizer["SERVER_OnSlay"].ToString()
            .Replace("{name}", target.PlayerName)
            .Replace("{sid}", target.SteamID.ToString())
            .Replace("{adminsid}", adminsid)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }

    public void SERVER_Ban(string sid, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(Localizer["SERVER_OnBan"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{ip}", ban.Ip)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{banType}", ban.BanType.ToString())
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    public void SERVER_Gag(string sid, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(Localizer["SERVER_OnGag"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    public void SERVER_Mute(string sid, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(Localizer["SERVER_OnMute"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    
    public void SERVER_UnGag(string arg, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(Localizer["SERVER_OnUnGag"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    public void SERVER_UnMute(string arg, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(Localizer["SERVER_OnUnMute"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    
    public void SERVER_UnBan(string arg, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(Localizer["SERVER_OnUnBan"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{ip}", ban.Ip)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{banType}", ban.BanType.ToString())
            .Replace("{admin}", GetAdminName(adminsid))
            .Replace("{bannedBy}", GetAdminName(ban.AdminSid))
            .Replace("{bannedBySid}", ban.AdminSid)
        );
    }
    
    public void NOTIFY_Ban(string sid, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnBan"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{ip}", ban.Ip)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{banType}", ban.BanType.ToString())
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    
    public void NOTIFY_Gag(string sid, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnGag"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    public void NOTIFY_Mute(string sid, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnMute"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
        );
    }
    public void NOTIFY_UnGag(string arg, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnUnGag"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
            .Replace("{bannedBy}", GetAdminName(ban.AdminSid))
            .Replace("{bannedBySid}", ban.AdminSid)
        );
    }
    public void NOTIFY_UnMute(string arg, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnUnMute"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{admin}", GetAdminName(adminsid))
            .Replace("{bannedBy}", GetAdminName(ban.AdminSid))
            .Replace("{bannedBySid}", ban.AdminSid)
        );
    }
    
    public void NOTIFY_UnBan(string arg, string adminsid, BannedPlayer ban)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnUnBan"].ToString()
            .Replace("{name}", ban.Name)
            .Replace("{sid}", ban.Sid)
            .Replace("{ip}", ban.Ip)
            .Replace("{adminsid}", adminsid)
            .Replace("{duration}", GetTimeKey(ban.BanTime))
            .Replace("{reason}", ban.BanReason)
            .Replace("{banType}", ban.BanType.ToString())
            .Replace("{admin}", GetAdminName(adminsid))
            .Replace("{bannedBy}", GetAdminName(ban.AdminSid))
            .Replace("{bannedBySid}", ban.AdminSid)
        );
    }
    
    public void NOTIFY_OnAdminAdd(Admin admin, string adminsid)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnAdminAdd"].ToString()
            .Replace("{admin}", admin.Name)
            .Replace("{sid}", admin.SteamId)
            .Replace("{flags}", admin.Flags)
            .Replace("{server_id}", admin.ServerId)
            .Replace("{group_id}", admin.GroupId.ToString())
            .Replace("{group_name}", admin.GroupName)
            .Replace("{addedByName}", GetAdminName(adminsid))
            .Replace("{addedBySid}", adminsid)
        );
    }
    public void NOTIFY_OnAdminDel(Admin admin, string adminsid)
    {
        AdminApi.SendMessage(adminsid, Localizer["NOTIFY_OnAdminDel"].ToString()
            .Replace("{admin}", admin.Name)
            .Replace("{sid}", admin.SteamId)
            .Replace("{flags}", admin.Flags)
            .Replace("{server_id}", admin.ServerId)
            .Replace("{group_id}", admin.GroupId.ToString())
            .Replace("{group_name}", admin.GroupName)
            .Replace("{deletedByName}", GetAdminName(adminsid))
            .Replace("{deletedBySid}", adminsid)
        );
    }

}