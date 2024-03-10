using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Config;
using Discord.Webhook;
using Iks_Admin_Api;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace Iks_Admin_SocietyLogs;

public class Iks_Admin_SocietyLogs : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "Iks_Admin_SocietyLogs";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "iks";
    public override string ModuleDescription => "Logs for Iks_Admin =)";

    public static PluginCapability<IIks_Admin_Api> AdminApiCapability = new PluginCapability<IIks_Admin_Api>("iksadmin:core");

    IIks_Admin_Api? api;

    public static bool LogToVk;
    public static bool LogToDiscord;
    public static string DiscordWebHook;
    public static string VkToken;
    public static long VkChatID;
    public static IStringLocalizer loc;
    public PluginConfig Config {get; set;}
    
    public void OnConfigParsed(PluginConfig config)
    {
        config = ConfigManager.Load<PluginConfig>(ModuleName);
        loc = Localizer;
        LogToVk = config.LogToVk;
        LogToDiscord = config.LogToDiscord;
        DiscordWebHook = config.DiscordWebHook;
        VkToken = config.VkToken;
        VkChatID = config.VkChatID;
        
        Config = config;
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        api = AdminApiCapability.Get();
        if (api == null)
        {
            Logger.LogError("api not finded :(");
        }

        api!.OnAdminAdd += (admin, by) => {
            Console.WriteLine("adminadd log");
            string end = admin.End == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)admin.End);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnAdminAdd"].ToString()
                .Replace("{ByName}", api.GetAdminName(by))
                .Replace("{BySid}", by)
                .Replace("{name}", admin.Name)
                .Replace("{sid}", admin.SteamId)
                .Replace("{flags}", admin.Flags)
                .Replace("{immunity}", admin.Immunity.ToString())
                .Replace("{end}", end)
                .Replace("{group_name}", admin.GroupName)
                .Replace("{group_id}", admin.GroupId.ToString())
                .Replace("{server_id}", admin.ServerId)
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnAdminAdd"].ToString()
                .Replace("{ByName}", api.GetAdminName(by))
                .Replace("{BySid}", by)
                .Replace("{name}", admin.Name)
                .Replace("{sid}", admin.SteamId)
                .Replace("{flags}", admin.Flags)
                .Replace("{immunity}", admin.Immunity.ToString())
                .Replace("{end}", end)
                .Replace("{group_name}", admin.GroupName)
                .Replace("{group_id}", admin.GroupId.ToString())
                .Replace("{server_id}", admin.ServerId)
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnAdminDel += (admin, by) => {
            string end = admin.End == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)admin.End);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnAdminDel"].ToString()
                .Replace("{ByName}", api.GetAdminName(by))
                .Replace("{BySid}", by)
                .Replace("{name}", admin.Name)
                .Replace("{sid}", admin.SteamId)
                .Replace("{flags}", admin.Flags)
                .Replace("{immunity}", admin.Immunity.ToString())
                .Replace("{end}", end)
                .Replace("{group_name}", admin.GroupName)
                .Replace("{group_id}", admin.GroupId.ToString())
                .Replace("{server_id}", admin.ServerId)
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnAdminDel"].ToString()
                .Replace("{ByName}", api.GetAdminName(by))
                .Replace("{BySid}", by)
                .Replace("{name}", admin.Name)
                .Replace("{sid}", admin.SteamId)
                .Replace("{flags}", admin.Flags)
                .Replace("{immunity}", admin.Immunity.ToString())
                .Replace("{end}", end)
                .Replace("{group_name}", admin.GroupName)
                .Replace("{group_id}", admin.GroupId.ToString())
                .Replace("{server_id}", admin.ServerId)
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };

        api!.OnAddBan += (string sid, string adminsid, BannedPlayer ban) => {
            string end = ban.BanTime == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)ban.BanTimeEnd);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnAddBan"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{ip}", ban.Ip)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                .Replace("{banType}", ban.BanType.ToString())
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnAddBan"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{ip}", ban.Ip)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                .Replace("{banType}", ban.BanType.ToString())
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnAddGag += (string sid, string adminsid, BannedPlayer ban) => {
            Console.WriteLine("Addgag");
            string end = ban.BanTime == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)ban.BanTimeEnd);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnAddGag"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnAddGag"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnAddMute += (string sid, string adminsid, BannedPlayer ban) => {
            string end = ban.BanTime == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)ban.BanTimeEnd);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnAddMute"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnAddMute"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnUnGag += (string sid, string adminsid, BannedPlayer ban) => {
            string end = ban.BanTime == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)ban.BanTimeEnd);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnUnGag"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnUnGag"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnUnMute += (string sid, string adminsid, BannedPlayer ban) => {
            string end = ban.BanTime == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)ban.BanTimeEnd);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnUnMute"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnUnMute"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnUnBan += (string sid, string adminsid, BannedPlayer ban) => {
            string end = ban.BanTime == 0 ? "Never" : XHelper.GetDateStringFromUTC((ulong)ban.BanTimeEnd);
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnUnBan"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{ip}", ban.Ip)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                .Replace("{banType}", ban.BanType.ToString())
                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnUnBan"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", ban.Name)
                .Replace("{sid}", ban.Sid)
                .Replace("{ip}", ban.Ip)
                .Replace("{reason}", ban.BanReason)
                .Replace("{duration}", api.GetTimeKey(ban.BanTime / 60))
                .Replace("{end}", end)
                .Replace("{banType}", ban.BanType.ToString())
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnSlay += (CCSPlayerController? caller, CCSPlayerController target) => {
            string adminsid = caller == null ? "CONSOLE" : caller.SteamID.ToString();
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnSlay"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", target.PlayerName)
                .Replace("{sid}", target.SteamID.ToString())

                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnSlay"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", target.PlayerName)
                .Replace("{sid}", target.SteamID.ToString())
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnKick += (CCSPlayerController? caller, CCSPlayerController target, string reason) => {
            string adminsid = caller == null ? "CONSOLE" : caller.SteamID.ToString();
            if (LogToDiscord)
            {
                string dmessage = Localizer["DISCORD_OnKick"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", target.PlayerName)
                .Replace("{sid}", target.SteamID.ToString())
                .Replace("{reason}", reason)

                ;
                SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
            }
            if (LogToVk)
            {
                string dmessage = Localizer["VK_OnKick"].ToString()
                .Replace("{ByName}", api.GetAdminName(adminsid))
                .Replace("{BySid}", adminsid)
                .Replace("{name}", target.PlayerName)
                .Replace("{sid}", target.SteamID.ToString())
                .Replace("{reason}", reason)
                ;
                SocietyLogger.SendToVk(dmessage);
            }
        };
        api!.OnCmdUsage += (CCSPlayerController? caller, string command, string status, CCSPlayerController? target) => {
            var args = XHelper.GetArgsFromCommandLine(status);
            string adminsid = caller == null ? "CONSOLE" : caller.SteamID.ToString();
            if (command == "css_switchteam")
            {
                if (LogToDiscord)
                {
                    string dmessage = Localizer["DISCORD_SwitchTeam"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{name}", target.PlayerName)
                    .Replace("{sid}", target.SteamID.ToString())
                    .Replace("{oldTeam}", args[0])
                    .Replace("{newTeam}", args[1])

                    ;
                    SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
                }
                if (LogToVk)
                {
                    string dmessage = Localizer["VK_SwitchTeam"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{name}", target.PlayerName)
                    .Replace("{sid}", target.SteamID.ToString())
                    .Replace("{oldTeam}", args[0])
                    .Replace("{newTeam}", args[1])
                    ;
                    SocietyLogger.SendToVk(dmessage);
                }
                return;
            }
            if (command == "css_changeteam")
            {
                if (LogToDiscord)
                {
                    string dmessage = Localizer["DISCORD_ChangeTeam"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{name}", target.PlayerName)
                    .Replace("{sid}", target.SteamID.ToString())
                    .Replace("{oldTeam}", args[0])
                    .Replace("{newTeam}", args[1])

                    ;
                    SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
                }
                if (LogToVk)
                {
                    string dmessage = Localizer["VK_ChangeTeam"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{name}", target.PlayerName)
                    .Replace("{sid}", target.SteamID.ToString())
                    .Replace("{oldTeam}", args[0])
                    .Replace("{newTeam}", args[1])
                    ;
                    SocietyLogger.SendToVk(dmessage);
                }
                return;
            }
            if (target == null)
            {
                if (LogToDiscord)
                {
                    string dmessage = Localizer["DISCORD_Action"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{cmd}", command)
                    .Replace("{status}", status)
                    ;
                    SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
                }
                if (LogToVk)
                {
                    string dmessage = Localizer["VK_Action"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{cmd}", command)
                    .Replace("{status}", status)             
                    ;
                    SocietyLogger.SendToVk(dmessage);
                }
                return;
            }
            if (target != null)
            {
                if (LogToDiscord)
                {
                    string dmessage = Localizer["DISCORD_ActionWithTarget"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{cmd}", command)
                    .Replace("{status}", status)
                    .Replace("{targetName}", target.PlayerName)
                    .Replace("{targetSid}", target.SteamID.ToString())
                    ;
                    SocietyLogger.SendToDiscord(dmessage, new DColor(255, 0, 0));
                }
                if (LogToVk)
                {
                    string dmessage = Localizer["VK_ActionWithTarget"].ToString()
                    .Replace("{ByName}", api.GetAdminName(adminsid))
                    .Replace("{BySid}", adminsid)
                    .Replace("{cmd}", command)
                    .Replace("{status}", status)
                    .Replace("{targetName}", target.PlayerName)
                    .Replace("{targetSid}", target.SteamID.ToString())         
                    ;
                    SocietyLogger.SendToVk(dmessage);
                }
                return;
            }
        };
        Logger.LogInformation("SocietyLogs loaded");
    }

    

}

public static class SocietyLogger
{

    public static void SendToDiscord(string message, DColor? color = null)
    {
        color = color == null ? new DColor(255, 255, 255) : color;
        Task.Run(async () => {
            var webhookObject = new WebhookObject();
            webhookObject.AddEmbed(builder =>
            {
                builder.WithTitle(Iks_Admin_SocietyLogs.loc["DISCORD_Title"])
                    .WithColor(color)
                    .WithDescription(message);
            });
            await new Webhook(Iks_Admin_SocietyLogs.DiscordWebHook).SendAsync(webhookObject);
        });
    }

    public static void SendToVk(string message)
    {
        Task.Run(async () => {
            var apiAuthParams = new ApiAuthParams
            {
                AccessToken = Iks_Admin_SocietyLogs.VkToken,
                Settings = Settings.Messages
            };
            var api = new VkApi();
            await api.AuthorizeAsync(apiAuthParams);

            try
            {
                await api.Messages.SendAsync(new MessagesSendParams
                {
                    RandomId = new Random().Next(),
                    PeerId = Iks_Admin_SocietyLogs.VkChatID,
                    Message = message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Iks_Admin_SocietyLogs] Vk Message error: {ex.Message}");
            }
        });
    }
}
