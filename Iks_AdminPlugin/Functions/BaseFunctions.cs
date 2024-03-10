using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Localization;

namespace Iks_Admin;

public class BaseFuntions
{
    private IStringLocalizer Localizer;
    public BaseFuntions(IStringLocalizer localizer)
    {
        Localizer = localizer;
    }
    public static AdminApi AdminApi = Iks_Admin.adminApi;
    public void SlayPlayer(CCSPlayerController? admin, CCSPlayerController target, CommandInfo? info = null)
    {
        target.CommitSuicide(true, true);
        AdminApi.Slay(admin, target);
    }
    
    public void BanPlayer(string name, string sid, string ip, string adminsid, int time, string reason, int banType = 0)
    {
        AdminApi.AddBan(name, sid, ip, adminsid, time, reason, banType);
    }
    public void GagPlayer(string name, string sid, string adminsid, int time, string reason)
    {
        AdminApi.AddGag(name, sid, adminsid, time, reason);
    }
    public void MutePlayer(string name, string sid, string adminsid, int time, string reason)
    {
        AdminApi.AddMute(name, sid, adminsid, time, reason);
    }
    
    public void UnBanPlayer(string sid, string adminsid)
    {
        AdminApi.UnBanPlayer(sid, adminsid);
    }
    public void UnGagPlayer(string sid, string adminsid)
    {
        AdminApi.UnGagPlayer(sid, adminsid);
    }
    public void UnMutePlayer(string sid, string adminsid)
    {
        AdminApi.UnMutePlayer(sid, adminsid);
    }


    public static void KickPlayer(string sid)
    {
        Server.NextFrame(() =>
        {
            var target = XHelper.GetPlayerFromArg($"#{sid}");
            if (target == null) return;
            Server.ExecuteCommand("kickid " + target.UserId);
        });
    }

}